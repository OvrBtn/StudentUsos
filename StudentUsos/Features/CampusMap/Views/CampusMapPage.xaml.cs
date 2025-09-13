using StudentUsos.Controls;
using System.Text.Json.Serialization;

namespace StudentUsos.Features.CampusMap.Views;

public partial class CampusMapPage : CustomContentPageNotAnimated
{
    CampusMapViewModel viewModel;
    public CampusMapPage(CampusMapViewModel viewModel)
    {
        InitializeComponent();

        this.viewModel = viewModel;
        viewModel.SetCampusMapPage(this);
        BindingContext = viewModel;

        hybridWebView.SetInvokeJavaScriptTarget(this);
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
            _ = viewModel.Init();
        });
    }

    public void ReceiveRoomClicked(string roomId)
    {
        viewModel.ReceiveRoomClicked(roomId);
    }

    public void ReceiveCampusBuildingClicked(string buildingId)
    {
        viewModel.ReceiveCampusBuildingClicked(buildingId);
    }

    public async Task SendFloorDataToHybridWebView(string floorData)
    {
        await hybridWebView.InvokeJavaScriptAsync<object>("ReceiveFloorData",
            HybridWebViewJsonContext.Default.Object,
            [floorData],
            [HybridWebViewJsonContext.Default.String]);
    }

    public async Task SendFloorSvgToHybridWebView(string floorSvg)
    {
        await hybridWebView.InvokeJavaScriptAsync<object>("ReceiveFloorSvg",
            HybridWebViewJsonContext.Default.Object,
            [floorSvg],
            [HybridWebViewJsonContext.Default.String]);
    }

    public async Task SendCampusSvgToHybridWebView(string campusSvg)
    {
        await hybridWebView.InvokeJavaScriptAsync<object>("ReceiveCampusSvg",
            HybridWebViewJsonContext.Default.Object,
            [campusSvg],
            [HybridWebViewJsonContext.Default.String]);
    }


}

[JsonSerializable(typeof(object)), JsonSerializable(typeof(string))]
partial class HybridWebViewJsonContext : JsonSerializerContext
{ }
