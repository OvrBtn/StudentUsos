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

    public string CurrentBuildingId { get; private set; } = "A23";
    public string CurrentFloor { get; private set; } = "0";

    async Task Init()
    {

        Buildings = await campusMapService.GetBuildingsDataDeserialized() ?? new();
        Floors = Buildings[0].Floors;

        await Task.Delay(3000);

        var svg = await campusMapService.GetFloorSvg(CurrentBuildingId, CurrentFloor) ?? string.Empty;
        await SendFloorSvgToHybridWebView(svg);

        var floorData = await campusMapService.GetFloorData(CurrentBuildingId, CurrentFloor) ?? string.Empty;
        try
        {
            FloorData = JsonSerializer.Deserialize(floorData, RoomInfoJsonContext.Default.ListRoomInfo) ?? new();
        }
        catch (Exception e)
        {
            var t = 5;
        }

        await SendFloorDataToHybridWebView(floorData);
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

    private void OnSendMessageButtonClicked(object sender, EventArgs e)
    {
        hybridWebView.SendRawMessage($"");
    }

    private async void OnHybridWebViewRawMessageReceived(object sender, HybridWebViewRawMessageReceivedEventArgs e)
    {

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
}

[JsonSerializable(typeof(object)), JsonSerializable(typeof(string))]
partial class HybridWebViewJsonContext : JsonSerializerContext
{ }
