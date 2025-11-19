using Plugin.FirebasePushNotifications;
using Plugin.FirebasePushNotifications.Model;
using StudentUsos.Services.ServerConnection;

namespace StudentUsos.Services;

public class FirebasePushNotificationsService
{
    IServerConnectionManager serverConnectionManager;
    INotificationPermissions permissions;
    IFirebasePushNotification firebasePushNotification;
    ILocalStorageManager localStorageManager;
    ILogger logger;
    public FirebasePushNotificationsService(IServerConnectionManager serverConnectionManager,
        INotificationPermissions permissions,
        IFirebasePushNotification firebasePushNotification,
        ILocalStorageManager localStorageManager,
        ILogger logger)
    {
        this.serverConnectionManager = serverConnectionManager;
        this.permissions = permissions;
        this.firebasePushNotification = firebasePushNotification;
        this.localStorageManager = localStorageManager;
        this.logger = logger;
    }

    static Func<Task<string>> GetFcmTokenFunc { get; set; }
    public static void GetFcmTokenFuncInitialize(Func<Task<string>> getFcmToken)
    {
        GetFcmTokenFunc = getFcmToken;
    }

    public async Task<string> GetFcmTokenAsync()
    {
#if ANDROID
        return await GetFcmTokenFunc();
#else
        await Task.CompletedTask;
        return string.Empty;
#endif
    }

    public async Task InitNotificationsAsync()
    {
        try
        {
            await CheckNotificationsPermissionsAsync(permissions);
            await RegisterPushNotificationsAsync(firebasePushNotification);

            if (localStorageManager.TryGettingString(LocalStorageKeys.FcmToken, out string fcmToken))
            {
                await SendFcmTokenToServerAsync(fcmToken);
            }
        }
        catch (Exception e)
        {
            //limiting logging to warning since used library will throw exception even when firebase is simply not available
            logger.Log(LogLevel.Warn, e);
        }
    }

    async Task CheckNotificationsPermissionsAsync(INotificationPermissions permissions)
    {
        if (await permissions.GetAuthorizationStatusAsync() != AuthorizationStatus.Granted)
        {
            await permissions.RequestPermissionAsync();
        }
    }

    async Task RegisterPushNotificationsAsync(IFirebasePushNotification firebasePushNotification)
    {
        try
        {
            await firebasePushNotification.RegisterForPushNotificationsAsync();
            firebasePushNotification.TokenRefreshed += FirebasePushNotification_TokenRefreshed;
        }
        catch (Exception e)
        {
            //limiting logging to warning since used library will throw exception even when firebase is simply not available
            logger.Log(LogLevel.Warn, e);
        }
    }

    private async void FirebasePushNotification_TokenRefreshed(object? sender, FirebasePushNotificationTokenEventArgs e)
    {
        localStorageManager.SetString(LocalStorageKeys.FcmToken, e.Token);

        await SendFcmTokenToServerAsync(e.Token);
    }

    public void CacheFcmToken(string token)
    {
        localStorageManager.SetString(LocalStorageKeys.FcmToken, token);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="token"></param>
    /// <returns>true if FCM token was successfully sent to server, false otherwise</returns>
    public async Task<bool> SendFcmTokenToServerAsync(string token)
    {
        Dictionary<string, string> args = new()
        {
            { "FcmToken", token }
        };
        var result = await serverConnectionManager.SendAuthorizedPostRequestAsync("usosnotificationhub/registerFcmToken", args, AuthorizationMode.Full);
        if (result != null)
        {
            localStorageManager.Remove(LocalStorageKeys.FcmToken);
            return true;
        }
        return false;
    }
}