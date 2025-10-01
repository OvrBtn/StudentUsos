using StudentUsos.Features.CampusMap.Models;
using StudentUsos.Features.UserInfo;
using StudentUsos.Services.ServerConnection;
using System.Net;
using System.Text.Json;

namespace StudentUsos.Features.CampusMap.Services;

public class CampusMapService : ICampusMapService
{
    IServerConnectionManager serverConnectionManager;
    IUserInfoRepository userInfoRepository;
    public CampusMapService(IServerConnectionManager serverConnectionManager, IUserInfoRepository userInfoRepository)
    {
        this.serverConnectionManager = serverConnectionManager;
        this.userInfoRepository = userInfoRepository;
    }

    public async Task<string?> GetCampusMapSvgAsync()
    {
        var response = await serverConnectionManager.SendAuthorizedGetRequestAsync("CampusMap/CampusSvg", new(), AuthorizationMode.Full);
        if (response is null || response.IsSuccess == false)
        {
            return null;
        }
        return response.Response;
    }

    public async Task<string?> GetBuildingsDataAsync()
    {
        var response = await serverConnectionManager.SendAuthorizedGetRequestAsync("CampusMap/BuildingsList", new(), AuthorizationMode.Full);
        if (response is null || response.IsSuccess == false)
        {
            return null;
        }
        return response.Response;
    }

    public async Task<List<CampusBuilding>?> GetBuildingsDataDeserializedAsync()
    {
        var json = await GetBuildingsDataAsync();
        if (json is null)
        {
            return null;
        }
        var deserialized = JsonSerializer.Deserialize(json, CampusBuildingJsonContext.Default.ListCampusBuilding);
        return deserialized;
    }

    public async Task<string?> GetFloorDataAsync(string buildingId, string floor)
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

    public async Task<List<RoomInfo>?> GetFloorDataDeserializedAsync(string buildingId, string floor)
    {
        var response = await GetFloorDataAsync(buildingId, floor);
        if (response is null)
        {
            return null;
        }
        return JsonSerializer.Deserialize(response, RoomInfoJsonContext.Default.ListRoomInfo);
    }

    public async Task<string?> GetFloorSvgAsync(string buildingId, string floor)
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

    public async Task<HttpStatusCode?> SendUserSuggestionAsync(string suggestedName, string buildingId, string floor, string roomId, string studentNumber)
    {
        var payload = new Dictionary<string, string>()
        {
            { "SuggestedRoomName", suggestedName },
            { "BuildingId", buildingId },
            { "Floor", floor },
            { "RoomId", roomId },
            { "UserStudentNumber", studentNumber }
        };
        var response = await serverConnectionManager.SendAuthorizedPostRequestAsync("CampusMap/UserSuggestion", payload, AuthorizationMode.Full);
        if (response is null)
        {
            return null;
        }
        return response.HttpResponseMessage.StatusCode;
    }

    public async Task<HttpStatusCode?> UpvoteUserSuggestionAsync(string buildingId, string floor, int roomId, int suggestionId)
    {
        var userInfo = userInfoRepository.GetUserInfo();
        if (userInfo is null)
        {
            return null;
        }

        var payload = new Dictionary<string, string>()
        {
            { "BuildingId", buildingId },
            { "Floor", floor },
            { "RoomId", roomId.ToString() },
            { "InternalUserSuggestionId", suggestionId.ToString() },
            { "StudentNumber", userInfo.StudentNumber }
        };
        var response = await serverConnectionManager.SendAuthorizedPostRequestAsync("CampusMap/UpvoteUserSuggestion", payload, AuthorizationMode.Full);
        if (response is null)
        {
            return null;
        }
        return response.HttpResponseMessage.StatusCode;
    }

    public async Task<HttpStatusCode?> DownvoteUserSuggestionAsync(string buildingId, string floor, int roomId, int suggestionId)
    {
        var userInfo = userInfoRepository.GetUserInfo();
        if (userInfo is null)
        {
            return null;
        }

        var payload = new Dictionary<string, string>()
        {
            { "BuildingId", buildingId },
            { "Floor", floor },
            { "RoomId", roomId.ToString() },
            { "InternalUserSuggestionId", suggestionId.ToString() },
            { "StudentNumber", userInfo.StudentNumber }
        };
        var response = await serverConnectionManager.SendAuthorizedPostRequestAsync("CampusMap/DownvoteUserSuggestion", payload, AuthorizationMode.Full);
        if (response is null)
        {
            return null;
        }
        return response.HttpResponseMessage.StatusCode;
    }

    public async Task<List<string>?> FetchIdsOfUsersUpvotesAsync()
    {
        var response = await serverConnectionManager.SendAuthorizedGetRequestAsync("CampusMap/UsersUpvotes", new(), AuthorizationMode.Full);
        if (response is null || response.IsSuccess == false)
        {
            return null;
        }

        return JsonSerializer.Deserialize(response.Response, JsonContext.Default.ListString);
    }

    public async Task<List<string>?> FetchIdsOfUsersDownvotesAsync()
    {
        var response = await serverConnectionManager.SendAuthorizedGetRequestAsync("CampusMap/UsersDownvotes", new(), AuthorizationMode.Full);
        if (response is null || response.IsSuccess == false)
        {
            return null;
        }

        return JsonSerializer.Deserialize(response.Response, JsonContext.Default.ListString);
    }
}
