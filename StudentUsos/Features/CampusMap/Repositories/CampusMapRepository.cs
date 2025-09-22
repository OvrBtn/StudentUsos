using StudentUsos.Features.CampusMap.Models;

namespace StudentUsos.Features.CampusMap.Repositories;

public class CampusMapRepository : ICampusMapRepository
{
    ILocalDatabaseManager localDatabaseManager;
    ILocalStorageManager localStorageManager;
    public CampusMapRepository(ILocalDatabaseManager localDatabaseManager, ILocalStorageManager localStorageManager)
    {
        this.localDatabaseManager = localDatabaseManager;
        this.localStorageManager = localStorageManager;
    }

    public string? GetCampusMap()
    {
        if (localStorageManager.TryGettingData(LocalStorageKeys.CampusMapSvg, out string svg))
        {
            return svg;
        }
        return null;
    }

    public void SaveCampusMap(string svg)
    {
        localStorageManager.SetData(LocalStorageKeys.CampusMapSvg, svg);
    }

    public List<CampusBuilding> GetBuildingsData()
    {
        return localDatabaseManager.GetAll<CampusBuilding>();
    }

    public void SaveBuildingsData(List<CampusBuilding> buildings)
    {
        localDatabaseManager.ClearTable<CampusBuilding>();
        localDatabaseManager.InsertAll(buildings);
    }

    public FloorMap? GetFloorMap(string buildingId, string floor)
    {
        return localDatabaseManager.Get<FloorMap>(x => x.BuildingId == buildingId && x.Floor == floor);
    }

    public void SaveFloorMap(FloorMap floorMap)
    {
        localDatabaseManager.Remove<FloorMap>($"BuildingId=\"{floorMap.BuildingId}\" AND Floor=\"{floorMap.Floor}\"");
        localDatabaseManager.Insert(floorMap);
    }

    public void SaveFloorMap(string buildingId, string floor, string floorSvg)
    {
        localDatabaseManager.Remove<FloorMap>($"BuildingId=\"{buildingId}\" AND Floor=\"{floor}\"");
        FloorMap floorMap = new()
        {
            BuildingId = buildingId,
            Floor = floor,
            FloorSvg = floorSvg
        };
        localDatabaseManager.Insert(floorMap);
    }

    public List<RoomInfo> GetFloorData(string buildingId, string floor)
    {
        return localDatabaseManager.GetAll<RoomInfo>(x => x.BuildingId == buildingId && x.Floor == floor);
    }

    public void SaveFloorData(string buildingId, string floor, List<RoomInfo> roomInfos)
    {
        localDatabaseManager.Remove<RoomInfo>($"BuildingId=\"{buildingId}\" AND Floor=\"{floor}\"");
        localDatabaseManager.InsertAll(roomInfos);
    }
}
