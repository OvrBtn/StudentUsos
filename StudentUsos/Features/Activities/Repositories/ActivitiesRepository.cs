using StudentUsos.Features.Activities.Models;

namespace StudentUsos.Features.Activities.Repositories
{
    public class ActivitiesRepository : IActivitiesRepository
    {
        ILocalDatabaseManager localDatabaseManager;
        public ActivitiesRepository(ILocalDatabaseManager localDatabaseManager)
        {
            this.localDatabaseManager = localDatabaseManager;
        }

        public GetActivitiesResult? GetActivities(DateTime date)
        {
            try
            {
                var activities = localDatabaseManager.GetAll<Activity>(x => x.StartDateTime.Date == date.Date);
                if (activities.Count == 0)
                {
                    return null;
                }

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
                Utilities.ShowError(ex);
                return null;
            }
        }

        public GetActivitiesResult? GetAllActivities()
        {
            try
            {
                var activities = localDatabaseManager.GetAll<Activity>();
                if (activities.Count == 0)
                {
                    return null;
                }

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
                Utilities.ShowError(ex);
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
}
