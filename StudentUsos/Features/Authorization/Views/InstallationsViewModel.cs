using CommunityToolkit.Mvvm.ComponentModel;
using StudentUsos.Features.Authorization.Models;
using StudentUsos.Features.Authorization.Services;

namespace StudentUsos.Features.Authorization.Views;

public partial class InstallationsViewModel : BaseViewModel
{
    IUsosInstallationsService usosInstallationsService;
    public InstallationsViewModel(IUsosInstallationsService usosInstallationsService)
    {
        this.usosInstallationsService = usosInstallationsService;
    }

    [ObservableProperty]
    string mainStateKey = StateKey.Loading;

    [ObservableProperty]
    List<UsosInstallation> installations = new();

    public async Task InitAsync()
    {
        var result = await usosInstallationsService.GetUsosInstallationsAsync();
        if (result is null)
        {
            MainStateKey = StateKey.ConnectionError;
            return;
        }

        Installations = result;
        MainStateKey = StateKey.Loaded;
    }
}
