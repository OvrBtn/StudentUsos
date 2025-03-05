using StudentUsos.Features.Activities.Models;

namespace StudentUsos.Features.Activities;

internal class ActivitiesHelpers
{
    /// <summary>
    /// Group activities by their start date
    /// </summary>
    /// <param name="timetableDays"></param>
    /// <param name="activities"></param>
    internal static void GroupActivities(IEnumerable<TimetableDay> timetableDays, IEnumerable<Activity> activities)
    {
        try
        {
            foreach (var activity in activities)
            {
                var foundDay = timetableDays.Where(x => x.Date.Date == activity.StartDateTime.Date).ToList();
                if (foundDay.Count == 0) continue;
                foundDay[0].Activities.Add(activity);
            }
        }
        catch (Exception ex)
        {
            Logger.Default?.LogCatchedException(ex);
        }
    }

    internal static List<TimetableDay> GenerateEmptyTimetableDays(DateTime startDate, int amountOfDays)
    {
        List<TimetableDay> timetableDays = new();
        for (int i = 0; i < amountOfDays; i++)
        {
            timetableDays.Add(new TimetableDay(startDate));
            startDate = startDate.AddDays(1);
        }
        return timetableDays;
    }
}

