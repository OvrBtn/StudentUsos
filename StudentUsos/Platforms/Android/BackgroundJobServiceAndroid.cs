using Android.App;
using Android.Content;
using Android.Runtime;
using AndroidX.Work;
using StudentUsos.Platforms.Android;

namespace StudentUsos;

public class BackgroundJobServiceAndroid : IBackgroundJobService
{
    public BackgroundJobServiceAndroid()
    {

    }

    public void InitializeJobs()
    {
        InitializeActivitiesSynchronizationJob();
    }

    void InitializeActivitiesSynchronizationJob()
    {

        var context = Android.App.Application.Context;

        var constraints = new Constraints.Builder()
            .SetRequiredNetworkType(NetworkType.Connected!)
            .Build();

        var workRequest = PeriodicWorkRequest.Builder
            .From<ActivitiesSynchronizationWorker>(TimeSpan.FromMinutes(15))
            .SetConstraints(constraints)
            .Build();

        WorkManager.GetInstance(context).EnqueueUniquePeriodicWork(
            "ActivitiesSynchronizationWorker",
            ExistingPeriodicWorkPolicy.Keep!,
            workRequest);
    }
}

#if DEBUG
[BroadcastReceiver(Enabled = true, Exported = true)]
[IntentFilter(new[] { "com.studentusos.ACTION_SYNC_ACTIVITIES" })]
[Register("com.studentusos.ActivitiesSyncReceiver")]
public class ActivitiesSyncReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (intent is null)
        {
            return;
        }

        var request = OneTimeWorkRequest.Builder
            .From<ActivitiesSynchronizationWorker>()
            .Build();

        WorkManager.GetInstance(context)
            .Enqueue(request);
    }
}
#endif