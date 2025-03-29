using StudentUsos.Controls;
using StudentUsos.Resources.LocalizedStrings;
using StudentUsos.Services.LocalNotifications;
using StudentUsos.Services.ServerConnection;
using System.Net;
using System.Text.Json;

namespace StudentUsos.Features.Authorization.Services;

internal static class AuthorizationService
{

    public static string Installation { get; } = "https://usosapps.put.poznan.pl/";
    static List<string> scopes = new List<string> { "email", "offline_access", "studies", "grades", "payments", "surveys_filling", "other_emails" };
    public static string AccessToken { get; set; }
    public static string InternalAccessToken { get; set; }
    public static string InternalAccessTokenSecret { get; set; }

    public enum PreferencesKeys
    {
        AccessToken,
        [Obsolete]
        AccessTokenSecret,
        Scopes,
    }

    /// <summary>
    /// These keys should be used with <see cref="SecureStorage"/> but there are bugs making it unusable so <see cref="Preferences"/> are used instead
    /// </summary>
    public enum SecureStorageKeys
    {
        AccessToken,
        InternalAccessToken,
        InternalAccessTokenSecret
    }

    static IServerConnectionManager serverConnectionManager;
    static AuthorizationService()
    {
#if ANDROID
        MainActivity.OnLogInCallback += (v) => _ = ContinueAuthenticationAsync(v);
#endif

        serverConnectionManager = App.ServiceProvider?.GetService<IServerConnectionManager>()!;

        var firebasePushNotificationsService = App.ServiceProvider?.GetService<FirebasePushNotificationsService>();
        if (firebasePushNotificationsService is not null)
        {
            OnLoginSucceeded += async () =>
            {
                string token = await firebasePushNotificationsService.GetFcmTokenAsync();
                _ = firebasePushNotificationsService.SendFcmTokenToServerAsync(token);
            };
        }
    }

    public static event Action OnLoginSucceeded;
    public static event Action OnLoginFailed;

    public static bool HasJustLoggedIn { get; private set; }

    public static void CheckForMissingScopes()
    {
        var scopesFromPreferences = Preferences.Get(PreferencesKeys.Scopes.ToString(), "");
        if (scopesFromPreferences == "") Preferences.Set(PreferencesKeys.Scopes.ToString(), string.Join('|', scopes));
        else if (scopesFromPreferences != string.Join('|', scopes))
        {
            var scropesFromPreferencesSplited = scopesFromPreferences.Split("|").ToList();
            List<string> difference;
            if (scopes.Count > scropesFromPreferencesSplited.Count)
            {
                difference = scopes.Except(scropesFromPreferencesSplited).ToList();
            }
            else
            {
                difference = scropesFromPreferencesSplited.Except(scopes).ToList();
            }
            MessagePopupPage.CreateAndShow(LocalizedStrings.Permissions, LocalizedStrings.Permissions_MissingPermissionsDescription +
                                                                         "\n" + LocalizedStrings.Login_MissingScopes + " " + string.Join(" ", difference),
                LocalizedStrings.Logout, LocalizedStrings.Later, LogoutAsync, null);
        }
    }

    public static bool CheckIfSignedInAndRetrieveTokens()
    {
        string? accessToken = Preferences.Get(PreferencesKeys.AccessToken.ToString(), null);
        bool containsKeys = accessToken != null;
        if (containsKeys)
        {
            RetrieveTokens();
        }
        return containsKeys;
    }

    public static void RetrieveTokensIfNotSet()
    {
        if (string.IsNullOrEmpty(AccessToken)
            || string.IsNullOrEmpty(InternalAccessToken)
            || string.IsNullOrEmpty(InternalAccessTokenSecret))
        {
            RetrieveTokens();
        }
    }

    static void RetrieveTokens()
    {
        AccessToken = Preferences.Get(PreferencesKeys.AccessToken.ToString(), null)!;
        InternalAccessToken = Preferences.Get(SecureStorageKeys.InternalAccessToken.ToString(), null)!;
        InternalAccessTokenSecret = Preferences.Get(SecureStorageKeys.InternalAccessTokenSecret.ToString(), null)!;
    }


