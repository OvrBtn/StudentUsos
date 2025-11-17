using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CustomSchedule;
using StudentUsos.Features.Activities.Models;
using StudentUsos.Features.Activities.Repositories;
using StudentUsos.Features.Activities.Services;
using StudentUsos.Features.Authorization.Services;
using StudentUsos.Features.Person.Models;
using StudentUsos.Features.Settings.Views.Subpages;
using StudentUsos.Resources.LocalizedStrings;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;

namespace StudentUsos.Features.Activities.Views;

public partial class ActivitiesViewModel : BaseViewModel
{
    IApplicationService applicationService;
    IActivitiesRepository activitiesRepository;
    IActivitiesService activitiesService;
    ILogger? logger;
    public ActivitiesViewModel(IApplicationService applicationService,
        IActivitiesRepository activitiesRepository,
        IActivitiesService activitiesService,
        ILogger? logger = null)
    {
        this.applicationService = applicationService;
        this.activitiesRepository = activitiesRepository;
        this.activitiesService = activitiesService;
        this.logger = logger;

        TodayCommand = new Command(() => { DatePicked = DateTime.Today; DateOnlyPicked = DateOnly.FromDateTime(DateTimeOffset.Now.DateTime); });
        NextCommand = new Command(() => { DatePicked = DatePicked.AddDays(1); });
        PreviousCommand = new Command(() => { DatePicked = DatePicked.AddDays(-1); });
        ScheduleSwipedCommand = new(ScheduleSwiped);

        LoadMoreCommand = new Command(() =>
        {
            applicationService.ShowToast(LocalizedStrings.ActivitiesPage_Downloading);
            applicationService.WorkerThreadInvoke(() => RefreshActivitiesAsync(DateOnlyPicked.ToDateTime(TimeOnly.MinValue)));
        });

        AuthorizationService.OnLogout += AuthorizationService_OnLogout;
        AuthorizationService.OnLoginSucceeded += AuthorizationService_OnAuthorizationFinished;

        DiagnosisSubpage.OnLocalDataReseted += AuthorizationService_OnAuthorizationFinished;
    }

    private void AuthorizationService_OnAuthorizationFinished()
    {
        DatePicked = DateTimeOffset.Now.DateTime;
    }

    private void AuthorizationService_OnLogout()
    {
        ActivitiesStateKey = StateKey.Loading;
    }

    public Lecturer? Lecturer { get; private set; }
    [ObservableProperty] string mainStateKey = StateKey.Loading;

    public void Init(Lecturer lecturer)
    {
        Lecturer = lecturer;
        Init();
    }

    public void Init()
    {
        InitializeTimetableBackground();

        if (Lecturer == null)
        {
            var allActivitiesFromLocalDb = activitiesRepository.GetAllActivities();
            if (allActivitiesFromLocalDb is not null)
            {
                CacheResult(allActivitiesFromLocalDb.Result);

                //CustomSchedule
                for (int i = 0; i < allActivitiesFromLocalDb.Result.Count; i++)
                {
                    var currentTimeTableDay = allActivitiesFromLocalDb.Result[i];
                    ObservableDays[DateOnly.FromDateTime(currentTimeTableDay.Date)] = new(ActivityToCustomSheduleEvent(currentTimeTableDay.Activities));
                }
            }
        }

        ActivitiesStateKey = StateKey.Loading;
        DatePicked = DateTimeOffset.Now.DateTime;
        DateOnlyPicked = DateOnly.FromDateTime(DateTimeOffset.Now.DateTime);

        MainStateKey = StateKey.Loaded;
    }

    //Don't replace with [RelayCommand], the [RelayCommand] will generate IRelayCommand<> but CustomSchedule takes Command<> so it won't work
    [ObservableProperty] Command<Schedule.OnSwipedEventArgs> scheduleSwipedCommand;

    void ScheduleSwiped(Schedule.OnSwipedEventArgs args)
    {
        if (args.CurrentDay != DateOnly.MinValue && ObservableDays.ContainsKey(args.CurrentDay) == false)
        {
            applicationService.ShowToast(LocalizedStrings.ActivitiesPage_Downloading);

            if (args.CurrentDay != DateOnly.MinValue && args.PreviousDay != DateOnly.MinValue && args.CurrentDay < args.PreviousDay)
            {
                applicationService.WorkerThreadInvoke(() => RefreshActivitiesAsync(args.CurrentDay.ToDateTime(TimeOnly.MinValue), -7));
            }
            else
            {
                applicationService.WorkerThreadInvoke(() => RefreshActivitiesAsync(args.CurrentDay.ToDateTime(TimeOnly.MinValue)));
            }
        }
    }

    [ObservableProperty] ICommand loadMoreCommand;

    [ObservableProperty] string activitiesStateKey = StateKey.Loading;

    /// <summary>
    /// Collection which stores activities and provides data for view
    /// </summary>
    [ObservableProperty] ObservableCollection<Activity> activitiesObservable = new ObservableCollection<Activity>();
    [ObservableProperty] ObservableDays observableDays = new();

