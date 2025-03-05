using StudentUsos.Features.Calendar.Models;

namespace StudentUsos.Features.Calendar.Repositories;

public interface IUsosCalendarRepository
{
    public List<UsosCalendarEvent> GetAllEvents();
    public List<UsosCalendarEvent> GetEvents(int year, int month);
    public void SaveEvents(IEnumerable<UsosCalendarEvent> events);
    public bool IsEmpty();

    public void CancelAllLocalNotifications();
    /// <summary>
    /// Set up new calendar event notifications based on parameters or cancel them
    /// </summary>
    /// <param name="areNotificationsEnabled">If true notifications will be added, if false currently set notifications will be canceled</param>
    /// <param name="notifyDaysBeforeEvent">Notification will be sent given amount of days before the event</param>
    /// <param name="notifyAtTimeOfDay">Notification will be sent at given time of day ignoring time set in event</param>
    public Task<List<UsosCalendarEvent>> RefreshNotificationsAsync(bool areNotificationsEnabled, int notifyDaysBeforeEvent, TimeSpan notifyAtTimeOfDay);
    public Task<List<UsosCalendarEvent>> ScheduleNotificationsAsync(List<UsosCalendarEvent> events, int notifyDaysBeforeEvent, TimeSpan notifyAtTimeOfDay);

    public Task<(List<UsosCalendarEvent> localExceptServer, List<UsosCalendarEvent> serverExceptLocal)> SaveEventsFromServerAndHandleLocalNotificationsAsync(
        int year, int month, List<UsosCalendarEvent> events, bool isPrimaryUpdate);
}