using System.Text.Json.Serialization;

namespace StudentUsos.Features.CampusMap.Models;

[JsonSerializable(typeof(List<CampusBuilding>))]
public partial class CampusBuildingJsonContext : JsonSerializerContext
{ }

public class CampusBuilding
{
    public string Id { get; set; }
    public Dictionary<string, string> Name { get; set; }
    public string LocalizedName { get => Utilities.GetLocalizedString(Name); }
    public List<string> Floors { get; set; }
}
