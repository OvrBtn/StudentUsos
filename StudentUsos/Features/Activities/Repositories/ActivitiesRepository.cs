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

    async Task ScheduleNotification(string description)
    {
#if DEBUG && ANDROID
        Log.Debug("StudentUsos", $"SCHEDULING at {DateTimeOffset.Now.AddSeconds(10)} NOTIFICATION: {description.Replace('\n', ' ')}");
#endif

        LocalNotification notification = new()
        {
            ScheduledDateTime = DateTimeOffset.Now.DateTime.AddSeconds(10),
            Title = LocalizedStrings.ChangeInActivitiesSchedule,
            Description = description,
            Group = "ActivitiesSynchronization",
            Subtitle = ""
        };
        await localNotificationsService.ScheduleNotificationAsync(notification);
    }

    public async Task CompareAndScheduleNotificationsAsync(GetActivitiesResult local, GetActivitiesResult remote)
    {
        try
        {
            remote.Result = new() { remote.Result[0] };
            remote.Result[0].Activities.RemoveAt(0);

            foreach (var timetableDayLocal in local.Result)
            {
                var timetableDayRemote = remote.Result.Where(x => x.Date.Date == timetableDayLocal.Date.Date).FirstOrDefault();

                //edge case in which last synchronization was done previous day so 1 local and 1 remote timetable day won't have a match
                if (timetableDayRemote is null)
                {
                    continue;
                }

                foreach (var localActivity in timetableDayLocal.Activities)
                {
                    var remoteActivity = timetableDayRemote.Activities.FirstOrDefault(x =>
                    x.UnitId == localActivity.UnitId && x.CourseId == localActivity.CourseId);

                    if (remoteActivity is null)
                    {
                        await ScheduleNotification($"🗑️ {LocalizedStrings.ActivitiesScheduleUpdateNotification_Removed}\n{ActivityToFormattedString(localActivity)}");
                    }
                    else if (Activity.Comparer(localActivity, remoteActivity) == false)
                    {
                        await ScheduleNotification($"✏️ {LocalizedStrings.ActivitiesScheduleUpdateNotification_Updated}\n" +
                            $"{ActivityToFormattedString(localActivity)}\n↓\n{ActivityToFormattedString(remoteActivity)}");
                    }

                    if (remoteActivity is not null)
                    {
                        timetableDayRemote.Activities.Remove(remoteActivity);
                    }
                }

                foreach (var remoteActivity in timetableDayRemote.Activities)
                {
                    await ScheduleNotification($"➕ {LocalizedStrings.ActivitiesScheduleUpdateNotification_Added}\n{ActivityToFormattedString(remoteActivity)}");
                }
            }
        }
        catch (Exception ex)
        {
            logger?.LogCatchedException(ex);
        }
    }

    string ActivityToFormattedString(Activity activity)
    {
        return $"{activity.Name}\n{activity.StartDateTime.ToString()} - {activity.EndDateTime.ToString()}\n{activity.ClassTypeName}" +
            $"\n{activity.RoomNumber} - {activity.BuildingName}";
    }


}