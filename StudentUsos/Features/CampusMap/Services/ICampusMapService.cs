using StudentUsos.Features.CampusMap.Models;
using System.Net;

namespace StudentUsos.Features.CampusMap.Services;

public interface ICampusMapService
{
    public Task<string?> GetCampusMapSvg();
    public Task<string?> GetBuildingsData();
    public Task<List<CampusBuilding>?> GetBuildingsDataDeserialized();

    public Task<string?> GetFloorSvg(string buildingId, string floor);
    public Task<string?> GetFloorData(string buildingId, string floor);
    public Task<List<RoomInfo>?> GetFloorDataDeserialized(string buildingId, string floor);

    public Task<HttpStatusCode?> SendUserSuggestion(string suggestedName, string buildingId, string floor, string roomId, string studentNumber);

    public Task<HttpStatusCode?> UpvoteUserSuggestion(string buildingId, string floor, int roomId, int suggestionId);
    public Task<HttpStatusCode?> DownvoteUserSuggestion(string buildingId, string floor, int roomId, int suggestionId);
    public Task<List<string>?> FetchIdsOfUsersUpvotes();
    public Task<List<string>?> FetchIdsOfUsersDownvotes();
}
