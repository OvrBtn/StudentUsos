using CommunityToolkit.Mvvm.Input;
using StudentUsos.Features.Settings.Views.Subpages;

namespace StudentUsos.Features.Settings.Views;

public partial class SettingsViewModel : BaseViewModel
{
    INavigationService navigationService;
    public SettingsViewModel(INavigationService navigationService)
    {
        this.navigationService = navigationService;
    }

    [RelayCommand]
    async Task GoToNotificationsSubpageAsync()
    {
        await navigationService.PushAsync<NotificationsSubpage>();
    }

    [RelayCommand]
    async Task GoToAccountSubpageAsync()
    {
        await navigationService.PushAsync<AccountSubpage>();
    }

    [RelayCommand]
    async Task GoToApplicationSubpageAsync()
    {
        await navigationService.PushAsync<ApplicationSubpage>();
    }

}