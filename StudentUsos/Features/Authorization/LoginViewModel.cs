using CommunityToolkit.Mvvm.ComponentModel;
using StudentUsos.Features.Authorization.Services;

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

    public LoginViewModel()
    {
        LoginCommand = new Command(OnLoginClicked);
        LoginWithPinCommand = new Command(OnLoginWithPINClicked);

        if (areEventsSet == false)
        {
            AuthorizationService.OnLoginSucceeded += LoginSuccessAsync;
            AuthorizationService.OnLoginFailed += LoginFail;
            AuthorizationService.OnContinueLogging += () => { MainStateKey = StateKey.Loading; };
            AuthorizationService.OnAuthorizationFinished += () => { MainStateKey = StateKey.Loaded; };
            areEventsSet = true;
        }

        if (LocalStorageManager.Default.TryGettingData(LocalStorageKeys.LoginAttemptCounter, out string result) && int.TryParse(result, null, out int attemptCount))
        {
            if (attemptCount >= 2) IsAdditionalLoginOptionVisible = true;
        }
    }

    private void OnLoginClicked()
    {
        IsActivityIndicatorRunning = true;

        if (LocalStorageManager.Default.TryGettingData(LocalStorageKeys.LoginAttemptCounter, out string result) && int.TryParse(result, null, out int attemptCount))
        {
            attemptCount++;
            LocalStorageManager.Default.SetData(LocalStorageKeys.LoginAttemptCounter, attemptCount.ToString());
            if (attemptCount >= 2) IsAdditionalLoginOptionVisible = true;
        }
        else LocalStorageManager.Default.SetData(LocalStorageKeys.LoginAttemptCounter, "1");

        AuthorizationService.BeginLoginAsync(AuthorizationService.Mode.RedirectWithCallback);
    }

    private void OnLoginWithPINClicked()
    {
        IsActivityIndicatorRunning = true;

        AuthorizationService.BeginLoginAsync(AuthorizationService.Mode.UsePinCode);
    }

    private async void LoginSuccessAsync()
    {
        LocalStorageManager.Default.SetData(LocalStorageKeys.LoginAttemptCounter, "0");
        IsAdditionalLoginOptionVisible = false;
        IsActivityIndicatorRunning = false;
        LocalDatabaseManager.Default.GenerateTables();
        await Shell.Current.GoToAsync("//DashboardPage");
    }

    private void LoginFail()
    {
        IsActivityIndicatorRunning = false;
    }
}