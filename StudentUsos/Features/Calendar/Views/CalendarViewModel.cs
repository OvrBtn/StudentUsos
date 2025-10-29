using CommunityToolkit.Mvvm.ComponentModel;
using CustomCalendar;
using StudentUsos.Features.Calendar.Models;
using StudentUsos.Features.Calendar.Repositories;
using StudentUsos.Features.Calendar.Services;
using System.Collections.ObjectModel;

namespace StudentUsos.Features.Calendar.Views;

public partial class CalendarViewModel : BaseViewModel
{
    IUsosCalendarService usosCalendarService;
    IUsosCalendarRepository usosCalendarRepository;
    IGoogleCalendarService googleCalendarService;
    IGoogleCalendarRepository googleCalendarRepository;
    IApplicationService applicationService;
    ILogger? logger;
    public CalendarViewModel(IUsosCalendarService usosCalendarService,
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

        CustomCalendarClickedCommand = new(CustomCalendarClicked);

        GoogleCalendars = googleCalendarRepository.GetAllCalendars();
    }

    public async Task InitDelayedAsync()
    {
        await Task.Delay(10);
        _ = applicationService.WorkerThreadInvoke(() => InitAsync());
    }

    [ObservableProperty] string mainStateKey = StateKey.Loading;

    /// <summary>
    /// The date that is displayed above events and which is equal to clicked day
    /// </summary>
    [ObservableProperty] string titleDate;

    /// <summary>
    /// Months containing days with events from calendars, displayed in UI
    /// </summary>
    [ObservableProperty] ObservableCollection<CalendarMonth> calendarMonths = new();

    [ObservableProperty] ObservableCollection<UsosCalendarEvent> eventsData = new();
    [ObservableProperty] ObservableCollection<GoogleCalendarEvent> eventsDataGoogleCalendar = new();
    public List<GoogleCalendar> GoogleCalendars { get; set; }

    public async Task InitAsync()
    {
        var emptyCalendarMonths = GenerateEmptyMonths(CalendarSettings.MonthsToGetInTotal);

        await LoadCalendarAsync(emptyCalendarMonths, EventsFromLocalDatabaseLoaded, EventsFromWebrequestsLoaded);

        void EventsFromLocalDatabaseLoaded(ObservableCollection<CalendarMonth> calendarMonths)
        {
            applicationService.MainThreadInvoke(() => CalendarMonths = calendarMonths);
            MainStateKey = StateKey.Loaded;
        }

        void EventsFromWebrequestsLoaded(ObservableCollection<CalendarMonth> calendarMonths)
        {
            applicationService.MainThreadInvoke(() => CalendarMonths = calendarMonths);
            MainStateKey = StateKey.Loaded;
        }

    }

    /// <summary>
    /// Load whole calendar with all it's events to given collection
    /// </summary>
    /// <param name="calendarMonthsToEdit">Collection of months to fill with events</param>
    /// <param name="onSynchronousLoadingFinished">fired when USOS API and Google Calendar events are loaded from local database</param>
    /// <param name="onAsynchronousLoadingFinished">fired when USOS API and Google Calendar events are loaded from webrequests</param>
    /// <param name="asynchronousOperationsDelay">delay [in miliseconds] before executing asynchronous webrequests, default value is 0 meaning no delay,
    /// if there are no events from local database delay will be ignored</param>
    /// <returns></returns>
    public async Task LoadCalendarAsync(ObservableCollection<CalendarMonth> calendarMonthsToEdit, Action<ObservableCollection<CalendarMonth>> onSynchronousLoadingFinished,
        Action<ObservableCollection<CalendarMonth>> onAsynchronousLoadingFinished, int asynchronousOperationsDelay = 0)
    {

        int finishedCounter = 0;
        const int amountOfExpectedCallbacks = 2;
        LoadUsosEventsLocal(calendarMonthsToEdit);
        LoadGoogleEventsLocal(calendarMonthsToEdit);
        onSynchronousLoadingFinished?.Invoke(calendarMonthsToEdit);

        if (asynchronousOperationsDelay != 0 && calendarMonthsToEdit.Count > 0) await Task.Delay(asynchronousOperationsDelay);
        await LocalUsosEventsServerAsync(calendarMonthsToEdit, OnFinished);
        await LoadlAllGoogleCalendarsFromServerAsync(calendarMonthsToEdit, OnFinished);

        void OnFinished()
        {
            finishedCounter++;
            if (finishedCounter >= amountOfExpectedCallbacks)
            {
                onAsynchronousLoadingFinished?.Invoke(calendarMonthsToEdit);
            }
        }
    }

    [ObservableProperty] CustomObservableEvents customObservableEvents = new();
    [ObservableProperty] Command<CustomDay> customCalendarClickedCommand;
    [ObservableProperty] DateTime customCalendarMinDate = DateTimeOffset.Now.DateTime;
    [ObservableProperty] DateTime customCalendarMaxDate = DateTimeOffset.Now.DateTime.AddMonths(CalendarSettings.MonthsToGetInTotal - 1);

    void CustomCalendarClicked(CustomDay customDay)
    {
        TitleDate = customDay.DateOnly.ToString("dd.MM.yyyy");
        if (customDay.Events.Count > 0 && customDay.Events[0].OriginalDayReference is CalendarDay)
        {
            if (customDay.Events[0].OriginalDayReference is CalendarDay calendarDay)
            {
                calendarDay.OnClick?.Execute(null);
            }
        }
        else
        {
            EventsData = new();
            EventsDataGoogleCalendar = new();
        }
    }

