using System.Text.Json.Serialization;

namespace StudentUsos.Features.Settings.Models
{
    [JsonSerializable(typeof(List<Language>))]
    public partial class LanguageJsonContext : JsonSerializerContext { }

    public class Language
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
