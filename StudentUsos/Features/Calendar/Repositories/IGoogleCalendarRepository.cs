using StudentUsos.Features.Calendar.Models;

namespace StudentUsos.Features.Calendar.Repositories;

public interface IGoogleCalendarRepository
{
    public List<GoogleCalendar> GetAllCalendars();
    public void RemoveCalendar(GoogleCalendar calendar);
    public void SaveCalendar(GoogleCalendar calendar);
    public List<GoogleCalendarEvent> GetAllEvents();
    public void InsertOrReplaceEvents(IEnumerable<GoogleCalendarEvent> events);
    public void InsertEvents(IEnumerable<GoogleCalendarEvent> events);

    public void CancelAllLocalNotifications();
    /// <summary>
    /// Set up new calendar event notifications based on parameters or cancel them
    /// </summary>
    /// <param name="areNotificationsEnabled">If true notifications will be added, if false currently set notifications will be canceled</param>
    /// <param name="notifyDaysBeforeEvent">Notification will be sent given amount of days before the event</param>
    /// <param name="notifyAtTimeOfDay">Notification will be sent at given time of day ignoring time set in event</param>
    public Task<List<GoogleCalendarEvent>> RefreshNotificationsAsync(bool areNotificationsEnabled, int notifyDaysBeforeEvent, TimeSpan notifyAtTimeOfDay);

    public Task<List<GoogleCalendarEvent>> ScheduleNotificationsAsync(List<GoogleCalendarEvent> events, int notifyDaysBeforeEvent, TimeSpan notifyAtTimeOfDay);

    /// <summary>
    /// Save events from server and handle local notifications
    /// WARNING: this method can only take events from one google calendar, using events from multiple calendars will throw exception
    /// </summary>
    /// <param name="events"></param>
    /// <returns></returns>
    public Task<(List<GoogleCalendarEvent> localExceptServer, List<GoogleCalendarEvent> serverExceptLocal)>
        SaveEventsFromServerAndHandleLocalNotificationsAsync(List<GoogleCalendarEvent> events);
}