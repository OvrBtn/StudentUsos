using System.Text.Json.Serialization;

namespace StudentUsos.Features.Grades.Services
{
    [JsonSerializable(typeof(List<GradeDistributionRecord>))]
    internal partial class GradeDistributionJsonContext : JsonSerializerContext
    { }

    internal class GradeDistributionRecord
    {
        [JsonPropertyName("grade_symbol")]
        public string GradeSymbol { get; set; }
        [JsonPropertyName("percentage")]
        public float Percentage { get; set; } = 0f;
    }
}
