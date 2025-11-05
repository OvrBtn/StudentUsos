using Android.Content;
using AndroidX.Work;
using StudentUsos.Features.Activities.Models;
using StudentUsos.Features.Activities.Repositories;
using StudentUsos.Features.Activities.Services;
using StudentUsos.Resources.LocalizedStrings;
using StudentUsos.Services.LocalNotifications;

namespace StudentUsos.Platforms.Android;

public class ActivitiesSynchronizationWorker : Worker
{
    public ActivitiesSynchronizationWorker(Context context, WorkerParameters workerParams) : base(context, workerParams)
    {
    }

    public override Result DoWork()
    {
        try
        {
            SynchronizeAndScheduleNotifications().Wait();

            return Result.InvokeSuccess();
        }
        catch (Exception ex)
        {
            return Result.InvokeFailure();
        }
    }

    async Task SynchronizeAndScheduleNotifications()
    {
        try
        {

            var serviceProvider = App.ServiceProvider;
            var activitiesRepository = serviceProvider.GetService<IActivitiesRepository>()!;
            var activitiesService = serviceProvider.GetService<IActivitiesService>()!;

            //var t = serviceProvider.GetService<ILocalNotificationsService>()!;
            //await t.ScheduleNotificationAsync(new()
            //{
            //    Description = "tesdt",
            //    Group = "test",
            //    ScheduledDateTime = DateTime.Now.AddSeconds(10),
            //    Subtitle = "test",
            //    Title = "test"
            //});

            var local = activitiesRepository.GetAllActivities();
            var remote = await activitiesService.GetActivitiesOfCurrentUserApiAsync(AndroidHelper.GetCurrentDate(), 7);
            if (local is null || remote is null)
            {
                return;
            }

            activitiesRepository.Replace(remote.Result);

            localNotificationsService = serviceProvider.GetService<ILocalNotificationsService>()!;
            await CompareAndScheduleNotifications(local, remote);
        }
        catch (Exception e)
        {
            var t = 5;
        }

    }

    ILocalNotificationsService localNotificationsService;

    async Task ScheduleNotification(string description)
    {
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
            remote.Result[1].Activities.Add(remote.Result[2].Activities[0]);

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
        catch (Exception e)
        {
            var t = 5;
        }
    }

    string ActivityToString(Activity activity)
    {
        return $"{activity.Name}\n{activity.StartDateTime.ToString()} - {activity.EndDateTime.ToString()}\n{activity.ClassTypeName}" +
            $"\n{activity.RoomNumber} - {activity.BuildingName}";
    }
}
