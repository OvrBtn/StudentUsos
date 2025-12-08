using System.Text.Json.Serialization;

namespace StudentUsos.Features.Grades.Services;

[JsonSerializable(typeof(Dictionary<string, Dictionary<string, List<GradeDistributionJsonRoot>>>))]
internal partial class GradeDistributionJsonRootJsonContext : JsonSerializerContext
{ }

internal class GradeDistributionJsonRoot
{
    [JsonPropertyName("grades_distribution"), JsonConverter(typeof(JsonObjectToStringConverter))]
    public string GradeDistribution { get; set; }
    //in case of some installations using course_grades (1 grade for whole subject) this might be null
    [JsonPropertyName("course_unit")]
    public CourseUnitJson? CourseUnit { get; set; }
}

internal class CourseUnitJson
{
    [JsonPropertyName("id")]
    public string CourseUnitId { get; set; }
}