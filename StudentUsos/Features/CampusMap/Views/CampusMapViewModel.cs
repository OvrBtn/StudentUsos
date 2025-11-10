using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentUsos.Features.CampusMap.Models;
using StudentUsos.Features.CampusMap.Repositories;
using StudentUsos.Features.CampusMap.Services;
using StudentUsos.Features.UserInfo;
using StudentUsos.Resources.LocalizedStrings;
using System.Text.Json;

namespace StudentUsos.Features.CampusMap.Views;

public partial class CampusMapViewModel : BaseViewModel
{
    ICampusMapService campusMapService;
    IUserInfoRepository userInfoRepository;
    IApplicationService applicationService;
    ICampusMapRepository campusMapRepository;
    ILocalStorageManager localStorageManager;
    public CampusMapViewModel(ICampusMapService campusMapService,
        IUserInfoRepository userInfoRepository,
        IApplicationService applicationService,
        ICampusMapRepository campusMapRepository,
        ILocalStorageManager localStorageManager)
    {
        this.campusMapService = campusMapService;
        this.userInfoRepository = userInfoRepository;
        this.applicationService = applicationService;
        this.campusMapRepository = campusMapRepository;
        this.localStorageManager = localStorageManager;
    }

    //it might not be very MVVM but HybridWebView doesn't really support bindings and it has to be directly referenced
    public CampusMapPage CampusMapPage { get; private set; }

    public void SetCampusMapPage(CampusMapPage page)
    {
        CampusMapPage = page;
    }

    void UpdateBuildingsList(List<CampusBuilding> buildings)
    {
        var buildingWithMaps = buildings.Where(x => x.FloorsList is not null && x.FloorsList.Count > 0).ToList();

        CampusBuilding campus = new()
        {
            Id = LocalizedStrings.Campus,
            LocalizedName = LocalizedStrings.Campus,
            FloorsList = new()
        };

        buildings.Insert(0, campus);
        buildingWithMaps.Insert(0, campus);

        Buildings = buildings;
        BuildingsWithMaps = buildingWithMaps;
    }

    async Task InitWithLocalDataAsync()
    {
        var buildings = campusMapRepository.GetBuildingsData();
        if (buildings.Count == 0)
        {
            return;
        }

        UpdateBuildingsList(buildings);
        MainStateKey = StateKey.Loaded;

        await ShowCampusMapAsync();
    }

    async Task InitWithRemoteDataAsync()
    {
        var buildingsFromApi = await campusMapService.GetBuildingsDataDeserializedAsync();
        if (buildingsFromApi is null)
        {
            if (MainStateKey != StateKey.Loaded)
            {
                MainStateKey = StateKey.ConnectionError;
            }
            return;
        }

        UpdateBuildingsList(buildingsFromApi);
        MainStateKey = StateKey.Loaded;

        if (WebViewStateKey != StateKey.Loaded)
        {
            await ShowCampusMapAsync();
        }

        campusMapRepository.SaveBuildingsData(buildingsFromApi);
    }

    async Task InitializeLocallySavedUsersVotesAsync()
    {
        if (localStorageManager.TryGettingString(LocalStorageKeys.IsCampusMapInitialized, out string isCampusMapInitialized) &&
            bool.TryParse(isCampusMapInitialized, out bool parsed) && parsed)
        {
            return;
        }

        var upvotes = await campusMapService.FetchIdsOfUsersUpvotesAsync();
        var downvotes = await campusMapService.FetchIdsOfUsersDownvotesAsync();
        if (upvotes is null || downvotes is null)
        {
            return;
        }

        campusMapRepository.ImportUpvotes(upvotes);
        campusMapRepository.ImportDownvotes(downvotes);

        localStorageManager.SetString(LocalStorageKeys.IsCampusMapInitialized, true.ToString());
    }

    public async Task InitAsync()
    {
        _ = InitializeLocallySavedUsersVotesAsync();
        await InitWithLocalDataAsync();
        await InitWithRemoteDataAsync();
    }

