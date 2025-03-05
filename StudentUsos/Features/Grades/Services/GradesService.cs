using StudentUsos.Features.AcademicTerms.Repositories;
using StudentUsos.Features.AcademicTerms.Services;
using StudentUsos.Features.Grades.Helpers;
using StudentUsos.Features.Grades.Models;
using StudentUsos.Features.Grades.Repositories;
using StudentUsos.Features.Groups.Models;
using StudentUsos.Features.Groups.Repositories;
using StudentUsos.Features.Groups.Services;
using StudentUsos.Services.ServerConnection;
using System.Text.Json;

namespace StudentUsos.Features.Grades.Services;

public class GradesService : IGradesService
{
    IServerConnectionManager serverConnectionManager;
    IGradesRepository gradesRepository;
    IGroupsRepository groupsRepository;
    IGroupsService groupsService;
    ITermsRepository termsRepository;
    ITermsService termsService;
    ILogger? logger;
    public GradesService(IServerConnectionManager serverConnectionManager,
        IGradesRepository gradesRepository,
        IGroupsRepository groupsRepository,
        IGroupsService groupsService,
        ITermsRepository termsRepository,
        ITermsService termsService,
        ILogger? logger = null)
    {
        this.serverConnectionManager = serverConnectionManager;
        this.gradesRepository = gradesRepository;
        this.groupsRepository = groupsRepository;
        this.groupsService = groupsService;
        this.termsRepository = termsRepository;
        this.termsService = termsService;
        this.logger = logger;
    }

    //in seconds
    readonly int updatingFromApiCooldown = 10;
    public async Task UpdateGradeDistributionAndRefreshChartAsync(FinalGradeGroup finalGradeGroup)
    {
        if (DateTimeOffset.Now.DateTime - finalGradeGroup.lastChartUpdate < TimeSpan.FromSeconds(updatingFromApiCooldown)) return;
        string gradeDistributionBefore = finalGradeGroup.FirstTermGrade.GradeDistribution;
        await AssignGradeDistributionsAsync(new[] { finalGradeGroup });
        if (gradeDistributionBefore != finalGradeGroup.FirstTermGrade.GradeDistribution)
        {
            if (finalGradeGroup.SaveUpdatedGradeDistributionToLocalDatabse && finalGradeGroup.IsAnyTermModified == false)
            {
                var fromLocalDb = gradesRepository.Get(finalGradeGroup.FirstTermGrade.CourseUnitId);
                if (fromLocalDb != null)
                {
                    finalGradeGroup.FirstTermGrade.Id = fromLocalDb.Id;
                    gradesRepository.InsertOrReplace(finalGradeGroup.FirstTermGrade);
                }
            }
            finalGradeGroup.BuildGradeDistributionChart();
        }
        finalGradeGroup.lastChartUpdate = DateTimeOffset.Now.DateTime;
    }

