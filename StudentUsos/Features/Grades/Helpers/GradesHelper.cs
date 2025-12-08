using StudentUsos.Features.Grades.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace StudentUsos.Features.Grades.Helpers;

public static class GradesHelper
{
    public static FinalGrade? FindLatest(List<FinalGrade> grades)
    {
        try
        {
            if (grades.Count == 0) return null;
            var gradesToSort = new List<FinalGrade>(grades);
            gradesToSort = gradesToSort.OrderBy(x => x.DateModifiedDateTime).ToList();
            if (string.IsNullOrEmpty(gradesToSort[gradesToSort.Count - 1].CourseName)) return null;
            return gradesToSort[gradesToSort.Count - 1];
        }
        catch (Exception ex)
        {
            Logger.Default?.LogCatchedException(ex);
            return null;
        }
    }

    /// <summary>
    /// Group grades by their course_unit_id
    /// </summary>
    /// <param name="listOfGrades"></param>
    /// <returns>Groups of grades from first and second term for every subject</returns>
    public static List<FinalGradeGroup> GroupGrades(List<FinalGrade> listOfGrades)
    {
        try
        {
            if (listOfGrades == null)
            {
                return new();
            }
            List<FinalGrade> list = new List<FinalGrade>(listOfGrades);
            List<FinalGradeGroup> groups = new();
            while (list.Count > 0)
            {
                var res = list.Where(x => x.CourseUnitId == list[0].CourseUnitId).ToList();
                groups.Add(new FinalGradeGroup()
                {
                    CourseId = res[0].CourseId,
                    CourseName = res[0].CourseName,
                    FirstTermGrade = res.Where(x => x.ExamSessionNumber == "1").FirstOrDefault(res[0]),
                    SecondTermGrade = res.Where(x => x.ExamSessionNumber == "2").FirstOrDefault(res[1])
                });
                list.RemoveAll(x => x.CourseUnitId == res[0].CourseUnitId);
            }
            return groups;
        }
        catch (Exception ex)
        {
            Logger.Default?.LogCatchedException(ex);
            return new List<FinalGradeGroup>();
        }
    }

    public static List<FinalGrade> UngroupGrades(IEnumerable<FinalGradeGroup> finalGradeGroups)
    {
        try
        {
            List<FinalGrade> finalGrades = new();
            foreach (var item in finalGradeGroups)
            {
                finalGrades.Add(item.FirstTermGrade);
                finalGrades.Add(item.SecondTermGrade);
            }
            return finalGrades;
        }
        catch
        {
            return new List<FinalGrade>();
        }
    }

    /// <summary>
    /// Some univeristies have weird formats for grade symbols e.g. instead of doing only "4.5"
    /// they will add something more e.g. "4.5 (bd)" which won't work with normal float.TryParse.
    /// </summary>
    /// <param name="gradeString">Non standard grade string</param>
    /// <returns></returns>
    static bool TryParseGradeFromNonStandardGradeString(string gradeString, out float parsed)
    {
        gradeString = gradeString.Replace(',', '.');
        var match = Regex.Match(gradeString, @"[-+]?\d*\.?\d+");
        if (match.Success && float.TryParse(match.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parseResult))
        {
            parsed = parseResult;
            return true;
        }
        parsed = 0;
        return false;
    }

    public static float CalculateGradeAverage(IEnumerable<FinalGradeGroup> gradesGroups)
    {
        try
        {
            float top = 0, bottom = 0;
            var gradesGroupsList = new List<FinalGradeGroup>(gradesGroups);
            while (gradesGroupsList.Count > 0)
            {
                var allGradesGroupsForCourse = gradesGroupsList.Where(x => x.CourseId == gradesGroupsList[0].CourseId).ToList();
                List<float> gradesFromAllClassesTypes = new();
                float ectsPoints = 0;
                foreach (var gradesGroup in allGradesGroupsForCourse)
                {
                    if (gradesGroup.FirstTermGrade.CountsIntoAverage == "N")
                    {
                        continue;
                    }

                    float gradesFromTermsSum = 0;
                    int gradesFromTermsCount = 0;
                    FinalGrade? grade = null;
                    if (gradesGroup.SecondTermGrade.IsEmpty == false || gradesGroup.SecondTermGrade.IsModified)
                    {
                        grade = gradesGroup.SecondTermGrade;
                        if (float.TryParse(grade.ValueSymbol.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed) ||
                            TryParseGradeFromNonStandardGradeString(grade.ValueSymbol, out parsed))
                        {
                            gradesFromTermsSum += parsed;
                            gradesFromTermsCount++;
                        }
                    }

                    if (gradesGroup.FirstTermGrade.IsEmpty == false || gradesGroup.FirstTermGrade.IsModified)
                    {
                        grade = gradesGroup.FirstTermGrade;
                        if (float.TryParse(grade.ValueSymbol.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed) ||
                            TryParseGradeFromNonStandardGradeString(grade.ValueSymbol, out parsed))
                        {
                            gradesFromTermsSum += parsed;
                            gradesFromTermsCount++;
                        }
                    }

                    if (gradesFromTermsCount != 0 && grade != null)
                    {
                        gradesFromAllClassesTypes.Add(gradesFromTermsSum / gradesFromTermsCount);
                        ectsPoints = grade.EctsPointsFloat;
                    }

                }
                if (gradesFromAllClassesTypes.Count > 0)
                {
                    top += ectsPoints * gradesFromAllClassesTypes.Sum() / gradesFromAllClassesTypes.Count;
                    bottom += ectsPoints;
                }
                gradesGroupsList.RemoveAll(x => allGradesGroupsForCourse.Contains(x));
            }
            if (bottom == 0) return 0f;
            return (float)Math.Round((double)top / bottom, 2);
        }
        catch (Exception ex)
        {
            Logger.Default?.LogCatchedException(ex);
            return 0f;
        }
    }

    public static void AssignAcademicGroupsToGrades(IEnumerable<Groups.Models.Group> groups, IEnumerable<FinalGrade> grades)
    {
        foreach (var item in grades)
        {
            var foundGroup = groups.FirstOrDefault(x => x.CourseId == item.CourseId);
            if (foundGroup == null) continue;
            item.Group = foundGroup;
        }
    }

    public static void CopyGradeDistributionsIfNotSet(IEnumerable<FinalGradeGroup> copyFrom, IEnumerable<FinalGradeGroup> copyTo)
    {
        foreach (var item in copyTo)
        {
            if (string.IsNullOrEmpty(item.FirstTermGrade.GradeDistribution) == false)
            {
                continue;
            }
            var found = copyFrom.FirstOrDefault(x => x?.FirstTermGrade?.CourseUnitId == item?.FirstTermGrade?.CourseUnitId);
            if (found == null) continue;
            item.FirstTermGrade.GradeDistribution = found.FirstTermGrade.GradeDistribution;
        }
    }

    public static void CopyGradeDistributions(IEnumerable<FinalGradeGroup> copyFrom, IEnumerable<FinalGradeGroup> copyTo)
    {
        foreach (var item in copyTo)
        {
            var found = copyFrom.FirstOrDefault(x => x?.FirstTermGrade?.CourseUnitId == item?.FirstTermGrade?.CourseUnitId);
            if (found == null) continue;
            item.FirstTermGrade.GradeDistribution = found.FirstTermGrade.GradeDistribution;
        }
    }
}