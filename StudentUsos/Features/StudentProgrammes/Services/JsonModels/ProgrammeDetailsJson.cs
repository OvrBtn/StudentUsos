using System.Text.Json.Serialization;

namespace StudentUsos.Features.StudentProgrammes.Services.JsonModels
{
    public class ProgrammeDetailsJson
    {
        [JsonPropertyName("name"), JsonConverter(typeof(JsonObjectToStringConverter))]
        public string NameJson { get; set; }
        [JsonPropertyName("faculty")]
        public Faculty Faculty { get; set; }
    }
}