    /// <summary>
    /// Command used in button to change viewed activities date to previous day
    /// </summary>
    public Command PreviousCommand { get; set; }
    /// <summary>
    /// Command used in button to change viewed activities date to today
    /// </summary>
    public Command TodayCommand { get; set; }
    /// <summary>
    /// Command used in button to change viewed activities date to next day
    /// </summary>
    public Command NextCommand { get; set; }

    [ObservableProperty] bool isWeekEven;

    /// <summary>
    /// Picked date and refresh activities on change
    /// </summary>
    public DateTime DatePicked
    {
        get
        {
            return datePicked;
        }
        set
        {
            if (value == datePicked) return;
            SetProperty(ref datePicked, value);
            //applicationService.WorkerThreadInvoke(() => RefreshActivities(datePicked));
            IsWeekEven = GetIso8601WeekOfYear(datePicked) % 2 == 0;
        }

    }
    DateTime datePicked;

    [ObservableProperty] bool isTodayButtonVisible = false;

    public DateOnly DateOnlyPicked
    {
        get => dateOnlyPicked;
        set
        {
            //Since binding to CustomSchedule uses TwoWay binding this setter gets called 2 times in a row
            if (dateOnlyPicked == value)
            {
                return;
            }
            SetProperty(ref dateOnlyPicked, value);
            if (ObservableDays.ContainsKey(dateOnlyPicked) == false)
            {
                applicationService.ShowToast(LocalizedStrings.ActivitiesPage_Downloading);
                applicationService.WorkerThreadInvoke(() => RefreshActivitiesAsync(datePicked));
            }
            IsWeekEven = GetIso8601WeekOfYear(dateOnlyPicked.ToDateTime(TimeOnly.MinValue)) % 2 == 0;
            DateChanged?.Invoke();
        }
    }
    //this field should not default to DateTime.Now since it will break the return in setter for lecturers, instead it should be set in Init
    DateOnly dateOnlyPicked;

    public event Action DateChanged;

    public static int GetIso8601WeekOfYear(DateTime dateTime)
    {
        DateTime time = new(dateTime.Ticks);
        DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
        if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
        {
            time = time.AddDays(3);
        }
        return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }

    [RelayCommand]
    void DatePickerClicked()
    {
        DatePickerPopup.CreateAndShow((popup, date) =>
        {
            DatePicked = date.ToDateTime(TimeOnly.MinValue);
            DateOnlyPicked = date;
            popup.Close();
        }, DateOnlyPicked);
    }

    //Used to store activities received from USOS API and saved in local database to minimize amount of webrequests
    Dictionary<DateOnly, List<Activity>> cachedActivities = new Dictionary<DateOnly, List<Activity>>();

    async Task RefreshActivitiesAsync(DateTime currentDate, int daysToGet = 7)
    {
        try
        {
            DatePicked = currentDate;
            ActivitiesStateKey = StateKey.Loading;

            //If activities for given date had been saved before load them and return to avoid making unnecessary request
            if (cachedActivities.ContainsKey(DateOnly.FromDateTime(currentDate)))
            {
                applicationService.MainThreadInvoke(() =>
                {
                    ActivitiesObservable = new ObservableCollection<Activity>(cachedActivities[DateOnly.FromDateTime(currentDate)]);
                    if (ActivitiesObservable.Count > 0)
                    {
                        InitializeTimetableBackground((IEnumerable<Activity>)ActivitiesObservable);
                        ScrollToActivityAsync(ActivitiesObservable[0]);
                        ActivitiesStateKey = StateKey.Loaded;
                    }
                    else ActivitiesStateKey = StateKey.Empty;
                });
                return;
            }

            applicationService.MainThreadInvoke(() => ActivitiesObservable.Clear());

            await LoadActivitiesFromApiAsync(currentDate, daysToGet);
        }
        catch (Exception ex)
        {
            logger?.LogCatchedException(ex);
            if (ActivitiesStateKey == StateKey.Loading) ActivitiesStateKey = StateKey.LoadingError;
        }
    }

    async Task LoadActivitiesFromApiAsync(DateTime date, int daysToGet = 7)
    {
        try
        {
            GetActivitiesResult? response;
            if (daysToGet < 0)
            {
                date = date.AddDays(daysToGet + 1);
                daysToGet *= -1;
            }

            if (Lecturer == null)
            {
                response = await activitiesService.GetActivitiesOfCurrentUserApiAsync(date, daysToGet);
            }
            else
            {
                response = await activitiesService.GetActivitiesApiAsync(date, Lecturer.Id, 7);
            }

            if (response is null)
            {
                if (ActivitiesStateKey == StateKey.Loading)
                {
                    ActivitiesStateKey = StateKey.ConnectionError;
                }
                applicationService.MainThreadInvoke(() =>
                {
                    applicationService.ShowToast(LocalizedStrings.Errors_USOSAPIConnectionError);
                });
                return;
            }
            applicationService.MainThreadInvoke(() =>
            {
                ActivitiesObservable = response.Result.First().Activities.ToObservableCollection();
                if (ActivitiesObservable.Count > 0)
                {
                    InitializeTimetableBackground((IEnumerable<Activity>)ActivitiesObservable);
                    ScrollToActivityAsync(ActivitiesObservable[0]);
                    ActivitiesStateKey = StateKey.Loaded;
                }
                else ActivitiesStateKey = StateKey.Empty;

                //CustomSchedule
                for (int i = 0; i < response.Result.Count; i++)
                {
                    var currentTimeTableDay = response.Result[i];
                    ObservableDays[DateOnly.FromDateTime(currentTimeTableDay.Date)] = new(ActivityToCustomSheduleEvent(currentTimeTableDay.Activities));
                }
            });
            CacheResult(response.Result);
        }
        catch (Exception ex)
        {
            logger?.LogCatchedException(ex);
            ActivitiesStateKey = StateKey.LoadingError;
        }
    }

