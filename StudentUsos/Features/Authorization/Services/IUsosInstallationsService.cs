using StudentUsos.Features.Authorization.Models;

namespace StudentUsos.Features.Authorization.Services;

public interface IUsosInstallationsService
{
    public Task<List<UsosInstallation>?> GetUsosInstallationsAsync();
    public void SaveCurrentInstallation(string installationUrl);
    public string? GetCurrentInstallation();
}