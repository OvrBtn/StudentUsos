using StudentUsos.Features.Calendar.Models;
using StudentUsos.Features.Calendar.Repositories;
using StudentUsos.Features.StudentProgrammes.Models;
using StudentUsos.Features.StudentProgrammes.Services;
using StudentUsos.Services.ServerConnection;
using System.Text.Json;

namespace StudentUsos.Features.Calendar.Services
{
    public class UsosCalendarService : IUsosCalendarService
    {
        IUsosCalendarRepository usosCalendarRepository;
        IServerConnectionManager serverConnectionManager;
        IStudentProgrammeService studentProgrammeService;
        ILocalStorageManager localStorageManager;
        ILogger? logger;
        public UsosCalendarService(IUsosCalendarRepository usosCalendarRepository,
            IServerConnectionManager serverConnectionManager,
                IStudentProgrammeService studentProgrammeService,
                ILocalStorageManager localStorageManager,
                ILogger? logger = null)
        {
            this.usosCalendarRepository = usosCalendarRepository;
            this.serverConnectionManager = serverConnectionManager;
            this.studentProgrammeService = studentProgrammeService;
            this.localStorageManager = localStorageManager;
            this.logger = logger;
        }

        bool isInitialized;
        void EnsureInitialized()
        {
            if (isInitialized)
            {
                return;
            }
            isInitialized = true;

            bool isTableEmpty = usosCalendarRepository.IsEmpty();
            //if dates of last updates with USOS API is not set or if there are no record in API's events table in local database set last update dates to force making webrequests
            if (localStorageManager.TryGettingData(LocalStorageKeys.LastPrimaryCalendarUpdate, out var lastPrimaryEventsUpdate) == false || isTableEmpty)
            {
                localStorageManager.SetData(LocalStorageKeys.LastPrimaryCalendarUpdate, DateTimeOffset.Now.DateTime.AddYears(-1).ToString());
            }
            if (localStorageManager.TryGettingData(LocalStorageKeys.LastSecondaryCalendarUpdate, out var lastSecondaryUpdate) == false || isTableEmpty)
            {
                localStorageManager.SetData(LocalStorageKeys.LastSecondaryCalendarUpdate, DateTimeOffset.Now.DateTime.AddYears(-1).ToString());
            }
        }

        public async Task<(DateTime date, bool isPrimaryUpdate, List<UsosCalendarEvent>)?> GetEventsAsync(string facultyId, int month, int year, bool isPrimaryUpdate)
        {
            DateTime firstDayOfMonth = new DateTime(year, month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddDays(DateTime.DaysInMonth(firstDayOfMonth.Year, firstDayOfMonth.Month) - 1);
            var args = new Dictionary<string, string>
            {
                { "faculty_id", facultyId },
                { "start_date", firstDayOfMonth.ToString("yyyy-MM-dd") },
                { "end_date", lastDayOfMonth.ToString("yyyy-MM-dd") }
            };
            var response = await serverConnectionManager.SendRequestToUsosAsync("services/calendar/search", args);
            if (response == null)
            {
                return null;
            }
            var deserialized = JsonSerializer.Deserialize(response, UsosCalendarEventJsonContext.Default.ListUsosCalendarEvent);
            if (deserialized is null)
            {
                return null;
            }
            deserialized.ForEach(x => x.isPrimaryUpdate = isPrimaryUpdate);
            return new(new(year, month, 1), isPrimaryUpdate, deserialized);
        }

        static StudentProgramme? studentProgramme = null;
        public async Task<List<(DateTime date, bool isPrimaryUpdate, List<UsosCalendarEvent> events)>?> TryFetchingAvailableEventsAsync()
        {
            try
            {
                EnsureInitialized();

                if (studentProgramme == null)
                {
                    studentProgramme = await studentProgrammeService.GetCurrentStudentProgrammeAsync();
                }
                if (studentProgramme == null)
                {
                    return null;
                }
                string facultyId = studentProgramme.FacultyId;

                bool canGetPrimary = localStorageManager.TryGettingData(LocalStorageKeys.LastPrimaryCalendarUpdate, out var lastPrimaryEventsUpdate) &&
                                       (DateTime.TryParse(lastPrimaryEventsUpdate, out DateTime lastPrimaryEventsDate) &&
                                        DateTime.Compare(DateTimeOffset.Now.DateTime, lastPrimaryEventsDate.AddDays(CalendarSettings.PrimaryEventsUpdateInterval)) >= 0);

                bool canGetSecondary = localStorageManager.TryGettingData(LocalStorageKeys.LastSecondaryCalendarUpdate, out var lastSecondaryUpdate) &&
                                      (DateTime.TryParse(lastSecondaryUpdate, out DateTime lastSecondaryDate) &&
                                       DateTime.Compare(DateTimeOffset.Now.DateTime, lastSecondaryDate.AddDays(CalendarSettings.SecondaryEventsUpdateInterval)) >= 0);

                List<Task<(DateTime date, bool isPrimaryUpdate, List<UsosCalendarEvent>)?>> tasks = new();
                for (int i = 0; i < CalendarSettings.MonthsToGetInTotal; i++)
                {
                    var date = DateTimeOffset.Now.DateTime.AddMonths(i);
                    if (i < CalendarSettings.PrimaryUpdateMonths && canGetPrimary)
                    {
                        tasks.Add(GetEventsAsync(facultyId, date.Month, date.Year, true));
                    }

                    if (i >= CalendarSettings.PrimaryUpdateMonths && canGetSecondary)
                    {
                        tasks.Add(GetEventsAsync(facultyId, date.Month, date.Year, false));
                    }
                }

                var results = await Task.WhenAll(tasks);

                //if even one request has failed whole batch has to be treated as a fail
                foreach (var item in results)
                {
                    if (item is null)
                    {
                        return null;
                    }
                }

                if (canGetPrimary)
                {
                    localStorageManager.SetData(LocalStorageKeys.LastPrimaryCalendarUpdate, DateTimeOffset.Now.DateTime.ToString());
                }
                if (canGetSecondary)
                {
                    localStorageManager.SetData(LocalStorageKeys.LastSecondaryCalendarUpdate, DateTimeOffset.Now.DateTime.ToString());
                }

                return results.Select(x => x!.Value).ToList();
            }
            catch (Exception ex)
            {
                logger?.LogCatchedException(ex);
                return null;
            }
        }
    }
}
