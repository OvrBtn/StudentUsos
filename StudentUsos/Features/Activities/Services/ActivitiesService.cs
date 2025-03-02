using StudentUsos.Features.Activities.Models;
using StudentUsos.Services.ServerConnection;
using System.Text.Json;

namespace StudentUsos.Features.Activities.Services
{
    public class ActivitiesService : IActivitiesService
    {
        IServerConnectionManager serverConnectionManager;
        ILogger? logger;
        public ActivitiesService(IServerConnectionManager serverConnectionManager, ILogger? logger)
        {
            this.serverConnectionManager = serverConnectionManager;
            this.logger = logger;
        }

        public Task<GetActivitiesResult?> GetActivitiesOfCurrentUserApiAsync(DateTime date, int numberOfDays = 1)
        {
            return GetActivitiesAsync(date, numberOfDays);
        }

        public Task<GetActivitiesResult?> GetActivitiesApiAsync(DateTime date, string userId, int numberOfDays = 1)
        {
            return GetActivitiesAsync(date, numberOfDays, userId);
        }

        async Task<GetActivitiesResult?> GetActivitiesAsync(DateTime date, int numberOfDays = 1, string userId = "-1")
        {
            try
            {
                if (numberOfDays <= 0 || numberOfDays > 7) throw new Exception("numberOfDays must be between 1 and 7");

                string method = "services/tt/student";
                var arguments = new Dictionary<string, string> {
                    { "start", date.ToString("yyyy-MM-dd") },
                    { "days", numberOfDays.ToString() },
                    { "fields", "start_time|end_time|name|building_name|room_number|lecturer_ids|course_id|classtype_name|unit_id|group_number" } };
                if (userId != "-1")
                {
                    arguments.Add("user_id", userId);
                    method = "services/tt/staff";
                }
                var activitiesServer = await serverConnectionManager.SendRequestToUsosAsync(method, arguments);
                if (activitiesServer == null)
                {
                    return null;
                }
                var deserialized = JsonSerializer.Deserialize(activitiesServer, ActivityJsonContext.Default.ListActivity);
                if (deserialized == null)
                {
                    return null;
                }

                var timetableDays = ActivitiesHelpers.GenerateEmptyTimetableDays(date, numberOfDays);
                foreach (var item in deserialized)
                {
                    if (userId != "-1") item.Type = Activity.ActivityType.Staff;
                }
                ActivitiesHelpers.GroupActivities(timetableDays, deserialized);

                return new GetActivitiesResult(timetableDays, deserialized);

            }
            catch (Exception ex)
            {
                logger?.LogCatchedException(ex);
                return null;
            }
        }
    }
}
