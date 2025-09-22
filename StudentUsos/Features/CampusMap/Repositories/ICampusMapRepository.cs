using StudentUsos.Features.CampusMap.Models;

namespace StudentUsos.Features.CampusMap.Repositories;

public interface ICampusMapRepository
{
    public string? GetCampusMap();
    public void SaveCampusMap(string svg);

    public List<CampusBuilding> GetBuildingsData();
    public void SaveBuildingsData(List<CampusBuilding> buildings);

    public FloorMap? GetFloorMap(string buildingId, string floor);
    public void SaveFloorMap(FloorMap floorMap);
    public void SaveFloorMap(string buildingId, string floor, string floorMap);

    public List<RoomInfo> GetFloorData(string buildingId, string floor);
    public void SaveFloorData(string buildingId, string floor, List<RoomInfo> roomInfos);
}
