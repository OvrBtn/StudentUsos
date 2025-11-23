using Plugin.LocalNotification;

namespace StudentUsos.Services.LocalNotifications;

public class LocalNotificationsService : ILocalNotificationsService
{
    ILocalStorageManager localStorageManager;
    IApplicationService applicationService;
    public LocalNotificationsService(ILocalStorageManager localStorageManager, IApplicationService applicationService)
    {
        this.localStorageManager = localStorageManager;
        this.applicationService = applicationService;
    }


    int GetIdForNewNotification()
    {
        if (localStorageManager.ContainsKey(LocalStorageKeys.IdOfLastNotification) == false)
        {
            localStorageManager.SetString(LocalStorageKeys.IdOfLastNotification, "0");
        }
        var id = localStorageManager.GetString(LocalStorageKeys.IdOfLastNotification);
        if (id != null && int.TryParse(id, out int result))
        {
            result = result % (int.MaxValue - 1);
            result++;
            localStorageManager.SetString(LocalStorageKeys.IdOfLastNotification, result.ToString());
            return result;
        }
        return -1;
    }

    bool hasRequestedNotificationPermission = false;


    public Task<int> ScheduleNotificationAsync(LocalNotification notification)
    {
        return AddNotificationAsync(new()
        {
            Title = notification.Title,
            Description = notification.Description,
            Subtitle = notification.Subtitle,
            Schedule = new NotificationRequestSchedule()
            {
                NotifyTime = notification.ScheduledDateTime
            },
            Group = notification.Group
        });
    }

    async Task<int> AddNotificationAsync(NotificationRequest notification)
    {
        int id = GetIdForNewNotification();
        if (await HasOsLevelPermissionToScheduleNotificationsAsync() == false)
        {
            if (hasRequestedNotificationPermission)
            {
                return id;
            }
            hasRequestedNotificationPermission = true;
            applicationService.MainThreadInvoke(async () =>
            {
                try
                {
                    //this will crash when called from background worker hence the try catch
                    await LocalNotificationCenter.Current.RequestNotificationPermission();
                    notification.NotificationId = id;
                    await LocalNotificationCenter.Current.Show(notification);
                }
                catch
                {
#if DEBUG
                    throw;
#endif
                }
            });
        }
        else
        {
            notification.NotificationId = id;
            await LocalNotificationCenter.Current.Show(notification);
        }
        return id;
    }

    public void Remove(int id)
    {
        LocalNotificationCenter.Current.Cancel(id);
    }

    public void RemoveAll()
    {
        LocalNotificationCenter.Current.ClearAll();
        LocalNotificationCenter.Current.CancelAll();
    }

    public async Task<bool> HasOsLevelPermissionToScheduleNotificationsAsync()
    {
        return await LocalNotificationCenter.Current.AreNotificationsEnabled();
    }
}