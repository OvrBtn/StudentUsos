using Android.Views;
using Microsoft.Maui.Handlers;

namespace StudentUsos.Platforms.Android;

public class HybridWebViewCustomHandler : HybridWebViewHandler
{
    protected override global::Android.Webkit.WebView CreatePlatformView()
    {
        var platformView = base.CreatePlatformView();
        //platformView.SetLayerType(LayerType.Software, null);
        platformView.Settings.BuiltInZoomControls = true;
        platformView.Settings.DisplayZoomControls = false;
        platformView.Settings.SetSupportZoom(true);
        return platformView;
    }
}
