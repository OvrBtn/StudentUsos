using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentUsos.Features.Authorization.Models;
using StudentUsos.Features.Authorization.Services;

namespace StudentUsos.Features.Authorization.Views;

public partial class InstallationsViewModel : BaseViewModel, INavigableWithParameter<AuthorizationService.Mode>
{
    IUsosInstallationsService usosInstallationsService;
    INavigationService navigationService;
    public InstallationsViewModel(IUsosInstallationsService usosInstallationsService, INavigationService navigationService)
    {
        this.usosInstallationsService = usosInstallationsService;
        this.navigationService = navigationService;
    }

    [ObservableProperty]
    string mainStateKey = StateKey.Loading;

    [ObservableProperty]
    List<UsosInstallation> installations = new();
    List<UsosInstallation> originalInstallations = new();

    public async Task InitAsync()
    {
        if (usosInstallationsService is UsosInstallationsService service && service.UsosInstallationsCache is not null)
        {
            Installations = service.UsosInstallationsCache.OrderBy(x => x.Name).ToList();
            originalInstallations = Installations;
            MainStateKey = StateKey.Loaded;
        }

        var result = await usosInstallationsService.GetUsosInstallationsAsync();
        if (result is null)
        {
            if (MainStateKey == StateKey.Loading)
            {
                MainStateKey = StateKey.ConnectionError;
            }
            return;
        }

        result = result.OrderBy(x => x.Name).ToList();

        bool areDifferent = false;
        if (Installations.Count == result.Count)
        {
            for (int i = 0; i < result.Count; i++)
            {
                if (Installations[i].Equals(result[i]) == false)
                {
                    areDifferent = true;
                }
            }
        }
        else
        {
            areDifferent = true;
        }

        if (Installations.Count != 0 && areDifferent == false)
        {
            return;
        }

        Installations = result;
        originalInstallations = Installations;
        MainStateKey = StateKey.Loaded;
    }

    [RelayCommand]
    async Task InstallationClicked(UsosInstallation installation)
    {
        await navigationService.PushAsync<LoginResultPage, LoginResultParameters>(new(installation, suggestedDefaultMode));
    }

    [ObservableProperty]
    string editorText;

    [RelayCommand]
    void TextChanged()
    {
        Installations = FuzzySearch(originalInstallations, EditorText).ToList();
    }

    public static IEnumerable<UsosInstallation> FuzzySearch(IEnumerable<UsosInstallation> installations, string query, double threshold = 0.45)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return installations;
        }

        string normalizedQuery = FuzzyStringComparer.Normalize(query);

        return installations
            .Select(i => new
            {
                Installation = i,
                Score = i.SearchTokens
                    .Max(token =>
                    {
                        string normalizedToken = FuzzyStringComparer.Normalize(token);

                        if (normalizedToken.Equals(normalizedQuery))
                            return 1.2;

                        if (normalizedToken.StartsWith(normalizedQuery))
                            return 1.1;

                        if (normalizedToken.Contains(normalizedQuery))
                            return 1.0;

                        return FuzzyStringComparer.SimilarityScore(normalizedQuery, normalizedToken);
                    })
            })
            .Where(x => x.Score >= threshold)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Installation.Name)
            .Select(x => x.Installation);
    }

    AuthorizationService.Mode suggestedDefaultMode = AuthorizationService.Mode.RedirectWithCallback;

    void INavigableWithParameter<AuthorizationService.Mode>.OnNavigated(AuthorizationService.Mode navigationParameter)
    {
        suggestedDefaultMode = navigationParameter;
    }
}
