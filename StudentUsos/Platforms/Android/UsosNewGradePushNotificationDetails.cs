using System.Text.Json.Serialization;

namespace StudentUsos.Platforms.Android
{
    [JsonSerializable(typeof(UsosNewGradePushNotificationDetails))]
    internal partial class UsosNewGradePushNotificationDetailsJsonContext : JsonSerializerContext
    { }

    internal class UsosNewGradePushNotificationDetails
    {
        [JsonInclude, JsonPropertyName("value_symbol")]
        internal string ValueSymbol { get; set; }
        [JsonInclude, JsonPropertyName("course_edition")]
        internal CourseEdition CourseEdition { get; set; }
    }

    internal class CourseEdition
    {
        [JsonInclude, JsonPropertyName("course_name")]
        internal Dictionary<string, string> CourseNameLocalized { get; set; }
    }

}
