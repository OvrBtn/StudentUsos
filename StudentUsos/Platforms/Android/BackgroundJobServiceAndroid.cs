using Android.App;
using Android.Content;
using Android.Runtime;
using AndroidX.Work;
using StudentUsos.Platforms.Android;

namespace StudentUsos;

public class BackgroundJobServiceAndroid : IBackgroundJobService
{
    ILocalStorageManager localStorageManager;
    public BackgroundJobServiceAndroid(ILocalStorageManager localStorageManager)
    {
        this.localStorageManager = localStorageManager;
    }

    void DisableActivitiesBackgroundSynchronization()
    {
        var context = Android.App.Application.Context;
        WorkManager.GetInstance(context).CancelUniqueWork(BackgroundJobs.ActivitiesSynchronizationWorker.ToString());
    }

    void EnableActivitiesBackgroundSynchronization()
    {
        var context = Android.App.Application.Context;

        var constraints = new Constraints.Builder()
            .SetRequiredNetworkType(NetworkType.Connected!)
            .Build();

        var workRequest = PeriodicWorkRequest.Builder
            .From<ActivitiesSynchronizationWorker>(TimeSpan.FromHours(4))
            .SetConstraints(constraints)
            .Build();

        WorkManager.GetInstance(context).EnqueueUniquePeriodicWork(
            BackgroundJobs.ActivitiesSynchronizationWorker.ToString(),
            ExistingPeriodicWorkPolicy.Keep!,
            workRequest);
    }

    public void InitializeJobs()
    {
        bool isActivitiesBackgroundSynchronizationEnabled = localStorageManager.GetBool(LocalStorageKeys.ActivitiesSynchronizationBackgroundWorker_IsEnabled, true);

        if(isActivitiesBackgroundSynchronizationEnabled)
        {
            EnableActivitiesBackgroundSynchronization();
        }
    }

    public void SetActivitiesBackgroundSynchronizationEnabled(bool isEnabled)
    {
        if(isEnabled)
        {
            EnableActivitiesBackgroundSynchronization();
        }
        else
        {
            DisableActivitiesBackgroundSynchronization();
        }
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