using StudentUsos.Services.ServerConnection;

namespace StudentUsos.Features.CampusMap.Services;

public class CampusMapService : ICampusMapService
{
    IServerConnectionManager serverConnectionManager;
    public CampusMapService(IServerConnectionManager serverConnectionManager)
    {
        this.serverConnectionManager = serverConnectionManager;
    }

    public async Task<string?> GetCampusMapSvg()
    {
        var response = await serverConnectionManager.SendAuthorizedGetRequestAsync("CampusMap/CampusSvg", new(), AuthorizationMode.Full);
        if (response is null || response.IsSuccess == false)
        {
            return null;
        }
        return response.Response;
    }

    public async Task<string?> GetBuildingsData()
    {
        var response = await serverConnectionManager.SendAuthorizedGetRequestAsync("CampusMap/BuildingsList", new(), AuthorizationMode.Full);
        if (response is null || response.IsSuccess == false)
        {
            return null;
        }
        return response.Response;
    }

    public Task<string> GetFloorData(string buildingId, string floor)
    {
        throw new NotImplementedException();
    }

    public Task<string> GetFloorSvg(string buildingId, string floor)
    {
        throw new NotImplementedException();
    }
}
