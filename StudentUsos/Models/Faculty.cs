using System.Text.Json.Serialization;

namespace StudentUsos.Models;

[JsonSerializable(typeof(Faculty))]
internal partial class FacultyJsonContext : JsonSerializerContext
{ }

public class Faculty
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("name"), JsonConverter(typeof(JsonObjectToStringConverter))]
    public string NameJson { get; set; }
}