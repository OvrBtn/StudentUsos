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

    public TaskCompletionSource<UsosInstallation?> TaskCompletionSource { get; set; } = new();

    public async Task InitAsync()
    {
        var result = await usosInstallationsService.GetUsosInstallationsAsync();
        if (result is null)
        {
            MainStateKey = StateKey.ConnectionError;
            return;
        }

        Installations = result.OrderBy(x => x.Name).ToList();
        MainStateKey = StateKey.Loaded;
    }

    [RelayCommand]
    async Task InstallationClicked(UsosInstallation installation)
    {
        await navigationService.PushAsync<LoginResultPage, UsosInstallation>(installation);
    }
}
