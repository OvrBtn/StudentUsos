#if ANDROID
using Android.Util;
#endif
using StudentUsos.Features.Activities.Models;
using StudentUsos.Resources.LocalizedStrings;
using StudentUsos.Services.LocalNotifications;

namespace StudentUsos.Features.Activities.Repositories;

public class ActivitiesRepository : IActivitiesRepository
{
    ILocalDatabaseManager localDatabaseManager;
    ILocalNotificationsService localNotificationsService;
    ILogger? logger;
    public ActivitiesRepository(ILocalDatabaseManager localDatabaseManager,
        ILocalNotificationsService localNotificationsService,
        ILogger? logger = null)
    {
        this.localDatabaseManager = localDatabaseManager;
        this.localNotificationsService = localNotificationsService;
        this.logger = logger;
    }

    public GetActivitiesResult? GetActivities(DateTime date)
    {
        try
        {
            var all = localDatabaseManager.GetAll<Activity>();
            var activities = localDatabaseManager.GetAll<Activity>(x => x.StartDateTime.Date == date.Date);

            var timetableDays = localDatabaseManager.GetAll<TimetableDay>(x => x.Date.Date == date.Date);
            if (timetableDays.Count == 0)
            {
                return null;
            }
            ActivitiesHelpers.GroupActivities(timetableDays, activities);
            return new GetActivitiesResult(timetableDays, activities);
        }
        catch (Exception ex)
        {
            logger?.LogCatchedException(ex);
            return null;
        }
    }

    public GetActivitiesResult? GetAllActivities()
    {
        try
        {
            var activities = localDatabaseManager.GetAll<Activity>();

            var timetableDays = localDatabaseManager.GetAll<TimetableDay>();
            if (timetableDays.Count == 0)
            {
                return null;
            }
            ActivitiesHelpers.GroupActivities(timetableDays, activities);
            return new GetActivitiesResult(timetableDays, activities);
        }
        catch (Exception ex)
        {
            logger?.LogCatchedException(ex);
            return null;
        }
    }

    public void Replace(IEnumerable<TimetableDay> timetableDays)
    {
        localDatabaseManager.ClearTable<TimetableDay>();
        localDatabaseManager.ClearTable<Activity>();
        foreach (var timetableDay in timetableDays)
        {
            localDatabaseManager.InsertOrReplace(timetableDay);
            localDatabaseManager.InsertOrReplaceAll(timetableDay.Activities);
        }
    }

    Random random = new();

    async Task ScheduleNotification(string description)
    {
        //when scheduling multiple notifications and the exact same time sometimes some of them are not shown
        //using random delay seems to fix this
        int delaySeconds = random.Next(5, 50);
#if DEBUG && ANDROID
        Log.Debug("StudentUsos", $"SCHEDULING at {DateTimeOffset.Now.AddSeconds(delaySeconds)} NOTIFICATION: {description.Replace('\n', ' ')}");
#endif

        LocalNotification notification = new()
        {
            ScheduledDateTime = DateTimeOffset.Now.DateTime.AddSeconds(delaySeconds),
            Title = LocalizedStrings.ChangeInActivitiesSchedule,
            Description = description,
            Group = "ActivitiesSynchronization",
            Subtitle = ""
        };
        await localNotificationsService.ScheduleNotificationAsync(notification);
    }

    async Task ScheduleAddedNotification(Activity activity)
    {
        await ScheduleNotification($"➕ {LocalizedStrings.ActivitiesScheduleUpdateNotification_Added}\n{ActivityToFormattedString(activity)}");
    }

    async Task ScheduleRemovedNotification(Activity activity)
    {
        await ScheduleNotification($"🗑️ {LocalizedStrings.ActivitiesScheduleUpdateNotification_Removed}\n{ActivityToFormattedString(activity)}");
    }

    async Task ScheduleChangedNotification(Activity localActivity, Activity remoteActivity)
    {
        await ScheduleNotification($"✏️ {LocalizedStrings.ActivitiesScheduleUpdateNotification_Updated}\n" +
                $"{ActivityToFormattedString(localActivity)}\n↓\n{ActivityToFormattedString(remoteActivity)}");
    }

    public async Task CompareAndScheduleNotificationsAsync(GetActivitiesResult local, GetActivitiesResult remote)
    {
        await CompareAndScheduleNotificationsAsync(local.Result, remote.Result);
    }

