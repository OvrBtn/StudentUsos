using SQLite;
using StudentUsos.Resources.LocalizedStrings;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace StudentUsos.Features.CampusMap.Models;


[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(List<RoomInfo>))]
public partial class RoomInfoJsonContext : JsonSerializerContext
{ }

public class RoomInfo : INotifyPropertyChanged
{
    public int InternalId { get; set; }
    public int RoomId { get; set; }
    public string BuildingId { get; set; }
    public string Floor { get; set; }
    public string Name { get; set; }
    public int NameWeight { get; set; }

    public string NameWithTitle => $"{LocalizedStrings.SearchPage_Room} - {Name}";
    public string BuildingWithTitle => $"{LocalizedStrings.SearchPage_Building} - {BuildingId}";
    public string FloorWithTitle => $"{LocalizedStrings.SearchPage_Floor} - {Floor}";

    public event PropertyChangedEventHandler? PropertyChanged;

    [Ignore]
    public bool IsUpvoted
    {
        get => isUpvoted;
        set
        {
            isUpvoted = value;
            PropertyChanged?.Invoke(this, new(nameof(IsUpvoted)));
        }
    }
    bool isUpvoted;

    [Ignore]
    public bool IsDownvoted
    {
        get => isDownvoted;
        set
        {
            isDownvoted = value;
            PropertyChanged?.Invoke(this, new(nameof(IsDownvoted)));
        }
    }
    bool isDownvoted;

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

    private List<string> BuildSearchTokens()
    {
        var tokens = new List<string>();

        tokens.Add(Name);
        tokens.Add(Name.Replace(".", ""));
        tokens.Add(FuzzyStringComparer.Normalize(Name));

        return tokens
            .Where(t => string.IsNullOrWhiteSpace(t) == false)
            .Distinct()
            .ToList();
    }

}
