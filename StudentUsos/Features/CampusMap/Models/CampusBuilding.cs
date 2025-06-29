using System.Text.Json.Serialization;

namespace StudentUsos.Features.CampusMap.Models;

[JsonSerializable(typeof(List<CampusBuilding>))]
public partial class CampusBuildingJsonContext : JsonSerializerContext
{ }

public class CampusBuilding
{
    public string Id { get; set; }
    [JsonPropertyName("Name")]
    public Dictionary<string, string> NamesDictionary { get; set; }
    public string LocalizedName
    {
        get
        {
            if (string.IsNullOrEmpty(localizedName))
            {
                localizedName = Utilities.GetLocalizedString(NamesDictionary);
            }
            return localizedName;
        }
        set
        {
            localizedName = value;
        }
    }
    string localizedName;
    public List<string> Floors { get; set; } = new();
}
