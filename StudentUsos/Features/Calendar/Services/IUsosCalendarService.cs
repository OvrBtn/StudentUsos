using StudentUsos.Features.Calendar.Models;

namespace StudentUsos.Features.Calendar.Services
{
    public interface IUsosCalendarService
    {
        public Task<List<(DateTime date, bool isPrimaryUpdate, List<UsosCalendarEvent> events)>?> TryFetchingAvailableEventsAsync();
    }
}
