using StudentUsos.Controls;
using StudentUsos.Features.CampusMap.Models;
using StudentUsos.Features.CampusMap.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudentUsos.Features.CampusMap.Views;

public partial class CampusMapPage : CustomContentPageNotAnimated
{

    ICampusMapService campusMapService;
    public CampusMapPage(ICampusMapService campusMapService)
    {
        InitializeComponent();

        this.campusMapService = campusMapService;

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

    async Task Init()
    {

        Buildings = await campusMapService.GetBuildingsDataDeserialized() ?? new();
        Floors = Buildings[0].Floors;

        await Task.Delay(500);

        var svg = await campusMapService.GetFloorSvg("A23", "0") ?? string.Empty;
        await SendFloorSvgToHybridWebView(svg);

        var floorData = await campusMapService.GetFloorData("A23", "0") ?? string.Empty;
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
