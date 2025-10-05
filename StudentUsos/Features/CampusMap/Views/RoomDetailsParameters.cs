using StudentUsos.Features.CampusMap.Models;

namespace StudentUsos.Features.CampusMap.Views;

public class RoomDetailsParameters
{
    public string? RoomName { get; set; }
    public bool HasRoomName { get => string.IsNullOrEmpty(RoomName) == false; }
    public string? Description { get; set; }
    public List<string>? AdditionalRoomNames { get; set; }
    public List<RoomInfo> FullRoomInfos { get; set; }
    public string AdditionalRoomNamesJoined { get => string.Join(", ", AdditionalRoomNames ?? new()); }
    public bool HasAdditionalRoomNames { get => AdditionalRoomNames is not null && AdditionalRoomNames.Count > 0; }
    public required Action<string> ConfirmAction { get; set; }
}

