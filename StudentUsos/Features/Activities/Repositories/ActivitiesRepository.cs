using StudentUsos.Features.Activities.Models;

namespace StudentUsos.Features.Activities.Repositories;

public class ActivitiesRepository : IActivitiesRepository
{
    ILocalDatabaseManager localDatabaseManager;
    ILogger? logger;
    public ActivitiesRepository(ILocalDatabaseManager localDatabaseManager, ILogger? logger = null)
    {
        this.localDatabaseManager = localDatabaseManager;
        this.logger = logger;
    }

    public Task CompareAndScheduleNotifications(List<Activity> oldActivities, List<Activity> newActivities)
    {
        return Task.CompletedTask;
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


}