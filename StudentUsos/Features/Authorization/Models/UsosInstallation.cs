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

    public UsosInstallation()
    {

    }

    public bool Equals(UsosInstallation? other)
    {
        return this.InstallationId == other?.InstallationId && this.InstallationUrl == other?.InstallationUrl && this.Name == other?.Name;
    }

    public IReadOnlyList<string> SearchTokens
    {
        get
        {
            if (searchTokens is null)
            {
                searchTokens = BuildSearchTokens();
            }
            return searchTokens;
        }
        private set
        {
            searchTokens = value;
        }
    }
    IReadOnlyList<string>? searchTokens;

    static string GetMainDomain(string host)
    {
        var parts = host.Split('.');

        if (parts.Length >= 3)
        {
            return parts[parts.Length - 3];
        }
        else if (parts.Length >= 2)
        {
            return parts[parts.Length - 2];
        }

        return host;
    }

    private List<string> BuildSearchTokens()
    {
        var tokens = new List<string>();

        string installationUrlHost = InstallationUrl.Replace("https://", "").Replace("http://", "").Replace("/", "");
        string mainDomainName = GetMainDomain(installationUrlHost);
        tokens.Add(mainDomainName);

        foreach (var name in LocalizedName.Values)
        {
            tokens.Add(name);
            tokens.Add(FuzzyStringComparer.Normalize(name));
            tokens.Add(FuzzyStringComparer.GetAcronymStartingWithUpperOnly(name));
        }

        return tokens
            .Where(t => string.IsNullOrWhiteSpace(t) == false)
            .Distinct()
            .ToList();
    }
}
