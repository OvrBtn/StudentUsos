using System.Text.Json.Serialization;

namespace StudentUsos.Features.Grades.Services
{
    [JsonSerializable(typeof(Dictionary<string, Dictionary<string, List<GradeDistributionJsonRoot>>>))]
    internal partial class GradeDistributionJsonRootJsonContext : JsonSerializerContext
    { }

    internal class GradeDistributionJsonRoot
    {
        [JsonPropertyName("grades_distribution"), JsonConverter(typeof(JsonObjectToStringConverter))]
        public string GradeDistribution { get; set; }
        [JsonPropertyName("course_unit")]
        public CourseUnitJson CourseUnit { get; set; }
    }

    internal class CourseUnitJson
    {
        [JsonPropertyName("id")]
        public string CourseUnitId { get; set; }
    }
}
