using StudentUsos.Features.Grades.Models;
using StudentUsos.Features.Groups.Models;

namespace StudentUsos.Features.Grades.Services
{
    public interface IGradesService
    {
        /// <summary>
        /// Assign grade distributions to a set of grade groups
        /// </summary>
        /// <param name="finalGradeGroups"></param>
        /// <returns></returns>
        public Task AssignGradeDistributionsAsync(IEnumerable<FinalGradeGroup> finalGradeGroups);

        public Task UpdateGradeDistributionAndRefreshChartAsync(FinalGradeGroup finalGradeGroup);

        //I could use API's "services/grades/latest" but it doesn't return course name (using "unit" field is causnig error 500)
        /// <summary>
        /// Get latest grade from USOS API
        /// </summary>
        /// <returns>FinalGrade object or null if failed</returns>
        public Task<FinalGrade?> GetLatestGradeServerAsync();

        /// <summary>
        /// Get grades from USOS API
        /// </summary>
        /// <param name="groups">Groups (subjects) which grades to get</param>
        /// <param name="academicTermId">ID of semester (e.g. 2023Z)</param>
        /// <returns>null if api request failed or exception was thrown or empty list if API returned empty JSON</returns>
        public Task<List<FinalGrade>?> GetGradesServerAsync(List<Group> groups, string academicTermId);

        /// <summary>
        /// Compare data from API with data from local database and for the modified or new Grades get GradeDistribution then save to local database
        /// </summary>
        /// <param name="groupedGradesFromApi"></param>
        public Task FetchOrCopyGradeDistributionForNewGradesFromApiAsync(IEnumerable<FinalGradeGroup> groupedGradesFromApi);
        /// <summary>
        /// Compare data from API with data from local database and for the modified or new Grades get GradeDistribution then save to local database
        /// </summary>
        /// <param name="gradesFromApi"></param>
        /// <returns></returns>
        public Task FetchOrCopyGradeDistributionForNewGradesFromApiAsync(IEnumerable<FinalGrade> gradesFromApi);
    }
}
