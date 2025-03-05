namespace StudentUsos.Services.LocalNotifications;

public interface ILocalNotificationsService
{
    public bool AreAnyNotificationsEnabled();
    public Task<int> ScheduleNotificationAsync(LocalNotification notification);
    public void Remove(int id);
    public void RemoveAll();
}