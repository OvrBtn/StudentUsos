using CommunityToolkit.Mvvm.Input;
using StudentUsos.Features.Authorization.Views;
using StudentUsos.Services.ServerConnection;

namespace StudentUsos.Features.Authorization;

public partial class LoginViewModel : BaseViewModel
{
    public Command LoginCommand { get; }
    public Command LoginWithPinCommand { get; }

    IServerConnectionManager serverConnectionManager;
    ILocalDatabaseManager localDatabaseManager;
    ILocalStorageManager localStorageManager;
    INavigationService navigationService;
    public LoginViewModel(IServerConnectionManager serverConnectionManager,
        ILocalDatabaseManager localDatabaseManager,
        ILocalStorageManager localStorageManager,
        INavigationService navigationService)
    {
        this.serverConnectionManager = serverConnectionManager;
        this.localDatabaseManager = localDatabaseManager;
        this.localStorageManager = localStorageManager;
        this.navigationService = navigationService;
    }

    void SwitchToDefaultServices()
    {
        if (serverConnectionManager is SwitchableServerConnectionManager manager)
        {
            manager.SwitchImplementation(App.ServiceProvider.GetService<ServerConnectionManager>()!);
        }
        DateAndTimeProvider.SwitchProvider(new DefaultDateAndTimeProvider());
    }

    [RelayCommand]
    async Task OnLoginClickedAsync()
    {
        SwitchToDefaultServices();

        await navigationService.PushAsync<InstallationsPage>();
    }

    [RelayCommand]
    async Task SignInAsGuest()
    {
        if (serverConnectionManager is SwitchableServerConnectionManager manager)
        {
            manager.SwitchImplementation(App.ServiceProvider.GetService<GuestServerConnectionManager>()!);
        }
        DateAndTimeProvider.SwitchProvider(new GuestDateAndTimeProvider());
        localStorageManager.SetBool(LocalStorageKeys.IsGuestMode, true);

        //create new tables from scratch or
        //drop all tables and then regenerate them in case there is something left from guest mode
        localDatabaseManager.ResetTables();
        await Shell.Current.GoToAsync("//DashboardPage");
    }
}