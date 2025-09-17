using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentUsos.Features.CampusMap.Models;
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

    public CampusMapViewModel(ICampusMapService campusMapService, IUserInfoRepository userInfoRepository, IApplicationService applicationService)
    {
        this.campusMapService = campusMapService;
        this.userInfoRepository = userInfoRepository;
        this.applicationService = applicationService;
    }

    //it might not be very MVVM but HybridWebView doesn't really support bindings and it has to be directly referenced
    public CampusMapPage CampusMapPage { get; private set; }

    public void SetCampusMapPage(CampusMapPage page)
    {
        CampusMapPage = page;
    }


    public async Task Init()
    {
        var buildingsFromApi = await campusMapService.GetBuildingsDataDeserialized();
        if (buildingsFromApi is null)
        {
            MainStateKey = StateKey.ConnectionError;
            return;
        }

        var buildingWithMaps = buildingsFromApi.Where(x => x.Floors is not null && x.Floors.Count > 0).ToList();

        CampusBuilding campus = new()
        {
            Id = LocalizedStrings.Campus,
            LocalizedName = LocalizedStrings.Campus,
            Floors = new()
        };

        buildingsFromApi.Insert(0, campus);
        buildingWithMaps.Insert(0, campus);

        Buildings = buildingsFromApi;
        BuildingsWithMaps = buildingWithMaps;
        CurrentFloor = "0";
        MainStateKey = StateKey.Loaded;

        await ShowCampusMap();
        WebViewStateKey = StateKey.Loaded;
    }

    [ObservableProperty] string mainStateKey = StateKey.Loading;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsWebViewVisibile))] string webViewStateKey = StateKey.Loading;

    public bool IsWebViewVisibile { get => WebViewStateKey == StateKey.Loaded; }

    public List<CampusBuilding> Buildings { get; set; }

    [ObservableProperty] List<CampusBuilding> buildingsWithMaps = new();
    [ObservableProperty] List<string> floors = new();

    public List<RoomInfo> FloorData { get; private set; }

    public string CurrentBuildingId { get; private set; } = LocalizedStrings.Campus;
    [ObservableProperty] string currentFloor = "";

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

    async Task UpdateWebView(string buildingId, string floor)
    {
        WebViewStateKey = StateKey.Loading;

        OnPropertyChanged(nameof(CurrentFullLocation));

        var svg = await campusMapService.GetFloorSvg(buildingId, floor);
        if (svg is null)
        {
            WebViewStateKey = StateKey.ConnectionError;
            return;
        }

        var floorData = await campusMapService.GetFloorData(buildingId, floor);
        if (floorData is null)
        {
            WebViewStateKey = StateKey.ConnectionError;
            return;
        }

        var floorDataDeserialized = JsonSerializer.Deserialize(floorData, RoomInfoJsonContext.Default.ListRoomInfo);
        if (floorDataDeserialized is null)
        {
            WebViewStateKey = StateKey.LoadingError;
            return;
        }
        FloorData = floorDataDeserialized;

        _ = CampusMapPage.SendFloorSvgToHybridWebView(svg);
        _ = CampusMapPage.SendFloorDataToHybridWebView(floorData);

        WebViewStateKey = StateKey.Loaded;
    }

    async Task ShowCampusMap()
    {
        WebViewStateKey = StateKey.Loading;
        OnPropertyChanged(nameof(CurrentFullLocation));
        var svg = await campusMapService.GetCampusMapSvg();
        if (svg is null)
        {
            WebViewStateKey = StateKey.ConnectionError;
            return;
        }

        _ = CampusMapPage.SendCampusSvgToHybridWebView(svg);
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

                Floors = Buildings[CurrentBuildingIndex].Floors;
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
            });
    }
}
