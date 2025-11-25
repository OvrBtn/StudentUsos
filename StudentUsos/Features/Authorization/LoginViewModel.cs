using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentUsos.Features.Authorization.Models;
using StudentUsos.Features.Authorization.Services;
using StudentUsos.Features.Authorization.Views;
using StudentUsos.Services.ServerConnection;

namespace StudentUsos.Features.Authorization;

public partial class LoginViewModel : BaseViewModel
{

    [ObservableProperty]
    bool isActivityIndicatorRunning;

    [ObservableProperty]
    bool isAdditionalLoginOptionVisible = false;

    [ObservableProperty]
    string mainStateKey = StateKey.Loaded;

    public Command LoginCommand { get; }
    public Command LoginWithPinCommand { get; }

    /// <summary>
    /// Flag to make sure that <see cref="AuthorizationService.OnLoginSucceeded"/> and <see cref="AuthorizationService.OnLoginFailed"/> are subscribed to just once
    /// </summary>
    static bool areEventsSet = false;

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

        if (areEventsSet == false)
        {
            AuthorizationService.OnLoginSucceeded += () => { _ = LoginSuccessAsync(); };
            AuthorizationService.OnLoginFailed += LoginFail;
            AuthorizationService.OnContinueLogging += () => { MainStateKey = StateKey.Loading; };
            AuthorizationService.OnAuthorizationFinished += () => { MainStateKey = StateKey.Loaded; };
            areEventsSet = true;
        }

        if (localStorageManager.TryGettingString(LocalStorageKeys.LoginAttemptCounter, out string result) && int.TryParse(result, null, out int attemptCount))
        {
            if (attemptCount >= 2) IsAdditionalLoginOptionVisible = true;
        }
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

        var installation = await navigationService.PushAndReturnAsync<InstallationsPage, UsosInstallation>();
        if (installation is null)
        {
            return;
        }
        usosInstallationsService.SaveCurrentInstallation(installation.InstallationUrl);

        IsActivityIndicatorRunning = true;

        if (localStorageManager.TryGettingString(LocalStorageKeys.LoginAttemptCounter, out string result) && int.TryParse(result, null, out int attemptCount))
        {
            attemptCount++;
            localStorageManager.SetString(LocalStorageKeys.LoginAttemptCounter, attemptCount.ToString());
            if (attemptCount >= 2) IsAdditionalLoginOptionVisible = true;
        }
        else localStorageManager.SetString(LocalStorageKeys.LoginAttemptCounter, "1");

        AuthorizationService.BeginLoginAsync(AuthorizationService.Mode.RedirectWithCallback);
    }

    [RelayCommand]
    async Task OnLoginWithPinClickedAsync()
    {
        SwitchToDefaultServices();
        IsActivityIndicatorRunning = true;

        var installation = await navigationService.PushAndReturnAsync<InstallationsPage, UsosInstallation>();
        if (installation is null)
        {
            return;
        }
        usosInstallationsService.SaveCurrentInstallation(installation.InstallationUrl);

        AuthorizationService.BeginLoginAsync(AuthorizationService.Mode.UsePinCode);
    }

    async Task LoginSuccessAsync()
    {
        localStorageManager.SetString(LocalStorageKeys.LoginAttemptCounter, "0");
        IsAdditionalLoginOptionVisible = false;
        IsActivityIndicatorRunning = false;
        //create new tables from scratch or
        //drop all tables and then regenerate them in case there is something left from guest mode
        localDatabaseManager.ResetTables();
        await Shell.Current.GoToAsync("//DashboardPage");
    }

    private void LoginFail()
    {
        IsActivityIndicatorRunning = false;
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
        await LoginSuccessAsync();
    }
}