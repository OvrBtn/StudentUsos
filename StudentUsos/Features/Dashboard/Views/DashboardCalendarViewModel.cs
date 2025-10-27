using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using StudentUsos.Features.Calendar;
using StudentUsos.Features.Calendar.Models;
using StudentUsos.Features.Calendar.Repositories;
using StudentUsos.Features.Calendar.Services;
using StudentUsos.Features.Dashboard.Models;
using System.Collections.ObjectModel;

namespace StudentUsos.Features.Dashboard.Views;

public partial class DashboardCalendarViewModel : BaseViewModel
{
    IUsosCalendarService usosCalendarService;
    IUsosCalendarRepository usosCalendarRepository;
    IGoogleCalendarService googleCalendarService;
    IGoogleCalendarRepository googleCalendarRepository;
    IApplicationService applicationService;
    ILogger? logger;
    public DashboardCalendarViewModel(IUsosCalendarService usosCalendarService,
        IUsosCalendarRepository usosCalendarRepository,
        IGoogleCalendarService googleCalendarService,
        IGoogleCalendarRepository googleCalendarRepository,
        IApplicationService applicationService,
        ILogger? logger = null)
    {
        this.usosCalendarService = usosCalendarService;
        this.usosCalendarRepository = usosCalendarRepository;
        this.googleCalendarService = googleCalendarService;
        this.googleCalendarRepository = googleCalendarRepository;
        this.applicationService = applicationService;
        this.logger = logger;
    }


    public async Task InitAsync()
    {
        CalendarStateKey = StateKey.Loading;
        await LoadCalendarEvents();
    }

    public event Action OnSynchronousLoadingFinished;
    public event Action OnAsynchronousLoadingFinished;

    [ObservableProperty] string calendarStateKey = StateKey.Loading;

    [ObservableProperty] ObservableCollection<UsosCalendarEvent> events = new();
    [ObservableProperty] ObservableCollection<GoogleCalendarEvent> eventsGoogle = new();

    [ObservableProperty] ObservableCollection<CalendarEvent> calendarEvents = new();

    const int WebrequestDelay = 1000;
    const int MaxCalendarEvents = 5;
    async Task LoadCalendarEvents()
    {
        try
        {
            CalendarEvents.Clear();

            LoadCalendarEventsLocal();
            OnSynchronousLoadingFinished?.Invoke();

            if (CalendarEvents.Count != 0)
            {
                await Task.Delay(WebrequestDelay);
            }

            await LoadCalendarEventsServerAsync();
            OnAsynchronousLoadingFinished?.Invoke();
        }
        catch (Exception ex)
        {
            logger?.LogCatchedException(ex);
        }
    }

    async Task LoadCalendarEventsServerAsync()
    {
        var usosEvents = await usosCalendarService.TryFetchingAvailableEventsAsync();
        List<UsosCalendarEvent> usosEventsLinearized;
        if (usosEvents == null)
        {
            usosEventsLinearized = usosCalendarRepository.GetAllEvents();
        }
        else
        {
            usosEventsLinearized = usosEvents.SelectMany(x => x.events).ToList();
            foreach (var item in usosEvents)
            {
                await usosCalendarRepository.SaveEventsFromServerAndHandleLocalNotificationsAsync(item.date.Year, item.date.Month, item.events, item.isPrimaryUpdate);
            }

            if (usosEvents.Count != CalendarSettings.MonthsToGetInTotal)
            {
                var date = DateTimeOffset.Now.DateTime;
                for (int i = 0; i < CalendarSettings.MonthsToGetInTotal; i++)
                {
                    if (usosEvents.Any(x => x.date.Year == date.Year && x.date.Month == date.Month) == false)
                    {
                        usosEventsLinearized.AddRange(usosCalendarRepository.GetEvents(date.Year, date.Month));
                    }
                    date = date.AddMonths(1);
                }
                Utilities.RemoveDuplicates(usosEventsLinearized, UsosCalendarEvent.AreEqual);
            }
        }

        List<GoogleCalendarEvent> googleCalendarEventsLinearized = new();
        var googleCalendars = googleCalendarRepository.GetAllCalendars();
        foreach (var calendar in googleCalendars)
        {
            var googleEvents = await googleCalendarService.GetGoogleCalendarEventsAsync(calendar);
            if (googleEvents == null)
            {
                googleCalendarEventsLinearized = googleCalendarRepository.GetAllEvents().Where(x => x.CalendarName == calendar.Name).ToList();
            }
            else
            {
                await googleCalendarRepository.SaveEventsFromServerAndHandleLocalNotificationsAsync(googleEvents);
                googleCalendarEventsLinearized.AddRange(googleEvents);
            }
        }

        var grouped = GroupCalendarEvents(usosEventsLinearized, googleCalendarEventsLinearized);
        SetCalendarEvents(grouped);
    }

    void LoadCalendarEventsLocal()
    {
        var usosEventsLocal = usosCalendarRepository.GetAllEvents();
        var googleCalendarEventsLocal = googleCalendarRepository.GetAllEvents();

        var events = GroupCalendarEvents(usosEventsLocal, googleCalendarEventsLocal);
        SetCalendarEvents(events);
    }

    List<CalendarEvent> GroupCalendarEvents(List<UsosCalendarEvent> usosEvents, List<GoogleCalendarEvent> googleEvents)
    {
        var now = DateTimeOffset.Now.DateTime;
        List<CalendarEvent> calendarEvents = new();

        var usosEventsSorted = usosEvents.Where(x => x.End >= now)
            .OrderBy(x => x.Start)
            .Take(MaxCalendarEvents);
        foreach (var item in usosEventsSorted)
        {
            calendarEvents.Add(new(item));
        }

        for (int i = 0; i < googleEvents.Count; i++)
        {
            if (googleEvents[i].Start < now && googleEvents[i].End < now)
            {
                continue;
            }
            calendarEvents.Add(new(googleEvents[i]));
            if (calendarEvents.Count > MaxCalendarEvents)
            {
                break;
            }
        }

        calendarEvents = calendarEvents.Where(x => now <= x.EndDateTime)
            .OrderBy(x => x.StartDateTime)
            .Take(MaxCalendarEvents).ToList();
        return calendarEvents;
    }

    void SetCalendarEvents(List<CalendarEvent> calendarEvents)
    {
        if (Utilities.CompareCollections(CalendarEvents, calendarEvents, CalendarEvent.AreEqual) == false)
        {
            applicationService.MainThreadInvoke(() =>
            {
                CalendarEvents = calendarEvents.ToObservableCollection();
            });
        }
        if (CalendarEvents.Count > 0) CalendarStateKey = StateKey.Loaded;
    }
}