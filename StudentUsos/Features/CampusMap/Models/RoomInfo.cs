using System.Text.Json.Serialization;

namespace StudentUsos.Features.CampusMap.Models;


[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(List<RoomInfo>))]
public partial class RoomInfoJsonContext : JsonSerializerContext
{ }

public class RoomInfo
{
    public int InternalId { get; set; }
    public int RoomId { get; set; }
    public string BuildingId { get; set; }
    public string Floor { get; set; }
    public string Name { get; set; }
    public int NameWeight { get; set; }

}
