using Android.Content;
using Android.Util;
using AndroidX.Work;
using StudentUsos.Features.Activities.Models;
using StudentUsos.Features.Activities.Repositories;
using StudentUsos.Features.Activities.Services;
using StudentUsos.Features.Authorization.Services;
using StudentUsos.Resources.LocalizedStrings;
using StudentUsos.Services.LocalNotifications;
using Activity = StudentUsos.Features.Activities.Models.Activity;

namespace StudentUsos.Platforms.Android;

public class ActivitiesSynchronizationWorker : Worker
{
    public ActivitiesSynchronizationWorker(Context context, WorkerParameters workerParams) : base(context, workerParams)
    {
    }

    IServiceProvider serviceProvider;
    IActivitiesRepository activitiesRepository;
    IActivitiesService activitiesService;
    ILocalNotificationsService localNotificationsService;
    ILogger? logger;

    public override Result DoWork()
    {
        try
        {
            serviceProvider = App.ServiceProvider;
            activitiesRepository = serviceProvider.GetService<IActivitiesRepository>()!;
            activitiesService = serviceProvider.GetService<IActivitiesService>()!;
            localNotificationsService = serviceProvider.GetService<ILocalNotificationsService>()!;
            logger = serviceProvider.GetService<ILogger>();

            //worker is created without fully loading the app, hence loading tokens has to be triggered manually
            AuthorizationService.CheckIfSignedInAndRetrieveTokens();

            bool isSuccessful = SynchronizeAndScheduleNotifications().GetAwaiter().GetResult();

            return isSuccessful ? Result.InvokeSuccess() : Result.InvokeRetry();
        }
        catch (Exception ex)
        {
            logger?.LogCatchedException(ex);
            return Result.InvokeFailure();
        }
    }

    async Task<bool> SynchronizeAndScheduleNotifications()
    {
        try
        {
            var local = activitiesRepository.GetAllActivities();
            var remote = await activitiesService.GetActivitiesOfCurrentUserApiAsync(AndroidHelper.GetCurrentDate(), 7);
            if (local is null || remote is null)
            {
                return false;
            }

            activitiesRepository.Replace(remote.Result);

            await CompareAndScheduleNotifications(local, remote);

            return true;
        }
        catch (Exception ex)
        {
            logger?.LogCatchedException(ex);
            return false;
        }

    }

    async Task ScheduleNotification(string description)
    {
#if DEBUG
        Log.Debug("StudentUsos", $"SCHEDULING at {AndroidHelper.GetCurrentDate().AddSeconds(10)} NOTIFICATION: {description.Replace('\n', ' ')}");
#endif

        LocalNotification notification = new()
        {
            ScheduledDateTime = AndroidHelper.GetCurrentDate().AddSeconds(10),
            Title = LocalizedStrings.ChangeInActivitiesSchedule,
            Description = description,
            Group = "ActivitiesSynchronization",
            Subtitle = ""
        };
        await localNotificationsService.ScheduleNotificationAsync(notification);
    }

    async Task CompareAndScheduleNotifications(GetActivitiesResult local, GetActivitiesResult remote)
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
                        await ScheduleNotification($"🗑️ {LocalizedStrings.ActivitiesScheduleUpdateNotification_Removed}\n{ActivityToString(localActivity)}");
                    }
                    else if (Activity.Comparer(localActivity, remoteActivity) == false)
                    {
                        await ScheduleNotification($"✏️ {LocalizedStrings.ActivitiesScheduleUpdateNotification_Updated}\n" +
                            $"{ActivityToString(localActivity)}\n↓\n{ActivityToString(remoteActivity)}");
                    }

                    if (remoteActivity is not null)
                    {
                        timetableDayRemote.Activities.Remove(remoteActivity);
                    }
                }

                foreach (var remoteActivity in timetableDayRemote.Activities)
                {
                    await ScheduleNotification($"➕ {LocalizedStrings.ActivitiesScheduleUpdateNotification_Added}\n{ActivityToString(remoteActivity)}");
                }
            }
        }
        catch (Exception ex)
        {
            logger?.LogCatchedException(ex);
        }
    }

    string ActivityToString(Activity activity)
    {
        return $"{activity.Name}\n{activity.StartDateTime.ToString()} - {activity.EndDateTime.ToString()}\n{activity.ClassTypeName}" +
            $"\n{activity.RoomNumber} - {activity.BuildingName}";
    }
}
