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

    public CampusMapViewModel(ICampusMapService campusMapService,
        IUserInfoRepository userInfoRepository,
        IApplicationService applicationService,
        ICampusMapRepository campusMapRepository)
    {
        this.campusMapService = campusMapService;
        this.userInfoRepository = userInfoRepository;
        this.applicationService = applicationService;
        this.campusMapRepository = campusMapRepository;
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

    async Task InitWithLocalData()
    {
        var buildings = campusMapRepository.GetBuildingsData();
        if (buildings.Count == 0)
        {
            return;
        }

        UpdateBuildingsList(buildings);
        MainStateKey = StateKey.Loaded;

        await ShowCampusMap();
        WebViewStateKey = StateKey.Loaded;
    }

    async Task InitWithRemoteData()
    {
        var buildingsFromApi = await campusMapService.GetBuildingsDataDeserialized();
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
            await ShowCampusMap();
        }
        WebViewStateKey = StateKey.Loaded;

        campusMapRepository.SaveBuildingsData(buildingsFromApi);
    }

    public async Task Init()
    {
        await InitWithLocalData();
        await InitWithRemoteData();
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

    async Task<(string? floorSvgLocal, string? floorDataLocal)> UpdateWebViewLocalData(string buildingId, string floor)
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

    async Task UpdateWebViewRemoteData(string buildingId, string floor, string? floorSvgLocal, string? floorDataLocal)
    {
        var floorSvgRemote = await campusMapService.GetFloorSvg(buildingId, floor);

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
            _ = CampusMapPage.SendFloorSvgToHybridWebView(floorSvgRemote);
        }

        var floorDataRemote = await campusMapService.GetFloorData(buildingId, floor);

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
            _ = CampusMapPage.SendFloorDataToHybridWebView(floorDataRemote);
        }

        WebViewStateKey = StateKey.Loaded;
    }

    async Task UpdateWebView(string buildingId, string floor)
    {
        WebViewStateKey = StateKey.Loading;

        var localData = await UpdateWebViewLocalData(buildingId, floor);
        await UpdateWebViewRemoteData(buildingId, floor, localData.floorSvgLocal, localData.floorDataLocal);
    }

    async Task ShowCampusMap()
    {
        string currentBuildingId = CurrentBuildingId;
        WebViewStateKey = StateKey.Loading;

        var campusMapLocal = campusMapRepository.GetCampusMap();
        if (campusMapLocal is not null)
        {
            await CampusMapPage.SendCampusSvgToHybridWebView(campusMapLocal);
            WebViewStateKey = StateKey.Loaded;
        }

        var campusMapRemote = await campusMapService.GetCampusMapSvg();

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
            _ = CampusMapPage.SendCampusSvgToHybridWebView(campusMapRemote);
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
            ConfirmAction = new((suggestedName) => _ = SendSuggestion(suggestedName, roomId))
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
            LongName = buildingDetails.LocalizedName
        });
    }

    UserInfo.UserInfo? currentUser = null;
    async Task SendSuggestion(string suggestedName, string roomId)
    {
        if (currentUser is null)
        {
            currentUser = userInfoRepository.GetUserInfo();
        }

        var responseCode = await campusMapService.SendUserSuggestion(suggestedName, CurrentBuildingId, CurrentFloor, roomId, currentUser!.StudentNumber);

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
                int index = Buildings.IndexOf(BuildingsWithMaps[pickedItem.Index]);

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
                    _ = ShowCampusMap();
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
                    _ = UpdateWebView(CurrentBuildingId, CurrentFloor);
                }
                OnPropertyChanged(nameof(CurrentFullLocation));
            });
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
                _ = UpdateWebView(CurrentBuildingId, CurrentFloor);
                OnPropertyChanged(nameof(CurrentFullLocation));
            });
    }
}
