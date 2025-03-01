using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudentUsos.Features.Person.Models
{
    [JsonSerializable(typeof(List<EmploymentPositionJson>))]
    internal partial class EmploymentPositionJsonContext : JsonSerializerContext { }

    public class EmploymentPosition
    {
        public string PositionId { get; set; }
        public string PositionName { get => Utilities.GetLocalizedStringFromJson(PositionNameJson); }
        public string PositionNameJson { get; set; }
        public string FacultyId { get; set; }
        public string FacultyName { get => Utilities.GetLocalizedStringFromJson(FacultyNameJson); }
        public string FacultyNameJson { get; set; }

        public static List<EmploymentPosition>? Deserialize(string employmentPositions)
        {
            var deserialized = JsonSerializer.Deserialize(employmentPositions, EmploymentPositionJsonContext.Default.ListEmploymentPositionJson);
            if (deserialized is null)
            {
                return null;
            }
            List<EmploymentPosition> result = new();
            foreach (var item in deserialized)
            {
                result.Add(new(item));
            }
            return result;
        }

        EmploymentPosition(EmploymentPositionJson employmentPositionJson)
        {
            PositionId = employmentPositionJson.Position.Id;
            PositionNameJson = employmentPositionJson.Position.Name;
            FacultyId = employmentPositionJson.Faculty.Id;
            FacultyNameJson = employmentPositionJson.Faculty.NameJson;
        }
    }

    class EmploymentPositionJson
    {
        [JsonPropertyName("position")]
        public Position Position { get; set; }
        [JsonPropertyName("faculty")]
        public Faculty Faculty { get; set; }
    }

    class Position
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("name"), JsonConverter(typeof(JsonObjectToStringConverter))]
        public string Name { get; set; }
    }

}
