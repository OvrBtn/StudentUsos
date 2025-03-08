using StudentUsos.Features.Authorization.Services;
using StudentUsos.Features.Calendar.Models;
using StudentUsos.Services.LocalNotifications;
using StudentUsos.Services.ServerConnection;
using System.Text.Json;

namespace StudentUsos.Services;

/// <summary>
/// Class for reseting locally saved data after app update
/// </summary>
public static class BackwardCompatibility
{
    /// <summary>
    /// When changes in code are made that might not correlate with previous version (e.g. local databse structure has changed and exceptions might occur) add number of new version here so local data will be deleted
    /// </summary>
    static readonly string[] AppVersionsToReset = { "3.7.0", "3.7.2", "3.7.3", "4.0.0" };

    public enum PrefencesEnum
    {
        ResetedVersions,
        PreviousAppVersionsToReset
    }

    public static string CurrentVersion => AppInfo.Current.VersionString;

    public static void Check()
    {
        try
        {
#if DEBUG
            //ResetLocalData();
#endif
            string resetedVersions = Preferences.Get(PrefencesEnum.ResetedVersions.ToString(), "");
            string appVersionsToResetJoined = string.Join("|", AppVersionsToReset);
            List<string> resetedVersionsList = resetedVersions.Split("|").ToList();
            string previousAppVersionsToReset = Preferences.Get(PrefencesEnum.PreviousAppVersionsToReset.ToString(), "");
            if (previousAppVersionsToReset != appVersionsToResetJoined && resetedVersionsList.Any(x => x == CurrentVersion) == false)
            {
                ResetLocalData();

                resetedVersionsList.Add(CurrentVersion);
                Preferences.Set(PrefencesEnum.ResetedVersions.ToString(), string.Join('|', resetedVersionsList));
                Preferences.Set(PrefencesEnum.PreviousAppVersionsToReset.ToString(), appVersionsToResetJoined);
            }

            _ = CheckIfUsosKeysAreStoredLocallyAsync();
        }
        catch (Exception ex)
        {
            Logger.Logger.Default?.LogCatchedException(ex);
        }
    }

    public static event Action OnCompatibilityRegisterSucceeded;

    static async Task CheckIfUsosKeysAreStoredLocallyAsync()
    {
        //see comments below for a explanation
        if (Preferences.ContainsKey("TempCompatibilityFlag"))
        {
            return;
        }

        var serverConnectionManager = App.ServiceProvider.GetService<IServerConnectionManager>()!;

        string emptyString = "empty";
        var accessToken = Preferences.Get(AuthorizationService.PreferencesKeys.AccessToken.ToString(), emptyString);
        var accessTokenSecret = Preferences.Get(AuthorizationService.PreferencesKeys.AccessTokenSecret.ToString(), emptyString);
        if (accessToken != emptyString && accessTokenSecret != emptyString)
        {
            Dictionary<string, string> args = new()
            {
                { "usosAccessToken", accessToken },
                { "usosAccessTokenSecret", accessTokenSecret }
            };
            var result = await serverConnectionManager.SendAuthorizedPostRequestAsync("authorization/compatibilityRegister", args, AuthorizationMode.StaticInternalsOnly);
            if (result is null)
            {
                return;
            }
            if (result.IsSuccess == false)
            {
                return;
            }

            //for now don't remove them so in case I need to revert update users won't have to sign in again
            //Preferences.Remove(AuthorizationService.PreferencesKeys.AccessToken.ToString());
            //Preferences.Remove(AuthorizationService.PreferencesKeys.AccessTokenSecret.ToString());

            //instead set a temp flag to avoid spamming the server
            Preferences.Set("TempCompatibilityFlag", true.ToString());

            Preferences.Set(AuthorizationService.SecureStorageKeys.AccessToken.ToString(), accessToken);
            var deserialized = JsonSerializer.Deserialize(result.Response, UtilitiesJsonContext.Default.DictionaryStringString);
            if (deserialized is null)
            {
                return;
            }
            if (deserialized.TryGetValue("internalAccessToken", out var internalAccessToken) &&
                deserialized.TryGetValue("internalAccessTokenSecret", out var internalAccessTokenSecret))
            {
                Preferences.Set(AuthorizationService.SecureStorageKeys.InternalAccessToken.ToString(), internalAccessToken);
                Preferences.Set(AuthorizationService.SecureStorageKeys.InternalAccessTokenSecret.ToString(), internalAccessTokenSecret);
                AuthorizationService.InternalAccessToken = internalAccessToken;
                AuthorizationService.InternalAccessTokenSecret = internalAccessTokenSecret;
            }

            var firebasePushNotificationsService = App.ServiceProvider.GetService<FirebasePushNotificationsService>()!;
            string token = await firebasePushNotificationsService.GetFcmTokenAsync();
            firebasePushNotificationsService.CacheFcmToken(token);
            await firebasePushNotificationsService.SendFcmTokenToServerAsync(token);

            OnCompatibilityRegisterSucceeded?.Invoke();
        }
    }

    public static void ResetLocalData()
    {
        try
        {
            var localDatabaseManager = App.ServiceProvider.GetService<ILocalDatabaseManager>()!;
            var localStorageManager = App.ServiceProvider.GetService<ILocalStorageManager>()!;
            var localNotificationsService = App.ServiceProvider.GetService<ILocalNotificationsService>()!;

            var scopes = Preferences.Get(AuthorizationService.PreferencesKeys.Scopes.ToString(), null);
            var accessToken = Preferences.Get(AuthorizationService.SecureStorageKeys.AccessToken.ToString(), null);
            //needed only for compatibility with versions <= 3.x.x
            var accessTokenSecret = Preferences.Get("AccessTokenSecret", null);
            var internalAccessToken = Preferences.Get(AuthorizationService.SecureStorageKeys.InternalAccessToken.ToString(), null);
            var internalAccessTokenSecret = Preferences.Get(AuthorizationService.SecureStorageKeys.InternalAccessTokenSecret.ToString(), null);
            var googleCalendars = localDatabaseManager.GetAll<GoogleCalendar>();

            localDatabaseManager.ResetTables();
            localStorageManager.DeleteEverything();
            localNotificationsService.RemoveAll();

            if (scopes is not null) Preferences.Set(AuthorizationService.PreferencesKeys.Scopes.ToString(), scopes);
            if (accessToken is not null) Preferences.Set(AuthorizationService.SecureStorageKeys.AccessToken.ToString(), accessToken);
            if (accessTokenSecret is not null) Preferences.Set("AccessTokenSecret", accessTokenSecret);
            if (internalAccessToken is not null) Preferences.Set(AuthorizationService.SecureStorageKeys.InternalAccessToken.ToString(), internalAccessToken);
            if (internalAccessTokenSecret is not null) Preferences.Set(AuthorizationService.SecureStorageKeys.InternalAccessTokenSecret.ToString(), internalAccessTokenSecret);
            if (googleCalendars is not null && googleCalendars.Count > 0) localDatabaseManager.InsertAll(googleCalendars);
        }
        catch (Exception ex) { Logger.Logger.Default?.LogCatchedException(ex); }
    }
}