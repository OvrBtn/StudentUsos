using StudentUsos.Features.Calendar.Models;

namespace StudentUsos.Features.Calendar.Services;

public interface IGoogleCalendarService
{
    /// <summary>
    /// Get specific Google Calendar by its url pointing to .ics file
    /// </summary>
    /// <param name="calendar"></param>
    public Task<List<GoogleCalendarEvent>> GetGoogleCalendarEventsAsync(GoogleCalendar calendar);
    public List<GoogleCalendarEvent> GetGoogleCalendarEvents(string icsFileContent, GoogleCalendar calendar);

    public Task<GoogleCalendar?> CreateCalendarAsync(string url);
}