    [ObservableProperty] string mainStateKey = StateKey.Loading;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsWebViewVisibile))] string webViewStateKey = StateKey.Loading;

    public bool IsWebViewVisibile { get => WebViewStateKey == StateKey.Loaded; }

    public List<CampusBuilding> Buildings { get; set; }

    [ObservableProperty] List<CampusBuilding> buildingsWithMaps = new();
    [ObservableProperty] List<string> floors = new();

    public List<RoomInfo> FloorData { get; private set; }

    public string CurrentBuildingId { get; private set; } = LocalizedStrings.Campus;
    [ObservableProperty] string currentFloor = "0";

    public string CurrentFullLocation
    {
        get
        {
            if (Buildings is null || Buildings.Count == 0)
            {
                return string.Empty;
            }
            if (CurrentBuildingIndex != 0)
            {
                return $"{Buildings[CurrentBuildingIndex].Id} - {Buildings[CurrentBuildingIndex].LocalizedName}";
            }
            else
            {
                return Buildings[CurrentBuildingIndex].LocalizedName;
            }
        }
    }

    async Task<(string? floorSvgLocal, string? floorDataLocal)> UpdateWebViewLocalDataAsync(string buildingId, string floor)
    {
        var floorSvgLocal = campusMapRepository.GetFloorMap(buildingId, floor)?.FloorSvg;
        var floorDataLocal = campusMapRepository.GetFloorData(buildingId, floor);
        string? floorDataLocalSerialized = null;

        if (floorSvgLocal is not null && floorDataLocal.Count > 0)
        {
            await CampusMapPage.SendFloorSvgToHybridWebView(floorSvgLocal);
            floorDataLocalSerialized = JsonSerializer.Serialize(floorDataLocal, RoomInfoJsonContext.Default.ListRoomInfo);
            FloorData = floorDataLocal;
            await CampusMapPage.SendFloorDataToHybridWebView(floorDataLocalSerialized);
            WebViewStateKey = StateKey.Loaded;
        }

        return new(floorSvgLocal, floorDataLocalSerialized);
    }

    async Task UpdateWebViewRemoteDataAsync(string buildingId, string floor, string? floorSvgLocal, string? floorDataLocal)
    {
        var floorSvgRemote = await campusMapService.GetFloorSvgAsync(buildingId, floor);

        if (buildingId != CurrentBuildingId || floor != CurrentFloor)
        {
            return;
        }

        if (floorSvgRemote is null)
        {
            if (floorSvgLocal is null)
            {
                WebViewStateKey = StateKey.ConnectionError;
            }
            return;
        }

        if (floorSvgLocal != floorSvgRemote)
        {
            campusMapRepository.SaveFloorMap(buildingId, floor, floorSvgRemote);
            await CampusMapPage.SendFloorSvgToHybridWebView(floorSvgRemote);
        }

        var floorDataRemote = await campusMapService.GetFloorDataAsync(buildingId, floor);

        if (buildingId != CurrentBuildingId || floor != CurrentFloor)
        {
            return;
        }

        if (floorDataRemote is null)
        {
            if (WebViewStateKey != StateKey.Loaded)
            {
                WebViewStateKey = StateKey.ConnectionError;
            }
            return;
        }

        var floorDataRemoteDeserialized = JsonSerializer.Deserialize(floorDataRemote, RoomInfoJsonContext.Default.ListRoomInfo);
        if (floorDataRemoteDeserialized is null)
        {
            if (WebViewStateKey != StateKey.Loaded)
            {
                WebViewStateKey = StateKey.LoadingError;
            }
            return;
        }

        if (floorDataLocal != floorDataRemote)
        {
            FloorData = floorDataRemoteDeserialized;
            campusMapRepository.SaveFloorData(buildingId, floor, floorDataRemoteDeserialized);
            await CampusMapPage.SendFloorDataToHybridWebView(floorDataRemote);
        }

        WebViewStateKey = StateKey.Loaded;
    }

    async Task UpdateWebViewAsync(string buildingId, string floor)
    {
        WebViewStateKey = StateKey.Loading;

        var localData = await UpdateWebViewLocalDataAsync(buildingId, floor);
        await UpdateWebViewRemoteDataAsync(buildingId, floor, localData.floorSvgLocal, localData.floorDataLocal);
    }

    async Task ShowCampusMapAsync()
    {
        string currentBuildingId = CurrentBuildingId;
        WebViewStateKey = StateKey.Loading;

        var campusMapLocal = campusMapRepository.GetCampusMap();
        if (campusMapLocal is not null)
        {
            await CampusMapPage.SendCampusSvgToHybridWebView(campusMapLocal);
            WebViewStateKey = StateKey.Loaded;
        }

        var campusMapRemote = await campusMapService.GetCampusMapSvgAsync();

        if (currentBuildingId != CurrentBuildingId)
        {
            return;
        }

        if (campusMapRemote is null)
        {
            if (WebViewStateKey != StateKey.Loaded)
            {
                WebViewStateKey = StateKey.ConnectionError;
            }
            return;
        }

        if (campusMapLocal != campusMapRemote)
        {
            await CampusMapPage.SendCampusSvgToHybridWebView(campusMapRemote);
            campusMapRepository.SaveCampusMap(campusMapRemote);
        }
        WebViewStateKey = StateKey.Loaded;
    }

    public void ReceiveRoomClicked(string roomId)
    {
        int roomIdParsed = int.Parse(roomId);

        var foundRoomInfos = FloorData.Where(x => x.RoomId == roomIdParsed).ToList();
        foundRoomInfos = foundRoomInfos.OrderByDescending(x => x.NameWeight).ToList();

        RoomInfo? primaryRoomInfo = null;
        if (foundRoomInfos.Count > 0)
        {
            primaryRoomInfo = foundRoomInfos[0];
        }
        List<RoomInfo> addtionalRoomInfos = new();
        if (foundRoomInfos.Count > 1)
        {
            addtionalRoomInfos = foundRoomInfos.GetRange(1, foundRoomInfos.Count - 1);
        }

        RoomDetailsPage.CreateAndShow(new()
        {
            RoomName = primaryRoomInfo?.Name ?? string.Empty,
            FullRoomInfos = foundRoomInfos.ToList(),
            AdditionalRoomNames = addtionalRoomInfos.Select(x => x.Name).ToList(),
            ConfirmAction = new((suggestedName) => _ = SendSuggestionAsync(suggestedName, roomId))
        });
    }

    public void ReceiveCampusBuildingClicked(string buildingId)
    {
        var buildingDetails = Buildings.FirstOrDefault(x => x.Id == buildingId);
        if (buildingDetails is null)
        {
            return;
        }

        CampusBuildingDetailsPage.CreateAndShow(new()
        {
            ShortName = buildingDetails.Id,
            LongName = buildingDetails.LocalizedName,
            GoToBuildingMap = new(() =>
            {
                ShowDefaultFloor(buildingDetails);
            })
        });
    }

    UserInfo.UserInfo? currentUser = null;
    async Task SendSuggestionAsync(string suggestedName, string roomId)
    {
        if (currentUser is null)
        {
            currentUser = userInfoRepository.GetUserInfo();
        }

        var responseCode = await campusMapService.SendUserSuggestionAsync(suggestedName, CurrentBuildingId, CurrentFloor, roomId, currentUser!.StudentNumber);

        if (responseCode == System.Net.HttpStatusCode.OK)
        {
            applicationService.ShowToast(LocalizedStrings.CampusMapPage_SuggestionSentSuccessfully);
        }
        else if (responseCode == System.Net.HttpStatusCode.Forbidden)
        {
            applicationService.ShowToast(LocalizedStrings.CampusMapPage_SuggestionAlreadyRecorder);
        }
        else
        {
            applicationService.ShowToast(LocalizedStrings.CampusMapPage_SuggestionUnexpectedStatusCode);
        }
    }

    public int CurrentBuildingIndex { get; private set; } = 0;

    [RelayCommand]
    void BuildingButtonClicked()
    {
        PickFromListPopup.CreateAndShow(LocalizedStrings.Buildings,
            BuildingsWithMaps.Select(x => $"{x.Id} - {x.LocalizedName}"),
            onPicked: (pickedItem) =>
            {
                ShowDefaultFloor(BuildingsWithMaps[pickedItem.Index]);
            });
    }

    void ShowDefaultFloor(CampusBuilding building)
    {
        int index = Buildings.IndexOf(building);

        if (index == CurrentBuildingIndex)
        {
            return;
        }

        CurrentBuildingIndex = index;

        Floors = Buildings[CurrentBuildingIndex].FloorsList;
        CurrentFloor = "0";

        CurrentBuildingId = Buildings[CurrentBuildingIndex].Id;
        if (CurrentBuildingIndex == 0)
        {
            _ = ShowCampusMapAsync();
        }
        else
        {
            if (Floors.Contains("0"))
            {
                CurrentFloor = "0";
            }
            else
            {
                CurrentFloor = Floors[0];
            }
            _ = UpdateWebViewAsync(CurrentBuildingId, CurrentFloor);
        }
        OnPropertyChanged(nameof(CurrentFullLocation));
    }

    [RelayCommand]
    void FloorButtonClicked()
    {
        PickFromListPopup.CreateAndShow(LocalizedStrings.Floors, Floors,
            onPicked: (pickedItem) =>
            {
                if (pickedItem.Value == CurrentFloor)
                {
                    return;
                }

                CurrentFloor = pickedItem.Value;
                _ = UpdateWebViewAsync(CurrentBuildingId, CurrentFloor);
                OnPropertyChanged(nameof(CurrentFullLocation));
            });
    }
}
