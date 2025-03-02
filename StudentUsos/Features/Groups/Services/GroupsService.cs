using StudentUsos.Features.Groups.Models;
using StudentUsos.Features.Groups.Repositories;
using StudentUsos.Features.Person.Repositories;
using StudentUsos.Features.Person.Services;
using StudentUsos.Services.ServerConnection;
using System.Text.Json;

namespace StudentUsos.Features.Groups.Services
{
    public class GroupsService : IGroupsService
    {
        IGroupsRepository groupsRepository;
        ILecturerService lecturerService;
        ILecturerRepository lecturerRepository;
        IServerConnectionManager serverConnectionManager;
        ILogger? logger;
        public GroupsService(IGroupsRepository groupsRepository,
            ILecturerService lecturerService,
            ILecturerRepository lecturerRepository,
            IServerConnectionManager serverConnectionManager,
            ILogger? logger = null)
        {
            this.groupsRepository = groupsRepository;
            this.lecturerService = lecturerService;
            this.lecturerRepository = lecturerRepository;
            this.serverConnectionManager = serverConnectionManager;
            this.logger = logger;
        }

        public async Task<bool> SetEctsPointsIfNotSetAsync(IEnumerable<Group> groups)
        {
            if (groups != null && groups.Any(x => string.IsNullOrEmpty(x.EctsPoints)))
            {
                await SetEctsPointsAsync(groups);
                return true;
            }
            return false;
        }

        public async Task SetEctsPointsAsync(IEnumerable<Group> groups)
        {
            try
            {
                var arguments = new Dictionary<string, string>();
                var apiResult = await serverConnectionManager.SendRequestToUsosAsync("services/courses/user_ects_points", arguments);
                if (apiResult == null) return;
                var deserialized = JsonSerializer.Deserialize(apiResult, CourseEctsPointsJsonContext.Default.DictionaryStringDictionaryStringString);
                if (deserialized is null)
                {
                    return;
                }
                foreach (var group in groups)
                {
                    if (deserialized.ContainsKey(group.TermId) && deserialized[group.TermId].ContainsKey(group.CourseId))
                    {
                        group.EctsPoints = deserialized[group.TermId][group.CourseId];
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.LogCatchedException(ex);
            }
        }

        public async Task<GetCurrentTermGroupsServerResult?> GetCurrentTermGroupsServerAsync()
        {
            try
            {
                GetCurrentTermGroupsServerResult getCurrentTermGroupsServerResult = new();
                var arguments = new Dictionary<string, string> {
                    { "fields", "group_number|course_name|course_is_currently_conducted|course_id|lecturers|participants|class_type|course_unit_id|course_fac_id" },
                    { "active_terms", "false" } };
                var groupsServer = await serverConnectionManager.SendRequestToUsosAsync("services/groups/user", arguments);
                if (groupsServer == null)
                {
                    return null;
                }
                var deserialized = JsonSerializer.Deserialize(groupsServer, GroupsJsonRootContext.Default.GroupsJsonRoot);
                if (deserialized is null)
                {
                    return null;
                }

                string idOfCurrentTerm = "";
                foreach (var term in deserialized.Terms)
                {
                    if (term.IsCurrentlyConducted)
                    {
                        idOfCurrentTerm = term.Id;
                    }
                }
                getCurrentTermGroupsServerResult.Terms = deserialized.Terms;

                var groups = deserialized.Groups[idOfCurrentTerm];
                List<string> lecturerIds = new();
                foreach (var group in groups)
                {
                    foreach (var lecturerFromGroup in group.Lecturers)
                    {
                        if (lecturerIds.Contains(lecturerFromGroup.Id) == false)
                        {
                            lecturerIds.Add(lecturerFromGroup.Id);
                        }
                    }
                    groupsRepository.InsertOrReplace(group);
                }
                getCurrentTermGroupsServerResult.Groups = groups;

                var detailedLecturers = await lecturerService.GetDetailedLecturersAsync(lecturerIds);
                if (detailedLecturers != null)
                {
                    lecturerRepository.InsertOrReplaceAll(detailedLecturers);
                }

                return getCurrentTermGroupsServerResult;
            }
            catch (Exception ex)
            {
                logger?.LogCatchedException(ex);
                return null;
            }
        }

        public async Task<GetGroupedGroupsServerResult?> GetGroupedGroupsServerAsync(bool getOnlyActiveTerms = true, bool getParticipants = true)
        {
            try
            {
                GetGroupedGroupsServerResult getGroupedGroupsServerResult = new();
                var arguments = new Dictionary<string, string> {
                    { "fields", "group_number|course_name|course_is_currently_conducted|course_id|lecturers|" + (getParticipants ? "participants|" : "") + "class_type|course_unit_id|course_fac_id" },
                    { "active_terms", getOnlyActiveTerms ? "true" : "false" } };
                var groupsServer = await serverConnectionManager.SendRequestToUsosAsync("services/groups/user", arguments);
                if (groupsServer == null)
                {
                    return null;
                }

                var deserialized = JsonSerializer.Deserialize(groupsServer, GroupsJsonRootContext.Default.GroupsJsonRoot);
                if (deserialized is null)
                {
                    return null;
                }

                getGroupedGroupsServerResult.Terms = deserialized.Terms;

                List<GroupsGrouped> groupsGrouped = new();
                List<string> lecturerIds = new();
                foreach (var term in deserialized.Terms)
                {
                    var groups = deserialized.Groups[term.Id];
                    foreach (var group in groups)
                    {
                        foreach (var lecturerFromGroup in group.Lecturers)
                        {
                            if (lecturerIds.Contains(lecturerFromGroup.Id) == false)
                            {
                                lecturerIds.Add(lecturerFromGroup.Id);
                            }
                        }
                    }
                    getGroupedGroupsServerResult.Groups.AddRange(groups);
                    GroupsGrouped groupsGroupedObject = new(term.Id, term.Name, groups);
                    groupsGrouped.Add(groupsGroupedObject);
                }
                getGroupedGroupsServerResult.GroupsGrouped = groupsGrouped;

                var detailedLecturers = await lecturerService.GetDetailedLecturersAsync(lecturerIds);
                if (detailedLecturers != null)
                {
                    lecturerRepository.InsertOrReplaceAll(detailedLecturers);
                }

                return getGroupedGroupsServerResult;
            }
            catch (Exception ex)
            {
                logger?.LogCatchedException(ex);
                return null;
            }
        }
    }
}
