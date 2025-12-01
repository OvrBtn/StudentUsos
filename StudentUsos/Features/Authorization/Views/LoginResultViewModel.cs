using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentUsos.Features.Authorization.Models;
using StudentUsos.Features.Authorization.Services;

namespace StudentUsos.Features.Authorization.Views;

public partial class LoginResultViewModel : BaseViewModel, INavigableWithParameter<LoginResultParameters>
{
    INavigationService navigationService;
    IUsosInstallationsService usosInstallationsService;
    ILocalDatabaseManager localDatabaseManager;
    ILocalStorageManager localStorageManager;
    public LoginResultViewModel(INavigationService navigationService,
        IUsosInstallationsService usosInstallationsService,
        ILocalDatabaseManager localDatabaseManager,
        ILocalStorageManager localStorageManager)
    {
        AuthorizationService.OnLoginSucceeded += AuthorizationService_OnLoginSucceeded;
        AuthorizationService.OnLoginFailed += AuthorizationService_OnLoginFailed;
        App.OnResumed += App_OnResumed;
#if ANDROID
        MainActivity.OnIntentRecreatedWithData += MainActivity_OnIntentRecreatedWithData;
#endif

        this.navigationService = navigationService;
        this.usosInstallationsService = usosInstallationsService;
        this.localDatabaseManager = localDatabaseManager;
        this.localStorageManager = localStorageManager;
    }

    bool isResumedThroughDeepLink = false;
    private void MainActivity_OnIntentRecreatedWithData(Dictionary<string, string?> dictionary)
    {
        if (dictionary.TryGetValue("oauth_verifier", out string? verifier) && verifier is not null && currentAuthorizationMode == AuthorizationService.Mode.RedirectWithCallback)
        {
            isResumedThroughDeepLink = true;
        }
    }

    [ObservableProperty]
    bool isCancelButtonVisible = false;
    private async void App_OnResumed()
    {
        await Task.Delay(1000);
        if (MainStateKey != StateKey.Loading)
        {
            return;
        }
        IsCancelButtonVisible = true;
        if (isResumedThroughDeepLink == false && currentAuthorizationMode == AuthorizationService.Mode.RedirectWithCallback)
        {
            AuthorizationService_OnLoginFailed();
        }
    }

    ~LoginResultViewModel()
    {
        AuthorizationService.OnLoginSucceeded -= AuthorizationService_OnLoginSucceeded;
        AuthorizationService.OnLoginFailed -= AuthorizationService_OnLoginFailed;
        App.OnResumed -= App_OnResumed;
#if ANDROID
        MainActivity.OnIntentRecreatedWithData -= MainActivity_OnIntentRecreatedWithData;
#endif
    }

    UsosInstallation usosInstallation;
    AuthorizationService.Mode suggestedDefaultMode = AuthorizationService.Mode.RedirectWithCallback;
    public void OnNavigated(LoginResultParameters navigationParameter)
    {
        usosInstallation = navigationParameter.Installation;
        usosInstallationsService.SaveCurrentInstallation(usosInstallation.InstallationUrl);
        suggestedDefaultMode = navigationParameter.DefaultMode;
    }

    public static class AdditionalStateKeys
    {
        public const string Success = nameof(Success);
        public const string Fail = nameof(Fail);
    }

    [ObservableProperty]
    string mainStateKey = StateKey.Loading;

    [ObservableProperty]
    TimeSpan progress = TimeSpan.Zero;

    void ResetAnimations()
    {
        Progress = TimeSpan.Zero;
    }

    public async Task InitAsync()
    {
        await InitiateSigningInAsync(suggestedDefaultMode);
    }

    AuthorizationService.Mode currentAuthorizationMode;

    async Task InitiateSigningInAsync(AuthorizationService.Mode mode)
    {
        currentAuthorizationMode = mode;

        isResumedThroughDeepLink = false;
        IsCancelButtonVisible = false;
        MainStateKey = StateKey.Loading;
        await Task.Delay(50);
        AuthorizationService.BeginLoginAsync(mode);
    }

    private void AuthorizationService_OnLoginFailed()
    {
        ResetAnimations();
        MainStateKey = AdditionalStateKeys.Fail;
    }

    private async void AuthorizationService_OnLoginSucceeded()
    {
        ResetAnimations();
        MainStateKey = AdditionalStateKeys.Success;

        await Task.Delay(2000);
        localStorageManager.SetInt(LocalStorageKeys.LoginAttemptCounter, 0);
        //create new tables from scratch or
        //drop all tables and then regenerate them in case there is something left from guest mode
        localDatabaseManager.ResetTables();
        await Shell.Current.GoToAsync("//DashboardPage");
    }

    [RelayCommand]
    async Task TryAgainAsync()
    {
        await InitiateSigningInAsync(AuthorizationService.Mode.RedirectWithCallback);
    }

    [RelayCommand]
    async Task TryAgainUsingPinCodeAsync()
    {
        await InitiateSigningInAsync(AuthorizationService.Mode.UsePinCode);
    }

    [RelayCommand]
    void CancelOnClick()
    {
        AuthorizationService_OnLoginFailed();
    }
}
