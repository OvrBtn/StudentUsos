using System.Text.Json.Serialization;

namespace StudentUsos.Features.CampusMap.Views;

public partial class CampusMapPage : ContentPage
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
            _ = viewModel.InitAsync();
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


    bool isWebViewIntialized = false;
    public async Task SendCampusSvgToHybridWebView(string campusSvg)
    {
        //as for .net 9 (9.0.110) webview is initialized after the page and not with it
        //this causes issues with sending first svg as soon as the page is loaded hence this workaround
        //TODO: refactor to use new webview's events introduced in .NET 10
        if (isWebViewIntialized == false)
        {
            string? result;
            do
            {
                result = await hybridWebView.EvaluateJavaScriptAsync("window.HybridWebView != null");
                await Task.Delay(50);
            }
            while (result != "true");
            isWebViewIntialized = true;
        }

        await hybridWebView.InvokeJavaScriptAsync<object>("ReceiveCampusSvg",
            HybridWebViewJsonContext.Default.Object,
            [campusSvg],
            [HybridWebViewJsonContext.Default.String]);
    }
}

[JsonSerializable(typeof(object)), JsonSerializable(typeof(string))]
partial class HybridWebViewJsonContext : JsonSerializerContext
{ }
