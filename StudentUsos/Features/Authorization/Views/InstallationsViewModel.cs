using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentUsos.Features.Authorization.Models;
using StudentUsos.Features.Authorization.Services;

namespace StudentUsos.Features.Authorization.Views;

public partial class InstallationsViewModel : BaseViewModel, INavigableWithResult<UsosInstallation>
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

    public TaskCompletionSource<UsosInstallation?> TaskCompletionSource { get; set; } = new();

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
        await navigationService.PushAsync<LoginResultPage, UsosInstallation>(installation);
    }

    [ObservableProperty]
    string editorText;

    [RelayCommand]
    void TextChanged()
    {
        Installations = UsosInstallation.FuzzySearch(originalInstallations, EditorText).ToList();
    }
}
