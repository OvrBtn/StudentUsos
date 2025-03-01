using StudentUsos.Features.Activities.Models;

namespace StudentUsos.Features.Activities.Repositories
{
    public interface IActivitiesRepository
    {
        /// <summary>
        /// Get activities saved in local database
        /// </summary>
        /// <param name="date">Get activities which start in given date (time is ignored)</param>
        /// <returns><see cref="GetActivitiesResult"/> containing response status and retrieved data</returns>
        public GetActivitiesResult? GetActivities(DateTime date);
        public GetActivitiesResult? GetAllActivities();
        public void Replace(IEnumerable<TimetableDay> timetableDays);
    }
}
