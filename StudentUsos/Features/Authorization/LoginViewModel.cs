using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentUsos.Features.Authorization.Services;
using StudentUsos.Features.Authorization.Views;
using StudentUsos.Services.ServerConnection;

namespace StudentUsos.Features.Authorization;

public partial class LoginViewModel : BaseViewModel
{
    IServerConnectionManager serverConnectionManager;
    ILocalDatabaseManager localDatabaseManager;
    ILocalStorageManager localStorageManager;
    INavigationService navigationService;
    IUsosInstallationsService usosInstallationsService;
    public LoginViewModel(IServerConnectionManager serverConnectionManager,
        ILocalDatabaseManager localDatabaseManager,
        ILocalStorageManager localStorageManager,
        INavigationService navigationService,
        IUsosInstallationsService usosInstallationsService)
    {
        this.serverConnectionManager = serverConnectionManager;
        this.localDatabaseManager = localDatabaseManager;
        this.localStorageManager = localStorageManager;
        this.navigationService = navigationService;
        this.usosInstallationsService = usosInstallationsService;

        AuthorizationService.OnLoginSucceeded += AuthorizationService_OnLoginSucceeded;
        AuthorizationService.OnLoginStarted += AuthorizationService_OnLoginStarted;

        _ = PreloadInstallations();

        if (localStorageManager.TryGettingInt(LocalStorageKeys.LoginAttemptCounter, out int attemptCount) && attemptCount > 0)
        {
            IsAdditionalLoginOptionVisible = true;
        }
    }

    ~LoginViewModel()
    {
        AuthorizationService.OnLoginSucceeded -= AuthorizationService_OnLoginSucceeded;
        AuthorizationService.OnLoginStarted -= AuthorizationService_OnLoginStarted;
    }

    private void AuthorizationService_OnLoginStarted()
    {
        if (localStorageManager.TryGettingInt(LocalStorageKeys.LoginAttemptCounter, out int attemptCount))
        {
            attemptCount++;
            localStorageManager.SetInt(LocalStorageKeys.LoginAttemptCounter, attemptCount);
            if (attemptCount > 0)
            {
                IsAdditionalLoginOptionVisible = true;
            }
        }
        else localStorageManager.SetInt(LocalStorageKeys.LoginAttemptCounter, 0);
    }

    private void AuthorizationService_OnLoginSucceeded()
    {
        localStorageManager.SetInt(LocalStorageKeys.LoginAttemptCounter, 0);
        IsAdditionalLoginOptionVisible = false;
    }

    async Task PreloadInstallations()
    {
        if (usosInstallationsService is not UsosInstallationsService service)
        {
            return;
        }
        var installations = await service.GetUsosInstallationsAsync();
        service.UsosInstallationsCache = installations;
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

        await navigationService.PushAsync<InstallationsPage, AuthorizationService.Mode>(AuthorizationService.Mode.RedirectWithCallback);
    }

    [ObservableProperty]
    bool isAdditionalLoginOptionVisible = false;

    [RelayCommand]
    async Task OnLoginWithPinClickedAsync()
    {
        SwitchToDefaultServices();

        await navigationService.PushAsync<InstallationsPage, AuthorizationService.Mode>(AuthorizationService.Mode.UsePinCode);
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