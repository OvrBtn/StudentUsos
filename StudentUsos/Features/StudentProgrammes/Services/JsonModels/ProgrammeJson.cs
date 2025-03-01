using System.Text.Json.Serialization;

namespace StudentUsos.Features.StudentProgrammes.Services.JsonModels
{
    public class ProgrammeJson
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("description"), JsonConverter(typeof(JsonObjectToStringConverter))]
        public string DescriptionJson { get; set; }
    }
}