    public async Task CompareAndScheduleNotificationsAsync(List<TimetableDay> local, List<TimetableDay> remote)
    {
        try
        {
            foreach (var timetableDayLocal in local)
            {
                var timetableDayRemote = remote.Where(x => x.Date.Date == timetableDayLocal.Date.Date).FirstOrDefault();

                //edge case in which last synchronization was done previous day so 1 local and 1 remote timetable day won't have a match
                if (timetableDayRemote is null)
                {
                    continue;
                }

                //copy to avoid modifying the original collection
                var localActivitiesCopy = new List<Activity>(timetableDayLocal.Activities);
                var remoteActivitiesCopy = new List<Activity>(timetableDayRemote.Activities);

                while (localActivitiesCopy.Count > 0)
                {
                    //this could have been simple if USOS had unique ids for activities
                    //but when two (or more) activities for the same subject are scheduled during one day
                    //they share the same UnitId and CourseId making it impossible to compare them and detect changes
                    //hence the separation of unique and non-unique activities

                    var localActivity = localActivitiesCopy[0];
                    var localActivitiesEqualUnitAndCourse = localActivitiesCopy.Where(x => x.UnitId == localActivity.UnitId &&
                    x.CourseId == localActivity.CourseId).ToList();

                    var remoteActivitiesEqualUnitAndCourse = remoteActivitiesCopy.Where(x => x.UnitId == localActivity.UnitId &&
                    x.CourseId == localActivity.CourseId).ToList();
                    var remoteActivity = remoteActivitiesEqualUnitAndCourse.FirstOrDefault();

                    remoteActivity = null;

                    if (localActivitiesEqualUnitAndCourse.Count <= 1 && remoteActivitiesEqualUnitAndCourse.Count <= 1)
                    {
                        await HandleUniqueActivitiesAsync(localActivitiesCopy, remoteActivitiesCopy, localActivity, remoteActivity);
                    }
                    else
                    {
                        await HandleDuplicateActivitiesAsync(localActivitiesCopy, remoteActivitiesCopy, localActivitiesEqualUnitAndCourse, remoteActivitiesEqualUnitAndCourse);
                    }

                }

                foreach (var remoteActivity in remoteActivitiesCopy)
                {
                    await ScheduleAddedNotification(remoteActivity);
                }
            }
        }
        catch (Exception ex)
        {
            logger?.LogCatchedException(ex);
        }
    }

    async Task HandleUniqueActivitiesAsync(List<Activity> localActivitiesCopy, List<Activity> remoteActivitiesCopy, Activity localActivity, Activity? remoteActivity)
    {
        if (remoteActivity is null)
        {
            await ScheduleRemovedNotification(localActivity);
        }
        else if (Activity.Comparer(localActivity, remoteActivity) == false)
        {
            await ScheduleChangedNotification(localActivity, remoteActivity);
        }

        localActivitiesCopy.Remove(localActivity);
        if (remoteActivity is not null)
        {
            remoteActivitiesCopy.Remove(remoteActivity);
        }
    }

    async Task HandleDuplicateActivitiesAsync(List<Activity> localActivitiesCopy, List<Activity> remoteActivitiesCopy, List<Activity> localFound, List<Activity> remoteFound)
    {
        //in this case trying to send activities about changes would be too messy and could lead to false notifications
        //hence it's limited to notifying about added and removed activities

        //remove unchanged activities from both lists
        for (int i = 0; i < localFound.Count; i++)
        {
            var remote = remoteFound.Where(x => Activity.Comparer(localFound[i], x)).ToList();
            if (remote.Count == 0)
            {
                continue;
            }
            localActivitiesCopy.Remove(localFound[i]);
            localFound.RemoveAt(i);
            i--;
            foreach (var item in remote)
            {
                remoteActivitiesCopy.Remove(item);
                remoteFound.Remove(item);
            }
        }

        //treat all remaining local activities as removed
        //and all remaining remote activities as added
        foreach (var localActivity in localFound)
        {
            await ScheduleRemovedNotification(localActivity);
            localActivitiesCopy.Remove(localActivity);
        }
        foreach (var remoteActivity in remoteFound)
        {
            await ScheduleAddedNotification(remoteActivity);
            remoteActivitiesCopy.Remove(remoteActivity);
        }
    }

    string ActivityToFormattedString(Activity activity)
    {
        return $"{activity.Name}\n{activity.StartDateTime.ToString("g")} - {activity.EndDateTime.ToString("g")}\n{activity.ClassTypeName}" +
            $"\n{activity.RoomNumber} - {activity.BuildingName}";
    }


}