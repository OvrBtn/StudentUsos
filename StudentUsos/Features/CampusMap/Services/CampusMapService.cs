using StudentUsos.Features.CampusMap.Models;
using StudentUsos.Services.ServerConnection;
using System.Text.Json;

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

    public async Task<List<CampusBuilding>?> GetBuildingsDataDeserialized()
    {
        var json = await GetBuildingsData();
        if (json is null)
        {
            return null;
        }
        return JsonSerializer.Deserialize(json, CampusBuildingJsonContext.Default.ListCampusBuilding);
    }

    public async Task<string?> GetFloorData(string buildingId, string floor)
    {
        var payload = new Dictionary<string, string>()
        {
            { "buildingId", buildingId },
            { "floor", floor },
        };
        var response = await serverConnectionManager.SendAuthorizedGetRequestAsync("CampusMap/FloorData", payload, AuthorizationMode.Full);
        if (response is null || response.IsSuccess == false)
        {
            return null;
        }
        return response.Response;
    }

    public async Task<List<FloorData>?> GetFloorDataDeserialized(string buildingId, string floor)
    {
        var response = await GetFloorData(buildingId, floor);
        if (response is null)
        {
            return null;
        }
        return JsonSerializer.Deserialize(response, FloorDataJsonContext.Default.ListFloorData);
    }

    public async Task<string?> GetFloorSvg(string buildingId, string floor)
    {
        var payload = new Dictionary<string, string>()
        {
            { "buildingId", buildingId },
            { "floor", floor },
        };
        var response = await serverConnectionManager.SendAuthorizedGetRequestAsync("CampusMap/FloorSvg", payload, AuthorizationMode.Full);
        if (response is null || response.IsSuccess == false)
        {
            return null;
        }
        return response.Response;
    }
}
