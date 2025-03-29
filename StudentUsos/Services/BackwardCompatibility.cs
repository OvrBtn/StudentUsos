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
    /// When changes in code are made that might not correlate with previous version (e.g. local databse structure has changed and exceptions might occur) 
    /// add number of new version here so local data will be deleted
    /// </summary>
    static readonly string[] AppVersionsToReset = { "3.7.0", "3.7.2", "3.7.3", "4.0.3" };

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

        //compatibility flag from previous iterations
        if (Preferences.ContainsKey("TempCompatibilityFlag"))
        {
            Preferences.Remove(AuthorizationService.PreferencesKeys.AccessTokenSecret.ToString());
            return;
        }

        var accessToken = Preferences.Get(AuthorizationService.PreferencesKeys.AccessToken.ToString(), null);
        var accessTokenSecret = Preferences.Get(AuthorizationService.PreferencesKeys.AccessTokenSecret.ToString(), null);
        if (accessToken is null || accessTokenSecret is null)
        {
            return;
        }

        var serverConnectionManager = App.ServiceProvider.GetService<IServerConnectionManager>()!;

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
            Preferences.Remove(AuthorizationService.PreferencesKeys.AccessTokenSecret.ToString());
        }

        var firebasePushNotificationsService = App.ServiceProvider.GetService<FirebasePushNotificationsService>()!;
        string token = await firebasePushNotificationsService.GetFcmTokenAsync();
        firebasePushNotificationsService.CacheFcmToken(token);
        await firebasePushNotificationsService.SendFcmTokenToServerAsync(token);

        OnCompatibilityRegisterSucceeded?.Invoke();
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
            var accessTokenSecret = Preferences.Get(AuthorizationService.PreferencesKeys.AccessTokenSecret.ToString(), null);

            var internalAccessToken = Preferences.Get(AuthorizationService.SecureStorageKeys.InternalAccessToken.ToString(), null);
            var internalAccessTokenSecret = Preferences.Get(AuthorizationService.SecureStorageKeys.InternalAccessTokenSecret.ToString(), null);
            var googleCalendars = localDatabaseManager.GetAll<GoogleCalendar>();

            var whatsNewCarouselLastId = localStorageManager.GetData(LocalStorageKeys.WhatsNewCarouselLastId);
            var whatsNewListLastId = localStorageManager.GetData(LocalStorageKeys.WhatsNewListLastId);

            localDatabaseManager.ResetTables();
            localStorageManager.DeleteEverything();
            localNotificationsService.RemoveAll();

            if (scopes is not null) Preferences.Set(AuthorizationService.PreferencesKeys.Scopes.ToString(), scopes);
            if (accessToken is not null) Preferences.Set(AuthorizationService.SecureStorageKeys.AccessToken.ToString(), accessToken);
            if (accessTokenSecret is not null) Preferences.Set("AccessTokenSecret", accessTokenSecret);
            if (internalAccessToken is not null) Preferences.Set(AuthorizationService.SecureStorageKeys.InternalAccessToken.ToString(), internalAccessToken);
            if (internalAccessTokenSecret is not null) Preferences.Set(AuthorizationService.SecureStorageKeys.InternalAccessTokenSecret.ToString(), internalAccessTokenSecret);
            if (googleCalendars is not null && googleCalendars.Count > 0) localDatabaseManager.InsertAll(googleCalendars);

            if (whatsNewCarouselLastId is not null) localStorageManager.SetData(LocalStorageKeys.WhatsNewCarouselLastId, whatsNewCarouselLastId);
            if (whatsNewListLastId is not null) localStorageManager.SetData(LocalStorageKeys.WhatsNewListLastId, whatsNewListLastId);
        }
        catch (Exception ex) { Logger.Logger.Default?.LogCatchedException(ex); }
    }
}