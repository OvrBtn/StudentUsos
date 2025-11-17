#if IOS
using UIKit;
#endif

using CommunityToolkit.Maui;
using CustomCalendar;
using CustomSchedule;
using DevExpress.Maui;
using Microsoft.Maui.Handlers;
using Plugin.FirebasePushNotifications;
using Plugin.FirebasePushNotifications.Platforms;
using Plugin.LocalNotification;
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls.Hosting;
using StudentUsos.Features.Dashboard.Views;
using Microsoft.Extensions.Logging;
#if ANDROID
using Android.Text;
using StudentUsos.Platforms.Android;
using Microsoft.Maui.LifecycleEvents;
#endif

namespace StudentUsos;

public static class MauiProgram
{
    static TaskCompletionSource BuildingTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public static Task BuildingTask = BuildingTaskCompletionSource.Task;

    public static MauiApp CreateMauiApp()
    {
        AllowMultiLineTruncation();

        var builder = MauiApp.CreateBuilder();

#if ANDROID
        builder.Services.AddSingleton<INotificationBuilder, CustomNotificationBuilder>();
#endif

#pragma warning disable CA1416
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
#pragma warning restore
            .UseSkiaSharp()
            .UseCustomCalendar()
            .UseCustomSchedule()
            .UseLocalNotification()
            .UseDevExpress()
            .UseFirebasePushNotifications()
            .RegisterViews()
            .RegisterViewModels()
            .RegisterServices()
#if ANDROID
            .ConfigureLifecycleEvents(events =>
            {
                events.AddAndroid(android => android.OnCreate((activity, bundle) => MakeStatusBarTranslucent(activity)));
            })
            .ConfigureStatusBarAndNavigationBarColorsForModalPages()
#endif
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });


#if DEBUG
        builder.Services.AddHybridWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        //first use of SkiaSharp is taking significant amount of time of main thread, forcing first execution on another thread seems to make it better
        DashboardViewModel.FinishedSynchronousLoading += InitializeSkiaSharp;

#if ANDROID
        builder.Services.AddSingleton<INotificationBuilder, CustomNotificationBuilder>();

        WebViewHandler.Mapper.AppendToMapping("zoom", (handler, view) =>
        {
            handler.PlatformView.Settings.BuiltInZoomControls = true;
            handler.PlatformView.Settings.DisplayZoomControls = true;
        });

        builder.ConfigureMauiHandlers(handlers =>
        {
            handlers.AddHandler<HybridWebView, HybridWebViewCustomHandler>();
        });
#endif

        builder.Services.AddSingleton<FirebasePushNotificationsService>();

        var app = builder.Build();

        BuildingTaskCompletionSource.TrySetResult();

        return app;
    }

    static void InitializeSkiaSharp()
    {
        Task.Run(() =>
        {
            SKCanvas skCanvas = new(new SKBitmap());
            skCanvas.Dispose();
        });
    }

#if ANDROID
    static void MakeStatusBarTranslucent(Android.App.Activity activity)
    {
        //necessary for APIs 35+
        AndroidX.Core.View.ViewCompat.SetOnApplyWindowInsetsListener(activity.Window?.DecorView, null);
        AndroidX.Core.View.WindowCompat.SetDecorFitsSystemWindows(activity.Window, false);

        activity.Window?.SetFlags(Android.Views.WindowManagerFlags.LayoutNoLimits, Android.Views.WindowManagerFlags.LayoutNoLimits);
        activity.Window?.ClearFlags(Android.Views.WindowManagerFlags.TranslucentStatus);
        activity.Window?.SetStatusBarColor(Android.Graphics.Color.Transparent);
    }
#endif

    //workaround for Label's properties MaxLines and LineBreakMode not working together, from https://github.com/dotnet/maui/discussions/5492
    static void AllowMultiLineTruncation()
    {
        static void UpdateMaxLines(ILabelHandler handler, ILabel label)
        {
#if ANDROID
            var textView = handler.PlatformView;
            if (label is Label controlsLabel && textView.Ellipsize == TextUtils.TruncateAt.End && controlsLabel.MaxLines != -1)
            {
                textView.SetMaxLines(controlsLabel.MaxLines);
            }
#elif IOS
                var textView = handler.PlatformView;
                if (label is Label controlsLabel && textView.LineBreakMode == UILineBreakMode.TailTruncation && controlsLabel.MaxLines != -1)
                {
                    textView.Lines = controlsLabel.MaxLines;
                }
#endif
        }
        ;

        LabelHandler.Mapper.AppendToMapping(
            nameof(Label.LineBreakMode), UpdateMaxLines);

        LabelHandler.Mapper.AppendToMapping(
            nameof(Label.MaxLines), UpdateMaxLines);
    }
}