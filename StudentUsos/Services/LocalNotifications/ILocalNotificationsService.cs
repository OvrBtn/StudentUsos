namespace StudentUsos.Services.LocalNotifications;

public interface ILocalNotificationsService
{
    public Task<bool> HasOsLevelPermissionToScheduleNotificationsAsync();
    public Task<int> ScheduleNotificationAsync(LocalNotification notification);
    public void Remove(int id);
    public void RemoveAll();
}