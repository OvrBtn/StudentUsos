using StudentUsos.Features.Authorization.Services;
using StudentUsos.Features.Calendar.Models;
using StudentUsos.Services.LocalNotifications;
using StudentUsos.Services.ServerConnection;
using System.Text.Json;

namespace StudentUsos.Services;

/// <summary>
/// Class repsonsible for handling compatibility between different versions
/// </summary>
public static class BackwardCompatibility
{
    public static string CurrentVersion => AppInfo.Current.VersionString;

    public static async Task Check()
    {
        try
        {
            await CheckIfShouldResetOrSignOut();
            await CheckIfUsosKeysAreStoredLocallyAsync();
        }
        catch (Exception ex)
        {
            Logger.Logger.Default?.LogCatchedException(ex);
        }
    }

    static async Task CheckIfShouldResetOrSignOut()
    {
        var localStorageManager = App.ServiceProvider.GetService<ILocalStorageManager>()!;
        string? lastCheckedVersion = localStorageManager.GetData(LocalStorageKeys.BackwardCompatibilityLastCheckedVersion);
        if (lastCheckedVersion is null)
        {
            localStorageManager.SetData(LocalStorageKeys.BackwardCompatibilityLastCheckedVersion, CurrentVersion);
            //if there were no previous checks because user updated from version which didn't have this implementation
            //this will allow current check to always execute and make it compatible with older versions
            lastCheckedVersion = "0.0.0";
        }

        if (lastCheckedVersion == CurrentVersion)
        {
            return;
        }

        using var stream = await FileSystem.OpenAppPackageFileAsync("versions.json");
        using var reader = new StreamReader(stream);
        var allVersionsJson = reader.ReadToEnd();
        var allVersions = JsonSerializer.Deserialize(allVersionsJson, AppVersionInfoJsonContext.Default.ListAppVersionInfo);
        if (allVersions is null)
        {
            return;
        }

        bool shouldResetLocalData = false;
        bool shouldForceSignOut = false;
        foreach (var versionInfo in allVersions)
        {
            //versionInfo.Version being bigger/later than CurrentVersion or smaller/earlier than lastCheckedVersion
            if (string.Compare(versionInfo.Version, CurrentVersion) > 0 ||
                string.Compare(versionInfo.Version, lastCheckedVersion) < 0)
            {
                continue;
            }
            if (versionInfo.ForceResetLocalData)
            {
                shouldResetLocalData = true;
            }
            if (versionInfo.ForceSignOut)
            {
                shouldForceSignOut = true;
            }
        }

        if (shouldForceSignOut)
        {
            AuthorizationService.LogoutAsync();
            localStorageManager.SetData(LocalStorageKeys.BackwardCompatibilityLastCheckedVersion, CurrentVersion);
            return;
        }
        if (shouldResetLocalData)
        {
            ResetLocalData();
            localStorageManager.SetData(LocalStorageKeys.BackwardCompatibilityLastCheckedVersion, CurrentVersion);
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
        var deserialized = JsonSerializer.Deserialize(result.Response, JsonContext.Default.DictionaryStringString);
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