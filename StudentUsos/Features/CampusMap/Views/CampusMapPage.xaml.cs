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

        hybridWebView.SetInvokeJavaScriptTarget(this);

        _ = Init();
    }

    public List<CampusBuilding> Buildings
    {
        get => buildings;
        set
        {
            buildings = value;
            OnPropertyChanged();
        }
    }
    List<CampusBuilding> buildings = new();

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
    public string CurrentFloor { get; private set; } = "0";

    public string CurrentFullLocation
    {
        get
        {
            if (CurrentBuildingIndex != 0)
            {
                return $"{Buildings[CurrentBuildingIndex].Id} - {Buildings[CurrentBuildingIndex].LocalizedName}, {CurrentFloor}";
            }
            else
            {
                return Buildings[CurrentBuildingIndex].LocalizedName;
            }
        }
    }

    async Task Init()
    {
        await Task.Delay(3000);

        Buildings = await campusMapService.GetBuildingsDataDeserialized() ?? new();
        Buildings.Insert(0, new()
        {
            Id = LocalizedStrings.Campus,
            LocalizedName = LocalizedStrings.Campus,
            Floors = new()
        });
        Floors = Buildings[0].Floors;

        _ = ShowCampusMap();
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
        });
    }

    async Task UpdateWebView(string buildingId, string floor)
    {
        var svg = await campusMapService.GetFloorSvg(buildingId, floor) ?? string.Empty;

        var floorData = await campusMapService.GetFloorData(buildingId, floor) ?? string.Empty;
        FloorData = JsonSerializer.Deserialize(floorData, RoomInfoJsonContext.Default.ListRoomInfo) ?? new();

        _ = SendFloorSvgToHybridWebView(svg);
        _ = SendFloorDataToHybridWebView(floorData);

        OnPropertyChanged(nameof(CurrentFullLocation));
    }

    async Task ShowCampusMap()
    {
        var svg = await campusMapService.GetCampusMapSvg() ?? string.Empty;
        _ = SendFloorSvgToHybridWebView(svg);
        OnPropertyChanged(nameof(CurrentFullLocation));
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

    async Task SendFloorDataToHybridWebView(List<FloorData> floorData)
    {
        await hybridWebView.InvokeJavaScriptAsync<object>("ReceiveFloorData",
            HybridWebViewJsonContext.Default.Object,
            [JsonSerializer.Serialize(floorData)],
            [HybridWebViewJsonContext.Default.String]);
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

    public int CurrentBuildingIndex { get; private set; } = 0;

    private void BuildingButton_Clicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        string clickedText = button!.Text;
        int index = buildings.FindIndex(x => x.Id == clickedText);

        if (index == CurrentBuildingIndex)
        {
            return;
        }
        CurrentBuildingIndex = index;

        Floors = Buildings[CurrentBuildingIndex].Floors;

        CurrentBuildingId = clickedText;
        if (CurrentBuildingIndex == 0)
        {
            _ = ShowCampusMap();
        }
        else
        {
            CurrentFloor = "0";
            _ = UpdateWebView(CurrentBuildingId, CurrentFloor);
        }
    }

    private void FloorButton_Clicked(object sender, EventArgs e)
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
