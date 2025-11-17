using Android.Content;
using AndroidX.Work;
using StudentUsos.Features.Activities.Repositories;
using StudentUsos.Features.Activities.Services;
using StudentUsos.Features.Authorization.Services;

namespace StudentUsos.Platforms.Android;

public class ActivitiesSynchronizationWorker : Worker
{
    public ActivitiesSynchronizationWorker(Context context, WorkerParameters workerParams) : base(context, workerParams)
    {
    }

    IServiceProvider serviceProvider;
    IActivitiesRepository activitiesRepository;
    IActivitiesService activitiesService;
    ILocalStorageManager localStorageManager;
    ILogger? logger;

    public override Result DoWork()
    {
        try
        {
            //there is a chance that some issues occur due to multiple workers running in the same time (I wasn't able to reproduce
            //so I'm doing a bit of guessing) hence a bit of randomness and it's possible that worker is trying to run code
            //dependent on the rest of the app before it has a chance to fully initialize
            Random random = new();
            int delay = random.Next(7000, 8000);
            Task.Delay(delay).Wait();

            MauiProgram.BuildingTask.Wait();

            serviceProvider = App.ServiceProvider;
            activitiesRepository = serviceProvider.GetService<IActivitiesRepository>()!;
            activitiesService = serviceProvider.GetService<IActivitiesService>()!;
            localStorageManager = serviceProvider.GetService<ILocalStorageManager>()!;
            logger = serviceProvider.GetService<ILogger>();

            IncreaseRunsCount();

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

    void IncreaseRunsCount()
    {
        int runsCount = localStorageManager.GetInt(LocalStorageKeys.ActivitiesSynchronizationBackgroundWorker_AmountOfRuns, 0);
        runsCount = (runsCount + 1) % int.MaxValue;
        localStorageManager.SetInt(LocalStorageKeys.ActivitiesSynchronizationBackgroundWorker_AmountOfRuns, runsCount);
    }

    async Task<bool> SynchronizeAndScheduleNotifications()
    {
        try
        {
            await LocalDatabaseManager.WaitUntilInitializedAsync();

            var local = activitiesRepository.GetAllActivities();
            var remote = await activitiesService.GetActivitiesOfCurrentUserApiAsync(AndroidHelper.GetCurrentDate(), 7);
            if (local is null || remote is null)
            {
                return false;
            }

            activitiesRepository.Replace(remote.Result);

            bool areNotificationsEnabled = localStorageManager.GetBool(LocalStorageKeys.ActivitiesSynchronizationBackgroundWorker_ShouldSendNotifications, true);

            if (areNotificationsEnabled)
            {
                await activitiesRepository.CompareAndScheduleNotificationsAsync(local, remote);
            }

            return true;
        }
        catch (Exception ex)
        {
            logger?.LogCatchedException(ex);
            return false;
        }
    }

}
