using System.Text.Json.Serialization;

namespace StudentUsos.Features.Authorization.Models;

[JsonSerializable(typeof(List<UsosInstallation>))]
public partial class UsosInstallationJsonContext : JsonSerializerContext
{ }

public class UsosInstallation
{
    public string InstallationId { get; set; }
    public string InstallationUrl { get; set; }
    public Dictionary<string, string> LocalizedName { get; set; }
    public string Name => Utilities.GetLocalizedString(LocalizedName);
}
