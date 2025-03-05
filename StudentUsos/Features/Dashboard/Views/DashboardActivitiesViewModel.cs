using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using StudentUsos.Features.Activities.Models;
using StudentUsos.Features.Activities.Repositories;
using StudentUsos.Features.Activities.Services;
using System.Collections.ObjectModel;

namespace StudentUsos.Features.Dashboard.Views;

public partial class DashboardActivitiesViewModel : BaseViewModel
{
    IActivitiesRepository activitiesRepository;
    IActivitiesService activitiesService;
    IApplicationService applicationService;
    ILogger? logger;

    public DashboardActivitiesViewModel(
        IActivitiesRepository activitiesRepository,
        IActivitiesService activitiesService,
        IApplicationService applicationService,
        ILogger? logger = null)
    {
        this.activitiesRepository = activitiesRepository;
        this.activitiesService = activitiesService;
        this.applicationService = applicationService;
        this.logger = logger;
    }

    public event Action OnSynchronousLoadingFinished;
    public event Action OnAsynchronousLoadingFinished;

    /// <summary>
    /// Timer used to refresh activity's timer
    /// </summary>
    System.Timers.Timer timer = new(1000);

    public void Init()
    {
        timer.Elapsed += (source, e) => RefreshTime();
        timer.AutoReset = true;
        timer.Enabled = true;

        //Title above list of activities
        //Invoked on worker thread so initialization of timezone database
        //on Android doesn't slow down the startup time
        ApplicationService.Default.WorkerThreadInvoke(() =>
        {
            ActivitiesTitle = DateTimeOffset.Now.DateTime.ToString("dd MMMM yyyy, dddd");
        });

        ActivitiesStateKey = StateKey.Loading;

        _ = LoadActivitiesAsync();
    }

    /// <summary>
    /// Text displayed above activities equal to today
    /// </summary>
    [ObservableProperty] string activitiesTitle;

    [ObservableProperty] ObservableCollection<Activity> activities = new();

    [ObservableProperty] Activity currentItemInCarousel;

    [ObservableProperty] string activitiesStateKey = StateKey.Loading;


    bool firstTimeReferesh = true;
    public event Action<Activity> OnCurrentActivityChanged;

    /// <summary>
    /// Handles timer's Elapsed event and refreshes timers in activities
    /// </summary>
    void RefreshTime()
    {
        for (int i = 0; i < Activities.Count; i++)
        {
            var current = Activities[i];
            //DateTime representing midnight
            DateTime timeZero = Utilities.SetTimeToZero(DateTimeOffset.Now.DateTime);
            //If activity is the first one during the day and it hasn't started yet set timer to time remaining to it's beggining
            if (i == 0 && DateTime.Compare(DateTimeOffset.Now.DateTime, current.StartDateTime) <= 0 && DateTime.Compare(DateTimeOffset.Now.DateTime, timeZero) > 0)
            {
                Update(current.StartDateTime - DateTimeOffset.Now.DateTime, current.StartDateTime - DateTime.Today, i);
            }
            //Set timer to time left to end of the break between classes
            if (i > 0 && DateTime.Compare(DateTimeOffset.Now.DateTime, Activities[i - 1].EndDateTime) > 0 && DateTime.Compare(DateTimeOffset.Now.DateTime, current.StartDateTime) < 0)
            {
                Update(current.StartDateTime - DateTimeOffset.Now.DateTime, current.StartDateTime - Activities[i - 1].EndDateTime, i);
            }
            //if activity has started set timer to time left to it's end
            if (DateTime.Compare(DateTimeOffset.Now.DateTime, Activities[i].StartDateTime) > 0 && DateTime.Compare(DateTimeOffset.Now.DateTime, current.EndDateTime) < 0)
            {
                Update(current.EndDateTime - DateTimeOffset.Now.DateTime, current.EndDateTime - current.StartDateTime, i);
            }

            //Handles updating timer
            void Update(TimeSpan currentDifference, TimeSpan maxDifference, int itemIndex)
            {
                if (firstTimeReferesh)
                {
                    CurrentItemInCarousel = Activities[i];
                    OnCurrentActivityChanged?.Invoke(Activities[i]);
                    firstTimeReferesh = false;
                }
                //hide not active timers
                for (int i = 0; i < Activities.Count; i++)
                {
                    if (i == itemIndex)
                    {
                        Activities[i].IsTimerVisible = true;
                    }
                    else
                    {
                        Activities[i].IsTimerVisible = false;
                    }
                }
                //using difference.ToString("HH:mm:ss") throws exception for some reason
                Activities[i].TimerValue = currentDifference.Hours.ToString("00") + ":" + currentDifference.Minutes.ToString("00") + ":" + currentDifference.Seconds.ToString("00");
                if (maxDifference.TotalSeconds != 0) current.TimerProgress = (float)((maxDifference.TotalSeconds - currentDifference.TotalSeconds) / maxDifference.TotalSeconds);
            }
        }
    }

    async Task LoadActivitiesAsync()
    {
        try
        {
            var activitiesFromLocalDb = LoadActivitiesLocalDb();
            OnSynchronousLoadingFinished?.Invoke();

            //to limit making requests for activities every time app is open
            if (activitiesFromLocalDb != null && activitiesFromLocalDb.FirstOrDefault(new TimetableDay()).CreationDate.Date == DateTimeOffset.Now.DateTime.Date) return;

            var activitiesFromApi = await LoadActivitiesApiAsync();
            OnAsynchronousLoadingFinished?.Invoke();
            if (activitiesFromApi != null) activitiesRepository.Replace(activitiesFromApi);
        }
        catch (Exception ex)
        {
            logger?.LogCatchedException(ex);
        }
    }

    List<TimetableDay>? LoadActivitiesLocalDb()
    {
        var dataFromLocalDb = activitiesRepository.GetActivities(DateTimeOffset.Now.DateTime);
        if (dataFromLocalDb is not null)
        {
            var timetableDay = dataFromLocalDb.Result.FirstOrDefault();
            Activities = timetableDay.Activities.ToObservableCollection();
            if (timetableDay.IsDayOff) ActivitiesStateKey = StateKey.Empty;
            else ActivitiesStateKey = StateKey.Loaded;

            return dataFromLocalDb.Result;
        }
        return null;
    }

    async Task<List<TimetableDay>?> LoadActivitiesApiAsync()
    {
        try
        {
            var resultFromApi = await activitiesService.GetActivitiesOfCurrentUserApiAsync(DateTimeOffset.Now.DateTime, 7);
            if (resultFromApi is null)
            {
                if (ActivitiesStateKey == StateKey.Loading)
                {
                    ActivitiesStateKey = StateKey.ConnectionError;
                }
                return null;
            }
            var firstTimetableDay = resultFromApi.Result.FirstOrDefault();
            if (Utilities.CompareCollections(Activities, firstTimetableDay.Activities, Activity.Comparer) == false)
            {
                applicationService.MainThreadInvoke(() =>
                {
                    Activities = firstTimetableDay.Activities.ToObservableCollection();
                });
            }
            if (firstTimetableDay.Activities.Count == 0) ActivitiesStateKey = StateKey.Empty;
            else ActivitiesStateKey = StateKey.Loaded;
            return resultFromApi.Result;
        }
        catch (Exception ex)
        {
            logger?.LogCatchedException(ex);
            return null;
        }
    }
}