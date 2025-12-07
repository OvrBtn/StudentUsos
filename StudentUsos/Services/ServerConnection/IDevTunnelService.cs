namespace StudentUsos.Services.ServerConnection;

public interface IDevTunnelService
{
    public void SaveDevTunnelId(string devTunnelId);
    public string? GetDevTunnelId();
    public void DeleteDevTunnel();
    public string? GetFullDevTunnelUrl();
}
