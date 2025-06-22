namespace StudentUsos.Features.CampusMap.Services;

interface ICampusMapService
{
    public Task<string?> GetCampusMapSvg();
    public Task<string?> GetBuildingsData();

    public Task<string> GetFloorSvg(string buildingId, string floor);
    public Task<string> GetFloorData(string buildingId, string floor);
}
