using StudentUsos.Features.CampusMap.Models;
using System.Net;

namespace StudentUsos.Features.CampusMap.Services;

public interface ICampusMapService
{
    public Task<string?> GetCampusMapSvgAsync();
    public Task<string?> GetBuildingsDataAsync();
    public Task<List<CampusBuilding>?> GetBuildingsDataDeserializedAsync();

    public Task<string?> GetFloorSvgAsync(string buildingId, string floor);
    public Task<string?> GetFloorDataAsync(string buildingId, string floor);
    public Task<List<RoomInfo>?> GetFloorDataDeserializedAsync(string buildingId, string floor);

    public Task<HttpStatusCode?> SendUserSuggestionAsync(string suggestedName, string buildingId, string floor, string roomId, string studentNumber);

    public Task<HttpStatusCode?> UpvoteUserSuggestionAsync(string buildingId, string floor, int roomId, int suggestionId);
    public Task<HttpStatusCode?> DownvoteUserSuggestionAsync(string buildingId, string floor, int roomId, int suggestionId);
    public Task<List<string>?> FetchIdsOfUsersUpvotesAsync();
    public Task<List<string>?> FetchIdsOfUsersDownvotesAsync();
}
