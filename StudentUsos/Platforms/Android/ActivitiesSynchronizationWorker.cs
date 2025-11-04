using Android.Content;
using AndroidX.Work;
using StudentUsos.Features.Activities.Repositories;
using StudentUsos.Features.Activities.Services;
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
        var serviceProvider = App.ServiceProvider;
        var activitiesRepository = serviceProvider.GetService<IActivitiesRepository>()!;
        var activitiesService = serviceProvider.GetService<IActivitiesService>()!;

        var local = activitiesRepository.GetAllActivities();
        var remote = await activitiesService.GetActivitiesOfCurrentUserApiAsync(AndroidHelper.GetCurrentDate(), 7);
        if (local is null || remote is null)
        {
            return;
        }

        activitiesRepository.Replace(remote.Result);

        var localNoticationsService = serviceProvider.GetService<ILocalNotificationsService>()!;

        foreach (var timetableDayLocal in local.Result)
        {
            var timetableDayRemote = remote.Result.Where(x => x.Date.Date == timetableDayLocal.Date.Date).FirstOrDefault();
            //edge case in which last synchronization was done previous day so 1 local and 1 remote timetable day won't have a match
            if (timetableDayRemote is null)
            {
                continue;
            }

        }
    }
}