    public async Task AssignGradeDistributionsAsync(IEnumerable<FinalGradeGroup> finalGradeGroups)
    {
        try
        {
            var finalGradeGroupsList = finalGradeGroups.ToList();
            List<string> terms = new();
            foreach (var item in finalGradeGroupsList)
            {
                string termId = item.FirstTermGrade.Group.TermId;
                if (terms.Contains(termId) == false)
                {
                    terms.Add(termId);
                }
            }

            Dictionary<string, string> args = new()
            {
                {"fields", "grades_distribution|course_unit" },
                {"term_ids", string.Join("|", terms) }
            };
            var result = await serverConnectionManager.SendRequestToUsosAsync("services/examrep/user2", args);
            if (result is null)
            {
                return;
            }
            var deserialized = JsonSerializer.Deserialize(result, GradeDistributionJsonRootJsonContext.Default.DictionaryStringDictionaryStringListGradeDistributionJsonRoot);
            if (deserialized is null)
            {
                return;
            }

            foreach (var item in finalGradeGroupsList)
            {
                if (deserialized.TryGetValue(item.FirstTermGrade.Group.TermId, out var distributionsInTerm) &&
                    distributionsInTerm.TryGetValue(item.FirstTermGrade.CourseId, out var distributions))
                {
                    var found = distributions.FirstOrDefault(x => x.CourseUnit.CourseUnitId == item.FirstTermGrade.CourseUnitId);
                    if (found != null)
                    {
                        item.FirstTermGrade.GradeDistribution = found.GradeDistribution;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger?.LogCatchedException(ex);
        }
    }

    public async Task<FinalGrade?> GetLatestGradeServerAsync()
    {
        try
        {
            if (termsRepository.TryGettingCurrentTerm(out var currentTerm) == false)
            {
                currentTerm = await termsService.GetCurrentTermAsync();
            }
            if (currentTerm is null) return null;
            var groups = groupsRepository.GetGroups(currentTerm);
            if (groups is null || groups.Count == 0)
            {
                var groupsFromApi = await groupsService.GetCurrentTermGroupsServerAsync();
                if (groupsFromApi is null) return null;
                groups = groupsFromApi.Groups;
                groupsRepository.InsertOrReplace(groupsFromApi.Terms);
                groupsRepository.InsertOrReplace(groupsFromApi.Groups);
            }
            if (groups is null) return null;
            var grades = await GetGradesServerAsync(groups, currentTerm.Id);
            if (grades is null) return null;
            var latest = GradesHelper.FindLatest(grades);
            if (latest is null) return null;
            int index = grades.IndexOf(latest);
            if (index == -1) return null;
            grades[index].IsLatest = true;

            await FetchOrCopyGradeDistributionForNewGradesFromApiAsync(grades);
            gradesRepository.DeleteAll();
            gradesRepository.InsertAll(grades);

            return latest;
        }
        catch (Exception ex) { logger?.LogCatchedException(ex); return null; }
    }

    public async Task<List<FinalGrade>?> GetGradesServerAsync(List<Group> groups, string academicTermId)
    {
        try
        {
            List<FinalGrade> grades = new();
            List<FinalGradeGroup> gradesGrouped = new();

            //API request
            var arguments = new Dictionary<string, string> { { "term_ids", academicTermId },
                { "fields", "value_symbol|passes|value_description|exam_id|exam_session_number|counts_into_average|comment|grade_type_id|date_modified|date_acquisition|modification_author" }};
            var result = await serverConnectionManager.SendRequestToUsosAsync("services/grades/terms2", arguments);
            if (result == null) return null;
            var deserialized = JsonSerializer.Deserialize(result, FinalGradeJsonContext.Default.DictionaryStringDictionaryStringCourseIdJsonObject);
            if (deserialized is null)
            {
                return null;
            }
            if (deserialized[academicTermId] == null)
            {
                return null;
            }

            var groupsJson = deserialized[academicTermId];

            // Handles edge case where at the start of every semester USOS will return only this:
            // "{\"2024Z\": {}}"
            if (groupsJson.Count == 0)
            {
                return new();
            }

            foreach (var group in groups)
            {
                //TODO: compiled thinks it will never return null but I think it did (see comment below)
                CourseIdJsonObject? course = groupsJson[group.CourseId];
                if (course is null)
                {
                    return new List<FinalGrade>(); // for some reason at the start of new term USOS is returning just "{{ "2023L": {} }}"
                }
                var courseUnits = course.CourseUnitsGrades;
                var courseUnit = courseUnits[group.CourseUnitId];
                foreach (var term in courseUnit)
                {
                    term.TryGetValue("1", out FinalGrade? finalGradeFirstTerm);
                    if (finalGradeFirstTerm is null)
                    {
                        finalGradeFirstTerm = new();
                    }
                    finalGradeFirstTerm.AssignGroup(group);
                    term.TryGetValue("2", out FinalGrade? finalGradeSecondTerm);
                    if (finalGradeSecondTerm is null)
                    {
                        finalGradeSecondTerm = new();
                    }
                    finalGradeSecondTerm.AssignGroup(group);
                    grades.Add(finalGradeFirstTerm);
                    grades.Add(finalGradeSecondTerm);
                }
            }
            return grades;
        }
        catch (Exception ex)
        {
            logger?.LogCatchedException(ex);
            return null;
        }
    }

    public async Task FetchOrCopyGradeDistributionForNewGradesFromApiAsync(IEnumerable<FinalGrade> gradesFromApi)
    {
        var grouped = GradesHelper.GroupGrades(gradesFromApi.ToList());
        await FetchOrCopyGradeDistributionForNewGradesFromApiAsync(grouped);
    }

    public async Task FetchOrCopyGradeDistributionForNewGradesFromApiAsync(IEnumerable<FinalGradeGroup> groupedGradesFromApi)
    {
        try
        {
            var groupedGradesFromApiList = groupedGradesFromApi.ToList();
            var gradesLocalGrouped = GradesHelper.GroupGrades(gradesRepository.GetAll());

            //find FinalGrades which are new/modified and which GradeDistribution should be refreshed
            Utilities.ListsDifference(gradesLocalGrouped, groupedGradesFromApiList, out List<FinalGradeGroup> localSubtractApi,
                out List<FinalGradeGroup> apiSubtractLocal, FinalGradeGroup.AreCourseExamAndGradeEqual);

            //get GradeDistribution for modified or new FinalGrades
            if (apiSubtractLocal.Count > 0)
            {
                await AssignGradeDistributionsAsync(apiSubtractLocal);
            }

            //because we are requesting GradeDistribution only for new/modified grades and grades returned from USOS API don't contain GradeDistribution
            //we should copy previously obtained GradeDistribution from grades saved in local db
            GradesHelper.CopyGradeDistributionsIfNotSet(gradesLocalGrouped, groupedGradesFromApiList);
        }
        catch (Exception ex)
        {
            logger?.LogCatchedException(ex);
        }
    }
}