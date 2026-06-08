using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentUsos.Features.CampusMap.Models;
using StudentUsos.Features.CampusMap.Repositories;
using StudentUsos.Features.CampusMap.Services;

namespace StudentUsos.Features.CampusMap.Views;

public partial class SearchViewModel : BaseViewModel, INavigableWithResult<RoomInfo>
{
    ICampusMapRepository campusMapRepository;
    ICampusMapService campusMapService;
    INavigationService navigationService;
    public SearchViewModel(ICampusMapRepository campusMapRepository, ICampusMapService campusMapService, INavigationService navigationService)
    {
        this.campusMapRepository = campusMapRepository;
        this.campusMapService = campusMapService;
        this.navigationService = navigationService;
    }

    public TaskCompletionSource<RoomInfo?> TaskCompletionSource { get; set; } = new();

    [ObservableProperty]
    string mainStateKey = StateKey.Loading;

    [ObservableProperty]
    List<RoomInfo> rooms = new();
    List<RoomInfo> originalRooms = new();

    public async Task InitAsync()
    {
        var allRoomsLocal = campusMapRepository.GetAllRooms();
        if (allRoomsLocal.Count > 0)
        {
            originalRooms = allRoomsLocal;
            Rooms = allRoomsLocal;
            MainStateKey = StateKey.Loaded;
        }

        var allRoomsRemote = await campusMapService.GetAllRoomsAsync();
        if (allRoomsRemote != null && allRoomsRemote.Count != 0)
        {
            originalRooms = allRoomsRemote;
            Rooms = allRoomsRemote;
            MainStateKey = StateKey.Loaded;

            campusMapRepository.SaveAllRooms(allRoomsRemote);
        }

        if (originalRooms.Count == 0)
        {
            MainStateKey = StateKey.ConnectionError;
        }
    }

    [RelayCommand]
    async Task RoomClicked(RoomInfo room)
    {
        TaskCompletionSource.SetResult(room);
        await navigationService.PopAsync();
    }

    [ObservableProperty]
    string editorText = string.Empty;

    [RelayCommand]
    void TextChanged()
    {
        Rooms = FuzzySearch(originalRooms, EditorText).ToList();
    }

    public static IEnumerable<RoomInfo> FuzzySearch(IEnumerable<RoomInfo> rooms, string query, double threshold = 0.45)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return rooms;
        }

        string normalizedQuery = FuzzyStringComparer.Normalize(query);

        return rooms
            .Select(i => new
            {
                Installation = i,
                Score = i.SearchTokens
                    .Max(token =>
                    {
                        string normalizedToken = FuzzyStringComparer.Normalize(token);

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

    public void Cancel()
    {
        TaskCompletionSource.SetCanceled();
    }
}
