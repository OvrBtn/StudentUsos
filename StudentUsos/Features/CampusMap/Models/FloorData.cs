using System.Text.Json.Serialization;

namespace StudentUsos.Features.CampusMap.Models;

[JsonSerializable(typeof(List<FloorData>))]
public partial class FloorDataJsonContext : JsonSerializerContext
{ }

public class FloorData
{
    public int InternalId { get; set; }
    public required int RoomId { get; set; }
    public required string BuildingId { get; set; }
    public required string Floor { get; set; }
    public required string Name { get; set; }
    public required int NameWeight { get; set; }
}
