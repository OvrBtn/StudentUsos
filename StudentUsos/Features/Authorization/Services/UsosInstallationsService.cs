using StudentUsos.Features.Authorization.Models;
using StudentUsos.Services.ServerConnection;
using System.Text.Json;

namespace StudentUsos.Features.Authorization.Services;

public class UsosInstallationsService : IUsosInstallationsService
{
    ILocalStorageManager localStorageManager;
    Lazy<IServerConnectionManager> serverConnectionManager;
    public UsosInstallationsService(ILocalStorageManager localStorageManager, Lazy<IServerConnectionManager> serverConnectionManager)
    {
        this.localStorageManager = localStorageManager;
        this.serverConnectionManager = serverConnectionManager;
    }

    public string? GetCurrentInstallation()
    {
        return localStorageManager.GetString(LocalStorageKeys.UsosInstallationsService_CurrentInstallation);
    }

    public void SaveCurrentInstallation(string installationUrl)
    {
        localStorageManager.SetString(LocalStorageKeys.UsosInstallationsService_CurrentInstallation, installationUrl);
    }

    public List<UsosInstallation>? UsosInstallationsCache { get; set; } = null;

    public async Task<List<UsosInstallation>?> GetUsosInstallationsAsync()
    {
        var result = await serverConnectionManager.Value.SendAuthorizedGetRequestAsync("usos/UsosInstallations", new(), AuthorizationMode.StaticInternalsOnly);
        if (result is null || result.IsSuccess == false)
        {
            return null;
        }

        var deserialized = JsonSerializer.Deserialize(result.Response, UsosInstallationJsonContext.Default.ListUsosInstallation);
        return deserialized;
    }
}