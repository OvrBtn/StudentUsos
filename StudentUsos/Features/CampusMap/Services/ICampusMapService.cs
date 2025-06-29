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
    public Task<List<FloorData>?> GetFloorDataDeserialized(string buildingId, string floor);

    public Task<HttpStatusCode?> SendUserSuggestion(string suggestedName, string buildingId, string floor, string roomId, string studentNumber);
}
