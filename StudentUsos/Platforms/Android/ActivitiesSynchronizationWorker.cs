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
    ILocalStorageManager localStorageManager;
    ILogger? logger;

    public override Result DoWork()
    {
        try
        {
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
        int runsCount = 0;
        if (localStorageManager.TryGettingData(LocalStorageKeys.ActivitiesSynchronizationBackgroundWorker_AmountOfRuns, out var runsCountString))
        {
            int.TryParse(runsCountString, out runsCount);
        }
        runsCount = (runsCount + 1) % (int.MaxValue - 1);
        localStorageManager.SetData(LocalStorageKeys.ActivitiesSynchronizationBackgroundWorker_AmountOfRuns, runsCount.ToString());
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

            await activitiesRepository.CompareAndScheduleNotificationsAsync(local, remote);

            return true;
        }
        catch (Exception ex)
        {
            logger?.LogCatchedException(ex);
            return false;
        }
    }
    
}