    List<ScheduleEvent> ActivityToCustomSheduleEvent(List<Activity> activities)
    {
        List<ScheduleEvent> scheduleEvents = new();
        for (int i = 0; i < activities.Count; i++)
        {
            scheduleEvents.Add(ActivityToCustomSheduleEvent(activities[i]));
        }
        return scheduleEvents;
    }

    ScheduleEvent ActivityToCustomSheduleEvent(Activity activity)
    {
        ScheduleEvent scheduleEvent = new(activity.StartDateTime, activity.EndDateTime, activity.GetRibbonColor());
        scheduleEvent.OriginalEventReference = activity;
        scheduleEvent.OnClick = (arg) => activity.ClickedCommand?.Execute(arg);
        scheduleEvent.Elements.ItemsSpacing = 15;
        scheduleEvent.Elements.Add(new TextElement() { Text = activity.Name, TextSize = 40, WrapMaxLines = 1 });
        scheduleEvent.Elements.Add(new TextElement() { Text = activity.ClassTypeName, TextSize = 30, WrapMaxLines = 1 });
        scheduleEvent.Elements.Add(new TextElement() { Text = activity.StartTime + " - " + activity.EndTime, TextSize = 30, WrapMaxLines = 1 });
        scheduleEvent.Elements.Add(new TextElement() { Text = LocalizedStrings.ActivitiesPage_Room + " " + activity.RoomNumber, TextSize = 25, WrapMaxLines = 1, TextTopMargin = 10 });
        scheduleEvent.Elements.Add(new TextElement() { Text = activity.BuildingName, TextSize = 25, WrapMaxLines = 1 });
        return scheduleEvent;
    }

    object cacheResultLock;
    void CacheResult(IEnumerable<TimetableDay> timetableDays)
    {
        lock (cacheResultLock)
        {
            foreach (var timetableDay in timetableDays)
            {
                cachedActivities[DateOnly.FromDateTime(timetableDay.Date)] = timetableDay.Activities;
            }
        }
    }

    [ObservableProperty] int timetableHeight = 2400;
    public static int TimetableHourHeight { get; } = 100;
    public List<TimetableBackgroundUnit> BackgroundUnits { get; set; } = new();
    void InitializeTimetableBackground(int minHour = 0, int maxHour = 23)
    {
        BackgroundUnits = new();
        TimeOnly time = TimeOnly.MinValue;
        time = time.AddHours(minHour);
        TimetableHeight = (maxHour - minHour + 1) * TimetableHourHeight;
        for (int i = minHour; i <= maxHour; i++)
        {
            BackgroundUnits.Add(new TimetableBackgroundUnit(time.ToString("HH:mm")));
            time = time.AddHours(1);
        }
    }

    void InitializeTimetableBackground(IEnumerable<Activity> activities)
    {
        if (activities == null) return;
        var activitiesList = activities.ToList();
        if (activitiesList.Count == 0) return;
        double minHour = activitiesList[0].StartDateTime.Hour + activitiesList[0].StartDateTime.Minute / 60f;
        minHour = Math.Floor(minHour) - 1;
        double maxHour = activitiesList[activitiesList.Count - 1].EndDateTime.Hour + activitiesList[activitiesList.Count - 1].EndDateTime.Minute / 60f;
        maxHour = Math.Ceiling(maxHour) + 1;
        activitiesList.ForEach(x => x.LayoutBounds = x.CalculateLayoutBounds());
        activitiesList.ForEach(x => x.LayoutBounds = new Rect(x.LayoutBounds.X, x.LayoutBounds.Y - minHour * TimetableHourHeight, x.LayoutBounds.Width, x.LayoutBounds.Height));
        InitializeTimetableBackground((int)minHour, (int)maxHour);
    }

    async void ScrollToActivityAsync(Activity activity)
    {
        if (activity == null) return;
        var rect = activity.LayoutBounds;
        //skip a few frames so the UI can build
        await Task.Delay(10);
        //TODO: move to code behind
        //activitiesPage.TimetableScrollViewReference.ScrollToAsync(0, rect.Top, false);
    }
}

public class TimetableBackgroundUnit
{
    public string Time { get; set; }

    public TimetableBackgroundUnit(string time)
    {
        Time = time;
    }
}