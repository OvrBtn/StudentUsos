using System.Text.Json.Serialization;

namespace StudentUsos.Features.CampusMap.Models;


[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(List<RoomInfo>))]
public partial class RoomInfoJsonContext : JsonSerializerContext
{ }

public class RoomInfo
{
    public int InternalId { get; set; }
    public required int RoomId { get; set; }
    public required string BuildingId { get; set; }
    public required string Floor { get; set; }
    public required string Name { get; set; }
    public required int NameWeight { get; set; }

}
