using StudentUsos.Features.Grades.Models;
using System.Text.Json.Serialization;

namespace StudentUsos.Features.Grades.Services;

[JsonSerializable(typeof(Dictionary<string, Dictionary<string, CourseIdJsonObject>>))]
internal partial class FinalGradeJsonContext : JsonSerializerContext
{ }

internal class CourseIdJsonObject
{
    [JsonPropertyName("course_units_grades")]
    public Dictionary<string, List<Dictionary<string, FinalGrade>>> CourseUnitsGrades { get; set; }
    /// <summary>
    /// In case of USOS API for PUT this is always just an empty list
    /// </summary>
    [JsonPropertyName("course_grades")]
    public List<Dictionary<string, FinalGrade>>? CourseGrades { get; set; }
}