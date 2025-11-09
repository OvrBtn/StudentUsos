using StudentUsos.Features.Activities.Models;

namespace StudentUsos.Features.Activities.Services;

public interface IActivitiesService
{
    /// <summary>
    /// Get activities from USOS API for currently signed-in user
    /// </summary>
    /// <param name="date">Get activities which start in given date (time is ignored)</param>
    /// <param name="numberOfDays">Get activities for next days, must be between 1 and 7</param>
    /// <returns><see cref="GetActivitiesResult"/> containing response status and retrieved data</returns>
    public Task<GetActivitiesResult?> GetActivitiesOfCurrentUserApiAsync(DateTime date, int numberOfDays = 1);

    /// <summary>
    /// Get activities from USOS API for any user, including staff
    /// </summary>
    /// <param name="date">Get activities which start in given date (time is ignored)</param>
    /// <param name="numberOfDays">Get activities for next days, must be between 1 and 7</param>
    /// <param name="userId">default value is "-1" meaning a timetable of currently logged in user will be displayed, other values will create page containing staff timetable using services/tt/staff method</param>
    /// <returns><see cref="GetActivitiesResult"/> containing response status and retrieved data</returns>
    public Task<GetActivitiesResult?> GetActivitiesApiAsync(DateTime date, string userId, int numberOfDays = 1);

}