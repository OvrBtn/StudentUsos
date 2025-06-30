using StudentUsos.Controls;
using StudentUsos.Features.CampusMap.Models;
using StudentUsos.Features.CampusMap.Services;
using StudentUsos.Features.UserInfo;
using StudentUsos.Resources.LocalizedStrings;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudentUsos.Features.CampusMap.Views;

public partial class CampusMapPage : CustomContentPageNotAnimated
{

    ICampusMapService campusMapService;
    IUserInfoRepository userInfoRepository;
    IApplicationService applicationService;
    public CampusMapPage(ICampusMapService campusMapService, IUserInfoRepository userInfoRepository, IApplicationService applicationService)
    {
        InitializeComponent();

        this.campusMapService = campusMapService;
        this.userInfoRepository = userInfoRepository;
        this.applicationService = applicationService;

        BindingContext = this;

        hybridWebView.SetInvokeJavaScriptTarget(this);
    }

    public List<CampusBuilding> Buildings { get; set; }

    public List<CampusBuilding> BuildingsWithMaps
    {
        get => buildingsWithMaps;
        set
        {
            buildingsWithMaps = value;
            OnPropertyChanged();
        }
    }
    List<CampusBuilding> buildingsWithMaps = new();

    public List<string> Floors
    {
        get => floors;
        set
        {
            floors = value;
            OnPropertyChanged();
        }
    }
    List<string> floors = new();

    public List<RoomInfo> FloorData { get; private set; }

    public string CurrentBuildingId { get; private set; } = LocalizedStrings.Campus;
    public string CurrentFloor
    {
        get
        {
            return currentFloor;
        }
        private set
        {
            currentFloor = value;
            OnPropertyChanged();
        }
    }
    string currentFloor = "";

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

    async Task Init()
    {
        var buildingsFromApi = await campusMapService.GetBuildingsDataDeserialized() ?? new();
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

        _ = ShowCampusMap();

        CurrentFloor = "0";
    }

    bool isInitialized = false;
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (isInitialized)
        {
            return;
        }
        Dispatcher.Dispatch(() =>
        {
            isInitialized = true;
            _ = Init();
        });
    }

    async Task UpdateWebView(string buildingId, string floor)
    {
        OnPropertyChanged(nameof(CurrentFullLocation));

        var svg = await campusMapService.GetFloorSvg(buildingId, floor) ?? string.Empty;

        var floorData = await campusMapService.GetFloorData(buildingId, floor) ?? string.Empty;
        FloorData = JsonSerializer.Deserialize(floorData, RoomInfoJsonContext.Default.ListRoomInfo) ?? new();

        _ = SendFloorSvgToHybridWebView(svg);
        _ = SendFloorDataToHybridWebView(floorData);
    }

    async Task ShowCampusMap()
    {
        OnPropertyChanged(nameof(CurrentFullLocation));
        var svg = await campusMapService.GetCampusMapSvg() ?? string.Empty;
        _ = SendCampusSvgToHybridWebView(svg);
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

    async Task SendFloorDataToHybridWebView(string floorData)
    {
        await hybridWebView.InvokeJavaScriptAsync<object>("ReceiveFloorData",
            HybridWebViewJsonContext.Default.Object,
            [floorData],
            [HybridWebViewJsonContext.Default.String]);
    }

    async Task SendFloorSvgToHybridWebView(string floorSvg)
    {
        await hybridWebView.InvokeJavaScriptAsync<object>("ReceiveFloorSvg",
            HybridWebViewJsonContext.Default.Object,
            [floorSvg],
            [HybridWebViewJsonContext.Default.String]);
    }

    async Task SendCampusSvgToHybridWebView(string camusSvg)
    {
        await hybridWebView.InvokeJavaScriptAsync<object>("ReceiveCampusSvg",
            HybridWebViewJsonContext.Default.Object,
            [camusSvg],
            [HybridWebViewJsonContext.Default.String]);
    }

    public int CurrentBuildingIndex { get; private set; } = 0;

    private void BuildingButton_Clicked(object sender, EventArgs e)
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
                CurrentFloor = "0";
                _ = UpdateWebView(CurrentBuildingId, CurrentFloor);
            }
        });
    }

    private void FloorButton_Clicked(object sender, EventArgs e)
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

    private void FloorButton_Clicked2(object sender, EventArgs e)
    {
        var button = sender as Button;
        string clickedText = button!.Text;

        if (clickedText == CurrentFloor)
        {
            return;
        }

        CurrentFloor = clickedText;
        _ = UpdateWebView(CurrentBuildingId, CurrentFloor);
    }
}

[JsonSerializable(typeof(object)), JsonSerializable(typeof(string))]
partial class HybridWebViewJsonContext : JsonSerializerContext
{ }
