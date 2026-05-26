using StudentUsos.Features.AcademicTerms.Repositories;
using StudentUsos.Features.Activities.Models;
using StudentUsos.Features.Groups.Repositories;
using StudentUsos.Services.ServerConnection;
using System.Text.Json;

namespace StudentUsos.Features.Activities.Services;

public class ActivitiesService : IActivitiesService
{
    IServerConnectionManager serverConnectionManager;
    IGroupsRepository groupsRepository;
    ITermsRepository termsRepository;
    IApplicationService applicationService;
    ILogger? logger;
    public ActivitiesService(IServerConnectionManager serverConnectionManager,
        IGroupsRepository groupsRepository,
        ITermsRepository termsRepository,
        IApplicationService applicationService,
        ILogger? logger)
    {
        this.serverConnectionManager = serverConnectionManager;
        this.groupsRepository = groupsRepository;
        this.termsRepository = termsRepository;
        this.applicationService = applicationService;
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

    public async Task<List<ActivityOccurence>?> GetAllOccurencesOfAllActivitiesAsync()
    {
        try
        {
            if (termsRepository.TryGettingCurrentTerm(out var currentTerm) == false || currentTerm is null)
            {
                return null;
            }

            var groups = groupsRepository.GetGroups(currentTerm);
            if (groups is null || groups.Count == 0)
            {
                return null;
            }

            object lockObject = new();
            List<ActivityOccurence> occurences = new();
            List<Task> tasks = new();

            string method = "services/tt/classgroup_dates";

            foreach (var group in groups)
            {
                tasks.Add(applicationService.WorkerThreadInvoke(async () =>
                {
                    var arguments = new Dictionary<string, string> {
                        { "unit_id", group.CourseUnitId },
                        { "group_number", group.GroupNumber } };

                    var occurencesServer = await serverConnectionManager.SendRequestToUsosAsync(method, arguments);
                    if (occurencesServer == null)
                    {
                        return;
                    }

                    var deserialized = JsonSerializer.Deserialize(occurencesServer, JsonContext.Default.ListString);
                    if (deserialized == null)
                    {
                        return;
                    }

                    List<ActivityOccurence> localOccurences = new();
                    foreach (string item in deserialized)
                    {
                        localOccurences.Add(new()
                        {
                            CourseUnitId = group.CourseUnitId,
                            GroupNumber = group.GroupNumber,
                            ClassType = group.ClassTypeEnum,
                            Occurence = item
                        });
                    }

                    lock (lockObject)
                    {
                        occurences.AddRange(localOccurences);
                    }
                }));
            }

            await Task.WhenAll(tasks);

            return occurences;

        }
        catch (Exception ex)
        {
            logger?.LogCatchedException(ex);
            return null;
        }
    }
}