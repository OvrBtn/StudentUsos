using AndroidX.Work;
using StudentUsos.Features.Activities.Services;
using StudentUsos.Platforms.Android;

namespace StudentUsos;

public class BackgroundJobServiceAndroid : IBackgroundJobService
{
    IActivitiesService activitiesService;
    public BackgroundJobServiceAndroid(IActivitiesService activitiesService)
    {
        this.activitiesService = activitiesService;
    }

    public void InitializeJobs()
    {
        InitializeActivitiesSynchronizationJob();
    }

    void InitializeActivitiesSynchronizationJob()
    {
        var context = Android.App.Application.Context;

        var workRequest = PeriodicWorkRequest.Builder
            .From<ActivitiesSynchronizationWorker>(TimeSpan.FromMinutes(15))
            .Build();

        WorkManager.GetInstance(context).EnqueueUniquePeriodicWork(
            "ActivitiesSynchronizationWorker",
            ExistingPeriodicWorkPolicy.Keep!,
            workRequest);
    }
}
