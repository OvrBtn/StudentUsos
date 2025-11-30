using System.Globalization;
using System.Text;
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
            tokens.Add(Normalize(name));
            tokens.Add(GetAcronymStartingWithUpperOnly(name));
        }

        return tokens
            .Where(t => string.IsNullOrWhiteSpace(t) == false)
            .Distinct()
            .ToList();
    }

    static string Normalize(string text)
    {
        return new string(text
            .ToLowerInvariant()
            .Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .ToArray());
    }

    static string GetAcronym(string name)
    {
        return string.Join("", name
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(w => char.ToUpper(w[0])));
    }

    private static string GetAcronymStartingWithUpperOnly(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "";

        var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var acronymLetters = words
            .Where(w => char.IsUpper(w[0]))
            .Select(w => w[0]);

        return string.Concat(acronymLetters);
    }


    public static IEnumerable<UsosInstallation> FuzzySearch(IEnumerable<UsosInstallation> installations, string query, double threshold = 0.45)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return installations;
        }

        string normalizedQuery = Normalize(query);

        return installations
            .Select(i => new
            {
                Installation = i,
                Score = i.SearchTokens
                    .Max(token =>
                    {
                        string normalizedToken = Normalize(token);

                        if (normalizedToken.Equals(normalizedQuery))
                            return 1.2;

                        if (normalizedToken.StartsWith(normalizedQuery))
                            return 1.1;

                        if (normalizedToken.Contains(normalizedQuery))
                            return 1.0;

                        return FuzzyStringComparer.SimilarityScore(normalizedQuery, normalizedToken);
                    })
            })
            .Where(x => x.Score >= threshold)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Installation.Name)
            .Select(x => x.Installation);
    }
}