    public static async Task<bool> IsSessionActiveAsync()
    {
        var result = await serverConnectionManager.SendAuthorizedGetRequestAsync("authorization/isSessionActive", new(), AuthorizationMode.Full);
        if (result is null)
        {
            return true;
        }
        if (result.IsSuccess)
        {
            return result.Response.ToLowerInvariant() == "true";
        }
        else
        {
            if (result.HttpResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
            {
                return false;
            }
        }
        return true;
    }

    public enum Mode
    {
        RedirectWithCallback,
        UsePinCode
    }

    static string? requestToken = null;
    public static async void BeginLoginAsync(Mode mode)
    {
        try
        {
            Dictionary<string, string> arguments = new()
            {
                { "scopes", string.Join("|", scopes) },
                { "callback", mode == Mode.UsePinCode ? "oob" : "studenckiusosput://studenckiusosput"}
            };
            var accessTokenRequestResult = await serverConnectionManager.SendGetRequestAsync("authorization/authorizeurl", arguments);
            if (accessTokenRequestResult is null || accessTokenRequestResult.IsSuccess == false)
            {
                throw new Exception("Request to USOS API has failed");
            }
            var accessTokenRequestResultDeserialized = JsonSerializer.Deserialize<Dictionary<string, string>>(accessTokenRequestResult.Response);
            if (accessTokenRequestResultDeserialized is null)
            {
                throw new Exception("Deserialization issue");
            }
            requestToken = accessTokenRequestResultDeserialized["requestToken"];
            LocalStorageManager.Default.SetData(LocalStorageKeys.RequestToken, requestToken);

            string authorizeUrl = accessTokenRequestResultDeserialized["authorizeURL"];
            await Browser.OpenAsync(authorizeUrl, BrowserLaunchMode.SystemPreferred);

            if (mode == Mode.UsePinCode)
            {
                EntryPopup.CreateAndShow("PIN",
                    LocalizedStrings.LoginPage_InputPIN,
                    LocalizedStrings.Confirm, LocalizedStrings.Cancel,
                    (v) => _ = ContinueAuthenticationAsync(v),
                    () => { OnLoginFailed?.Invoke(); }, Keyboard.Numeric);
            }
        }
        catch (Exception ex)
        {
            Logger.Default?.LogCatchedException(ex);
            OnCatch(OnLoginFailed);
        }

    }

    static void OnCatch(Action onLoginFailed)
    {
        onLoginFailed?.Invoke();
        ApplicationService.Default.ShowErrorMessage(LocalizedStrings.Errors_LoadingError, LocalizedStrings.LoginPage_LoginFailed);
    }

    public static event Action OnContinueLogging;
    public static event Action OnAuthorizationFinished;

    static async Task ContinueAuthenticationAsync(string returned)
    {
        try
        {
            OnContinueLogging?.Invoke();
            if (requestToken == null)
            {
                if (LocalStorageManager.Default.TryGettingData(LocalStorageKeys.RequestToken, out string requestTokenFromPrefrences))
                {
                    requestToken = requestTokenFromPrefrences;
                }
                else throw new Exception("Request token is null");
            }
            if (int.TryParse(returned, out int result) == false || result <= 0)
            {
                throw new Exception("Can't parse verifier");
            }
            string verifier = returned;

            Dictionary<string, string> arguments = new()
            {
                { "verifier", verifier },
                { "requesttoken", requestToken }
            };
            var requestResult = await serverConnectionManager.SendGetRequestAsync("authorization/accesstoken", arguments);
            if (requestResult is null || requestResult.IsSuccess == false)
            {
                throw new Exception("Request to USOS API has failed");
            }
            var resultDeserialized = JsonSerializer.Deserialize<Dictionary<string, string>>(requestResult.Response);
            if (resultDeserialized is null)
            {
                throw new Exception("Deserialization issue");
            }
            AccessToken = resultDeserialized["accessToken"];
            InternalAccessToken = resultDeserialized["internalAccessToken"];
            InternalAccessTokenSecret = resultDeserialized["internalAccessTokenSecret"];
            Preferences.Set(SecureStorageKeys.AccessToken.ToString(), AccessToken);
            Preferences.Set(SecureStorageKeys.InternalAccessToken.ToString(), InternalAccessToken);
            Preferences.Set(SecureStorageKeys.InternalAccessTokenSecret.ToString(), InternalAccessTokenSecret);

            HasJustLoggedIn = true;
            OnLoginSucceeded?.Invoke();
            OnAuthorizationFinished?.Invoke();
        }
        catch (Exception ex)
        {
            Logger.Default?.LogCatchedException(ex);
            OnCatch(OnLoginFailed);
        }
        finally
        {
            OnAuthorizationFinished?.Invoke();
        }
    }

    public static event Action OnLogout;

    public static async void LogoutAsync()
    {
        Preferences.Remove(PreferencesKeys.AccessToken.ToString());
        Preferences.Remove(PreferencesKeys.AccessTokenSecret.ToString());
        Preferences.Remove(SecureStorageKeys.AccessToken.ToString());
        Preferences.Remove(SecureStorageKeys.InternalAccessToken.ToString());
        Preferences.Remove(SecureStorageKeys.InternalAccessTokenSecret.ToString());
        AccessToken = string.Empty;
        LocalDatabaseManager.Default.ResetTables();
        LocalStorageManager.Default.DeleteEverything();
        App.ServiceProvider.GetService<ILocalNotificationsService>()?.RemoveAll();
        await Shell.Current.GoToAsync("//LoginPage");
        CustomTabBar.ActiveTabIndex = 0;
        _ = serverConnectionManager.SendAuthorizedGetRequestAsync("authorization/logout", new(), AuthorizationMode.Full).ConfigureAwait(false);
    }
}