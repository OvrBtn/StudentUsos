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

    public bool AreAnyNotificationsEnabled()
    {
        if (localStorageManager.TryGettingData(LocalStorageKeys.AreNotificationsEnabled, out string result) == false ||
            bool.TryParse(result, out bool parsedToBool) == false || parsedToBool == false)
        {
            return false;
        }
        return true;
    }


    int GetIdForNewNotification()
    {
        if (localStorageManager.ContainsData(LocalStorageKeys.IdOfLastNotification) == false)
        {
            localStorageManager.SetData(LocalStorageKeys.IdOfLastNotification, "0");
        }
        var id = localStorageManager.GetData(LocalStorageKeys.IdOfLastNotification);
        if (id != null && int.TryParse(id, out int result))
        {
            result = result % int.MaxValue - 1;
            result++;
            localStorageManager.SetData(LocalStorageKeys.IdOfLastNotification, result.ToString());
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
        if (await LocalNotificationCenter.Current.AreNotificationsEnabled() == false)
        {
            if (hasRequestedNotificationPermission) return id;
            hasRequestedNotificationPermission = true;
            applicationService.MainThreadInvoke(async () =>
            {
                await LocalNotificationCenter.Current.RequestNotificationPermission();
                notification.NotificationId = id;
                await LocalNotificationCenter.Current.Show(notification);
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
}