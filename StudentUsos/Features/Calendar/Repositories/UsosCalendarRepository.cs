using StudentUsos.Features.Calendar.Models;
using StudentUsos.Services.LocalNotifications;

namespace StudentUsos.Features.Calendar.Repositories;

public class UsosCalendarRepository : IUsosCalendarRepository
{
    ILocalNotificationsService localNotificationsService;
    ILocalDatabaseManager localDatabaseManager;
    ILogger? logger;
    public UsosCalendarRepository(ILocalNotificationsService localNotificationsService,
        ILocalDatabaseManager localDatabaseManager,
        ILogger? logger = null)
    {
        this.localNotificationsService = localNotificationsService;
        this.localDatabaseManager = localDatabaseManager;
        this.logger = logger;
    }

    public List<UsosCalendarEvent> GetAllEvents()
    {
        return localDatabaseManager.GetAll<UsosCalendarEvent>();
    }

    public List<UsosCalendarEvent> GetEvents(int year, int month)
    {
        int targetMonths = year * 12 + month;
        return localDatabaseManager.GetAll<UsosCalendarEvent>(x => 
        x.Start.Year * 12 + x.Start.Month <= targetMonths &&
        x.End.Year * 12 + x.End.Month >= targetMonths);
    }

    public void SaveEvents(IEnumerable<UsosCalendarEvent> events)
    {
        localDatabaseManager.InsertOrReplaceAll(events);
    }

    public bool IsEmpty()
    {
        return localDatabaseManager.IsTableEmpty<UsosCalendarEvent>();
    }

    public void CancelAllLocalNotifications()
    {
        var events = CancelAllLocalNotificationsInternal();
        SaveEvents(events);
    }

    List<UsosCalendarEvent> CancelAllLocalNotificationsInternal()
    {
        var calendarEvents = GetAllEvents();
        foreach (var item in calendarEvents)
        {
            localNotificationsService.Remove(item.NotificationId);
            item.NotificationId = -1;
        }
        return calendarEvents;
    }

    public async Task<List<UsosCalendarEvent>> ScheduleNotificationsAsync(List<UsosCalendarEvent> events, int notifyDaysBeforeEvent, TimeSpan notifyAtTimeOfDay)
    {
        foreach (var item in events)
        {
            var notifyTime = item.Start.AddDays(notifyDaysBeforeEvent * -1);
            if (notifyTime < DateAndTimeProvider.Current.Now)
            {
                continue;
            }
            var timeOfDay = notifyAtTimeOfDay;
            notifyTime = notifyTime.Date + timeOfDay;
            var id = await localNotificationsService.ScheduleNotificationAsync(new()
            {
                Title = item.Name,
                Description = item.StartString + " " + item.Type,
                ScheduledDateTime = notifyTime
            });
            item.NotificationId = id;
        }
        return events;
    }

    public async Task<List<UsosCalendarEvent>> RefreshNotificationsAsync(bool areNotificationsEnabled, int notifyDaysBeforeEvent, TimeSpan notifyAtTimeOfDay)
    {
        var events = CancelAllLocalNotificationsInternal();
        if (areNotificationsEnabled)
        {
            await ScheduleNotificationsAsync(events, notifyDaysBeforeEvent, notifyAtTimeOfDay);
        }
        SaveEvents(events);
        return events;
    }

    readonly List<UsosCalendarEvent> eventsWithScheduledNotifications = new();
    public async Task<(List<UsosCalendarEvent> localExceptServer, List<UsosCalendarEvent> serverExceptLocal)> SaveEventsFromServerAndHandleLocalNotificationsAsync(
        int year, int month, List<UsosCalendarEvent> events, bool isPrimaryUpdate)
    {
        try
        {
            if (events.Count == 0) return (new(), new());

            var localEvents = GetEvents(year, month);

            Utilities.ListsDifference(localEvents.Where(x => x.isPrimaryUpdate == isPrimaryUpdate),
                events.Where(x => x.isPrimaryUpdate == isPrimaryUpdate),
                out List<UsosCalendarEvent> localExceptApi,
                out List<UsosCalendarEvent> apiExceptLocal, UsosCalendarEvent.AreEqual);

            //remove event when it is added to calendar and saved in local database but isn't present in response from USOS API
            foreach (var item in localExceptApi)
            {
                localNotificationsService.Remove(item.NotificationId);
                localDatabaseManager.Remove(item);
            }

            //add event when it isn't added to calendar and isn't saved in local database but is present in response from USOS API
            foreach (var item in apiExceptLocal)
            {
                //setting up local notification
                if (CalendarSettings.AreCalendarNotificationsEnabled && eventsWithScheduledNotifications.Any(x => x.Id == item.Id) == false)
                {
                    await ScheduleNotificationsAsync(new() { item },
                        CalendarSettings.DaysBeforeCalendarEventToSendNotification,
                        CalendarSettings.TimeOfDayOfCalendarEventNotification);
                    eventsWithScheduledNotifications.Add(item);
                }

            }

            SaveEvents(apiExceptLocal);

            return new(localExceptApi, apiExceptLocal);

        }
        catch (Exception ex)
        {
            logger?.LogCatchedException(ex);
            return new(new(), new());
        }
    }
}