    /// <summary>
    /// Generate CalendarMonths with empty days
    /// </summary>
    /// <param name="amountOfMonths">Amount of months to generate from today</param>
    public ObservableCollection<CalendarMonth> GenerateEmptyMonths(int amountOfMonths)
    {
        var date = DateTimeOffset.Now.DateTime;
        date = date.AddDays(-1 * date.Day + 1);
        date = Utilities.SetTimeToZero(date);
        ObservableCollection<CalendarMonth> calendarMonths = new();
        for (int i = 0; i < amountOfMonths; i++)
        {
            calendarMonths.Add(new CalendarMonth(date, OnDayClicked, CustomObservableEvents));
            date = date.AddMonths(1);
        }
        return calendarMonths;
    }

    void OnDayClicked(CalendarDay day)
    {
        EventsData = day.EventsUsos;
        EventsDataGoogleCalendar = day.EventsGoogleCalendar;
        TitleDate = day.FullDateTime.ToString("dd.MM.yyyy");
    }


    public void LoadGoogleEventsLocal(ObservableCollection<CalendarMonth> calendarMonths)
    {
        SetGoogleEventsLocal(googleCalendarRepository.GetAllEvents(), calendarMonths);
    }

    public void SetGoogleEventsLocal(IEnumerable<GoogleCalendarEvent> googleEvents, IEnumerable<CalendarMonth> calendarMonths)
    {
        foreach (var googleEvent in googleEvents)
        {
            googleEvent.Calendar = GoogleCalendars.First(x => x.Name == googleEvent.CalendarName);
            var date = googleEvent.Start;
            while (date <= googleEvent.End)
            {
                var month = calendarMonths.FirstOrDefault(x => x.Month == date.Month && x.Year == date.Year);
                if (month == null)
                {
                    date = date.AddDays(1);
                    continue;
                }
                var day = month.Days.FirstOrDefault(x => x.Day == date.Day);
                if (day == null)
                {
                    date = date.AddDays(1);
                    continue;
                }
                day.AddEvent(googleEvent);
                date = date.AddDays(1);
            }
        }
    }

    public async Task LoadlAllGoogleCalendarsFromServerAsync(ObservableCollection<CalendarMonth> calendarMonths, Action? finished = null)
    {
        try
        {
            var calendars = googleCalendarRepository.GetAllCalendars();
            foreach (var calendar in calendars)
            {
                var events = await googleCalendarService.GetGoogleCalendarEventsAsync(calendar);
                SetGoogleEventsServer(events, calendarMonths);
            }
            finished?.Invoke();
        }
        catch (Exception ex)
        {
            logger?.LogCatchedException(ex);
            finished?.Invoke();
        }
    }

    public void SetGoogleEventsServer(List<GoogleCalendarEvent> events, ObservableCollection<CalendarMonth> calendarMonths)
    {
        try
        {
            if (events.Count() == 0) return;

            googleCalendarRepository.SaveEventsFromServerAndHandleLocalNotificationsAsync(events);

            foreach (var month in calendarMonths)
            {
                foreach (var day in month.Days)
                {
                    //find events that occur in currently given day
                    var eventsFound = events.Where(x => Utilities.CheckIfBetweenDates(day.FullDateTime, x.Start, x.End)).ToList();
                    Utilities.ListsDifference(day.EventsGoogleCalendar.Where(x => x.Calendar?.Name == events[0].Calendar?.Name).ToList(),
                    eventsFound, out List<GoogleCalendarEvent> localExceptApi, out List<GoogleCalendarEvent> apiExceptLocal, GoogleCalendarEvent.AreEqual);

                    //when event is added to calendar and saved in local database but isn't present in API response
                    foreach (var item in localExceptApi)
                    {
                        day.RemoveEvent(item);
                    }

                    //when event isn't added to calendar and isn't saved in local database but is present in API response
                    foreach (var item in apiExceptLocal)
                    {
                        day.AddEvent(item);
                    }
                }
            }
        }
        catch (Exception ex) { logger?.LogCatchedException(ex); }
    }

    public void LoadUsosEventsLocal(ObservableCollection<CalendarMonth> calendarMonths)
    {
        try
        {
            for (int i = 0; i < calendarMonths.Count; i++)
            {
                var events = usosCalendarRepository.GetEvents(calendarMonths[i].Year, calendarMonths[i].Month);
                calendarMonths[i].SetEventsFromLocalDatabase(events);
            }
        }
        catch (Exception ex) { logger?.LogCatchedException(ex); }
    }

    public async Task LocalUsosEventsServerAsync(ObservableCollection<CalendarMonth> calendarMonths, Action? finished = null)
    {
        try
        {
            var results = await usosCalendarService.TryFetchingAvailableEventsAsync();
            if (results == null)
            {
                finished?.Invoke();
                return;
            }
            for (int i = 0; i < results.Count; i++)
            {
                await usosCalendarRepository.SaveEventsFromServerAndHandleLocalNotificationsAsync(results[i].date.Year, results[i].date.Month, results[i].events, results[i].isPrimaryUpdate);
                calendarMonths[i].SetEventsFromServer(results[i].events, results[i].isPrimaryUpdate);
            }

            finished?.Invoke();
        }
        catch (Exception ex)
        {
            logger?.LogCatchedException(ex);
            finished?.Invoke();
        }
    }
}
