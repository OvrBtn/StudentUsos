using System.Text.Json.Serialization;

namespace StudentUsos.Features.Authorization.Models;

[JsonSerializable(typeof(List<UsosInstallation>))]
public partial class UsosInstallationJsonContext : JsonSerializerContext
{ }

public class UsosInstallation : IEquatable<UsosInstallation>
{
    public string InstallationId { get; set; }
    public string InstallationUrl { get; set; }
    public Dictionary<string, string> LocalizedName { get; set; }
    public string Name => Utilities.GetLocalizedString(LocalizedName);

    public bool Equals(UsosInstallation? other)
    {
        return this.InstallationId == other?.InstallationId && this.InstallationUrl == other?.InstallationUrl && this.Name == other?.Name;
    }
}
