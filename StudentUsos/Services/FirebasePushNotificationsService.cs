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
    public FirebasePushNotificationsService(IServerConnectionManager serverConnectionManager,
        INotificationPermissions permissions,
        IFirebasePushNotification firebasePushNotification,
        ILocalStorageManager localStorageManager)
    {
        this.serverConnectionManager = serverConnectionManager;
        this.permissions = permissions;
        this.firebasePushNotification = firebasePushNotification;
        this.localStorageManager = localStorageManager;
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
        await CheckNotificationsPermissionsAsync(permissions);
        await RegisterPushNotificationsAsync(firebasePushNotification);

        if (localStorageManager.TryGettingData(LocalStorageKeys.FcmToken, out string fcmToken))
        {
            await SendFcmTokenToServerAsync(fcmToken);
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
        await firebasePushNotification.RegisterForPushNotificationsAsync();
        firebasePushNotification.TokenRefreshed += FirebasePushNotification_TokenRefreshed;
    }

    private async void FirebasePushNotification_TokenRefreshed(object? sender, FirebasePushNotificationTokenEventArgs e)
    {
        localStorageManager.SetData(LocalStorageKeys.FcmToken, e.Token);

        await SendFcmTokenToServerAsync(e.Token);
    }

    public void CacheFcmToken(string token)
    {
        localStorageManager.SetData(LocalStorageKeys.FcmToken, token);
    }

    public async Task SendFcmTokenToServerAsync(string token)
    {
        Dictionary<string, string> args = new()
        {
            { "FcmToken", token }
        };
        var result = await serverConnectionManager.SendAuthorizedPostRequestAsync("usosnotificationhub/registerFcmToken", args, AuthorizationMode.Full);
        if (result != null)
        {
            localStorageManager.Remove(LocalStorageKeys.FcmToken);
        }
    }
}