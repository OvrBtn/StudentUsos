namespace StudentUsos.Services.ServerConnection;

public class DevTunnelService : IDevTunnelService
{
    ILocalStorageManager localStorageManager;
    public DevTunnelService(ILocalStorageManager localStorageManager)
    {
        this.localStorageManager = localStorageManager;
    }

    public void DeleteDevTunnel()
    {
        localStorageManager.Remove(LocalStorageKeys.DevTunnelService_DevTunnelId);
    }

    public string? GetDevTunnelId()
    {
        return localStorageManager.GetString(LocalStorageKeys.DevTunnelService_DevTunnelId);
    }

    public string? GetFullDevTunnelUrl()
    {
        string? tunnelId = localStorageManager.GetString(LocalStorageKeys.DevTunnelService_DevTunnelId);
        if (tunnelId is null)
        {
            return null;
        }
        return $"https://{tunnelId}.devtunnels.ms/";
    }

    public void SaveDevTunnelId(string devTunnelId)
    {
        localStorageManager.SetString(LocalStorageKeys.DevTunnelService_DevTunnelId, devTunnelId);
    }
}
