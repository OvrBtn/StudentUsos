using StudentUsos.Controls;
using StudentUsos.Features.CampusMap.Services;

namespace StudentUsos.Features.CampusMap.Views;

public partial class CampusMapPage : CustomContentPageNotAnimated
{

    ICampusMapService campusMapService;
    public CampusMapPage(ICampusMapService campusMapService)
    {
        InitializeComponent();

        this.campusMapService = campusMapService;

        Init();
    }

    void Init()
    {

    }

    //bool isViewModelSet = false;
    //protected override void OnNavigatedTo(NavigatedToEventArgs args)
    //{
    //    base.OnNavigatedTo(args);

    //    if (isViewModelSet)
    //    {
    //        return;
    //    }
    //    Dispatcher.Dispatch(() =>
    //    {
    //        isViewModelSet = true;
    //        viewModel.Init();
    //    });
    //}

    private void OnSendMessageButtonClicked(object sender, EventArgs e)
    {
        hybridWebView.SendRawMessage($"Hello from C#!");
    }

    private async void OnHybridWebViewRawMessageReceived(object sender, HybridWebViewRawMessageReceivedEventArgs e)
    {
        var t = 5;
        //await DisplayAlert("Raw Message Received", e.Message, "OK");
    }
}