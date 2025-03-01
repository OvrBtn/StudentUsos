using StudentUsos.Features.Calendar.Models;
using StudentUsos.Resources.LocalizedStrings;
using StudentUsos.Services.LocalNotifications;

namespace StudentUsos.Features.Calendar.Repositories
{
    public class GoogleCalendarRepository : IGoogleCalendarRepository
    {
        ILocalNotificationsService localNotificationsService;
        ILocalDatabaseManager localDatabaseManager;
        public GoogleCalendarRepository(ILocalNotificationsService localNotificationsService, ILocalDatabaseManager localDatabaseManager)
        {
            this.localNotificationsService = localNotificationsService;
            this.localDatabaseManager = localDatabaseManager;
        }

        public List<GoogleCalendar> GetAllCalendars()
        {
            return localDatabaseManager.GetAll<GoogleCalendar>();
        }

        public void SaveCalendar(GoogleCalendar calendar)
        {
            localDatabaseManager.Insert(calendar);
        }

        public void RemoveCalendar(GoogleCalendar calendar)
        {
            var events = localDatabaseManager.GetAll<GoogleCalendarEvent>().Where(x => x.CalendarName == calendar.Name).ToList();
            localDatabaseManager.Remove<GoogleCalendarEvent>($"CalendarName = \"{calendar.Name}\"");
            localDatabaseManager.Remove(calendar);
            foreach (var item in events)
            {
                localNotificationsService.Remove(item.NotificationId);
            }
        }

        public List<GoogleCalendarEvent> GetAllEvents()
        {
            var calendars = GetAllCalendars();
            var events = localDatabaseManager.GetAll<GoogleCalendarEvent>();
            foreach (var item in events)
            {
                item.Calendar = calendars.First(x => x.Name == item.CalendarName);
            }
            return events;
        }

        public void InsertOrReplaceEvents(IEnumerable<GoogleCalendarEvent> events)
        {
            localDatabaseManager.InsertOrReplaceAll(events);
        }

        public void InsertEvents(IEnumerable<GoogleCalendarEvent> events)
        {
            localDatabaseManager.InsertAll(events);
        }

        public void CancelAllLocalNotifications()
        {
            var events = CancelAllLocalNotificationsInternal();
            InsertOrReplaceEvents(events);
        }

        List<GoogleCalendarEvent> CancelAllLocalNotificationsInternal()
        {
            var googleCalendarEvents = GetAllEvents();
            foreach (var item in googleCalendarEvents)
            {
                localNotificationsService.Remove(item.NotificationId);
                item.NotificationId = -1;
            }
            return googleCalendarEvents;
        }

        public async Task<List<GoogleCalendarEvent>> ScheduleNotificationsAsync(List<GoogleCalendarEvent> events, int notifyDaysBeforeEvent, TimeSpan notifyAtTimeOfDay)
        {
            foreach (var item in events)
            {
                var notifyTime = item.Start.AddDays(notifyDaysBeforeEvent * -1);
                if (notifyTime < DateTimeOffset.Now.DateTime)
                {
                    continue;
                }
                var timeOfDay = notifyAtTimeOfDay;
                notifyTime = notifyTime.Date + timeOfDay;
                var id = await localNotificationsService.ScheduleNotificationAsync(new()
                {
                    Title = item.Title,
                    Description = item.StartString + " " + item.Description,
                    Subtitle = item.CalendarName,
                    ScheduledDateTime = notifyTime,
                    Group = LocalizedStrings.CalendarPage_GoogleCalendar,
                });
                item.NotificationId = id;
            }
            return events;
        }

        public async Task<List<GoogleCalendarEvent>> RefreshNotificationsAsync(bool areNotificationsEnabled, int notifyDaysBeforeEvent, TimeSpan notifyAtTimeOfDay)
        {
            var events = CancelAllLocalNotificationsInternal();
            if (areNotificationsEnabled)
            {
                await ScheduleNotificationsAsync(events, notifyDaysBeforeEvent, notifyAtTimeOfDay);
            }
            InsertOrReplaceEvents(events);
            return events;
        }

        public async Task<(List<GoogleCalendarEvent> localExceptServer, List<GoogleCalendarEvent> serverExceptLocal)>
            SaveEventsFromServerAndHandleLocalNotificationsAsync(List<GoogleCalendarEvent> events)
        {
            try
            {
                if (events.Count() == 0) return new(new(), new());

                string calendarName = events[0].CalendarName;
                foreach (var item in events)
                {
                    if (item.CalendarName != calendarName)
                    {
                        throw new Exception("Events should not be from many different calendars");
                    }
                }

                var eventsLocal = GetAllEvents();

                var localFiltered = eventsLocal.Where(x => x.CalendarName == events[0].CalendarName).ToList();
                Utilities.ListsDifference(localFiltered, events,
                    out List<GoogleCalendarEvent> localExceptApi, out List<GoogleCalendarEvent> apiExceptLocal,
                    GoogleCalendarEvent.AreEqual);

                //when event is added to calendar and saved in local database but isn't present in response from Google Calendar 
                foreach (var item in localExceptApi)
                {
                    localNotificationsService.Remove(item.NotificationId);
                    localDatabaseManager.Remove(item);
                }

                //when event isn't added to calendar and isn't saved in local database but is present in response from Google Calendar 
                foreach (var item in apiExceptLocal)
                {
                    if (CalendarSettings.AreCalendarNotificationsEnabled)
                    {
                        await ScheduleNotificationsAsync(new() { item },
                            CalendarSettings.DaysBeforeCalendarEventToSendNotification,
                            CalendarSettings.TimeOfDayOfCalendarEventNotification);
                    }
                }

                InsertEvents(apiExceptLocal);

                return new(localExceptApi, apiExceptLocal);
            }
            catch (Exception ex)
            {
                Utilities.ShowError(ex);
                return (new(), new());
            }
        }
    }
}
