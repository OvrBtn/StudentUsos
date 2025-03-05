using System.Text.Json.Serialization;

namespace StudentUsos.Features.UserInfo;

[JsonSerializable(typeof(UserInfo))]
internal partial class UserInfoJsonContext : JsonSerializerContext
{ }

public class UserInfo : Person.Models.Person
{
    [JsonPropertyName("student_number")]
    public string StudentNumber { get; set; }
}