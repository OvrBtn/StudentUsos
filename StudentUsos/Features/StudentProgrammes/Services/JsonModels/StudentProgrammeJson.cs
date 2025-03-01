using System.Text.Json.Serialization;
using StudentUsos.Features.StudentProgrammes.Models;

namespace StudentUsos.Features.StudentProgrammes.Services.JsonModels
{
    public class StudentProgrammeJson
    {
        [JsonPropertyName("programme")]
        public ProgrammeJson Programme { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonPropertyName("admission_date")]
        public string AdmissionDate { get; set; }
        [JsonPropertyName("is_primary"), JsonConverter(typeof(JsonObjectToStringConverter))]
        public string IsPrimary { get; set; }
    }

    public static class StudentProgrammeJsonExtensions
    {
        public static StudentProgramme ToStudentProgramme(this StudentProgrammeJson studentProgrammeJson)
        {
            return new()
            {
                ProgrammeId = studentProgrammeJson.Programme.Id,
                DescriptionJson = studentProgrammeJson.Programme.DescriptionJson,
                Status = studentProgrammeJson.Status,
                AdmissionDate = studentProgrammeJson.AdmissionDate,
                IsPrimary = studentProgrammeJson.IsPrimary
            };
        }
    }
}
