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

        EnsureListOfVotesIsInitialized();
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

    List<string> idsOfUpvotedRoomInfos = new();
    List<string> idsOfDownvotedRoomInfos = new();
    bool areListsOfVotesInitialized = false;

    void EnsureListOfVotesIsInitialized()
    {
        if (areListsOfVotesInitialized)
        {
            return;
        }

        string upvoted, downvoted;

        if (localStorageManager.TryGettingData(LocalStorageKeys.IdsOfUpvotedRoomInfos, out upvoted) == false ||
            localStorageManager.TryGettingData(LocalStorageKeys.IdsOfDownvotedRoomInfos, out downvoted) == false)
        {
            localStorageManager.SetData(LocalStorageKeys.IdsOfUpvotedRoomInfos, string.Empty);
            localStorageManager.SetData(LocalStorageKeys.IdsOfDownvotedRoomInfos, string.Empty);
            return;
        }

        idsOfUpvotedRoomInfos = upvoted.Split('|').ToList();
        idsOfDownvotedRoomInfos = downvoted.Split('|').ToList();

        areListsOfVotesInitialized = true;
    }

    void SaveListsOfVotes()
    {
        localStorageManager.SetData(LocalStorageKeys.IdsOfUpvotedRoomInfos, string.Join('|', idsOfUpvotedRoomInfos));
        localStorageManager.SetData(LocalStorageKeys.IdsOfDownvotedRoomInfos, string.Join('|', idsOfDownvotedRoomInfos));
    }

    public void MarkAsUpvoted(RoomInfo roomInfo)
    {
        idsOfUpvotedRoomInfos.Add(roomInfo.InternalId.ToString());
        SaveListsOfVotes();
    }

    public void UnmarkAsUpvoted(RoomInfo roomInfo)
    {
        idsOfUpvotedRoomInfos.Remove(roomInfo.InternalId.ToString());
        SaveListsOfVotes();
    }

    public void MarkAsDownvoted(RoomInfo roomInfo)
    {
        idsOfDownvotedRoomInfos.Add(roomInfo.InternalId.ToString());
        SaveListsOfVotes();
    }

    public void UnmarkAsDownvoted(RoomInfo roomInfo)
    {
        idsOfDownvotedRoomInfos.Remove(roomInfo.InternalId.ToString());
        SaveListsOfVotes();
    }

    public bool IsUpvoted(RoomInfo roomInfo)
    {
        return idsOfUpvotedRoomInfos.Contains(roomInfo.InternalId.ToString());
    }

    public bool IsDownvoted(RoomInfo roomInfo)
    {
        return idsOfDownvotedRoomInfos.Contains(roomInfo.InternalId.ToString());
    }

    public void ImportUpvotes(List<string> upvotes)
    {
        idsOfUpvotedRoomInfos = new(upvotes);
        SaveListsOfVotes();
    }

    public void ImportDownvotes(List<string> downvotes)
    {
        idsOfDownvotedRoomInfos = new(downvotes);
        SaveListsOfVotes();
    }
}
