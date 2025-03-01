using System.Text.Json.Serialization;
using StudentUsos.Features.AcademicTerms.Models;
using StudentUsos.Features.Groups.Models;

namespace StudentUsos.Features.Groups.Services
{
    [JsonSerializable(typeof(GroupsJsonRoot))]
    public partial class GroupsJsonRootContext : JsonSerializerContext
    {

    }

    public class GroupsJsonRoot
    {
        [JsonPropertyName("groups")]
        public Dictionary<string, List<Group>> Groups { get; set; }
        [JsonPropertyName("terms")]
        public List<Term> Terms { get; set; }
    }
}
