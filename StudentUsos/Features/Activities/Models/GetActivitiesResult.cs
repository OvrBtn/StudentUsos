namespace StudentUsos.Features.Activities.Models;

public class GetActivitiesResult
{
    /// <summary>
    /// Activities grouped by days
    /// </summary>
    public List<TimetableDay> Result { get; set; } = new();
    public List<Activity> AllActivities { get; private set; } = new();
    public GetActivitiesResult(List<TimetableDay> result, List<Activity> allActivities)
    {
        Result = result;
        AllActivities = allActivities;
    }
}