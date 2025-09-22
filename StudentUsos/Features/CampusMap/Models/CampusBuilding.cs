using SQLite;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudentUsos.Features.CampusMap.Models;

[JsonSerializable(typeof(List<CampusBuilding>))]
public partial class CampusBuildingJsonContext : JsonSerializerContext
{ }

public class CampusBuilding
{
    public string Id { get; set; }
    [JsonPropertyName("Name"), JsonConverter(typeof(JsonObjectToStringConverter))]
    public string NameJson { get; set; }
    public string LocalizedName
    {
        get
        {
            if (string.IsNullOrEmpty(localizedName))
            {
                localizedName = Utilities.GetLocalizedStringFromJson(NameJson);
            }
            return localizedName;
        }
        set
        {
            localizedName = value;
        }
    }
    string localizedName;
    [JsonPropertyName("Floors"), JsonConverter(typeof(JsonObjectToStringConverter))]
    public string? FloorsJson { get; set; }
    [Ignore]
    public List<string> FloorsList
    {
        get
        {
            if (floors is null)
            {
                if (FloorsJson is null)
                {
                    floors = new();
                }
                else
                {
                    floors = JsonSerializer.Deserialize(FloorsJson, JsonContext.Default.ListString) ?? new();
                }
            }
            return floors;
        }
        set
        {
            floors = value;
        }
    }
    List<string> floors;
}
