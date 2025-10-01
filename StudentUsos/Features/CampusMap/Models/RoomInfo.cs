using SQLite;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace StudentUsos.Features.CampusMap.Models;


[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(List<RoomInfo>))]
public partial class RoomInfoJsonContext : JsonSerializerContext
{ }

public class RoomInfo : INotifyPropertyChanged
{
    public int InternalId { get; set; }
    public int RoomId { get; set; }
    public string BuildingId { get; set; }
    public string Floor { get; set; }
    public string Name { get; set; }
    public int NameWeight { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    [Ignore]
    public bool IsUpvoted
    {
        get => isUpvoted;
        set
        {
            isUpvoted = value;
            PropertyChanged?.Invoke(this, new(nameof(IsUpvoted)));
        }
    }
    bool isUpvoted;

    [Ignore]
    public bool IsDownvoted
    {
        get => isDownvoted;
        set
        {
            isDownvoted = value;
            PropertyChanged?.Invoke(this, new(nameof(IsDownvoted)));
        }
    }
    bool isDownvoted;

}
