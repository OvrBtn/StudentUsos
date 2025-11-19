using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentUsos.Features.Authorization.Services;
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
    public LoginViewModel(IServerConnectionManager serverConnectionManager,
        ILocalDatabaseManager localDatabaseManager,
        ILocalStorageManager localStorageManager)
    {
        this.serverConnectionManager = serverConnectionManager;
        this.localDatabaseManager = localDatabaseManager;
        this.localStorageManager = localStorageManager;

        LoginCommand = new Command(OnLoginClicked);
        LoginWithPinCommand = new Command(OnLoginWithPINClicked);

        if (areEventsSet == false)
        {
            AuthorizationService.OnLoginSucceeded += () => { _ = LoginSuccessAsync(); };
            AuthorizationService.OnLoginFailed += LoginFail;
            AuthorizationService.OnContinueLogging += () => { MainStateKey = StateKey.Loading; };
            AuthorizationService.OnAuthorizationFinished += () => { MainStateKey = StateKey.Loaded; };
            areEventsSet = true;
        }

        if (LocalStorageManager.Default.TryGettingString(LocalStorageKeys.LoginAttemptCounter, out string result) && int.TryParse(result, null, out int attemptCount))
        {
            if (attemptCount >= 2) IsAdditionalLoginOptionVisible = true;
        }
    }

    private void OnLoginClicked()
    {
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

    private void OnLoginWithPINClicked()
    {
        IsActivityIndicatorRunning = true;

        AuthorizationService.BeginLoginAsync(AuthorizationService.Mode.UsePinCode);
    }

    private async Task LoginSuccessAsync()
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
            manager.SwitchImplementation(new GuestServerConnectionManager());
        }
        DateAndTimeProvider.SwitchProvider(new GuestDateAndTimeProvider());
        await LoginSuccessAsync();
    }
}