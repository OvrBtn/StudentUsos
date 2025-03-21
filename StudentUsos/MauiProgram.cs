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
using DrawnUi.Maui.Draw;
using Microsoft.Extensions.Logging;
#endif

namespace StudentUsos;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        AllowMultiLineTruncation();

        var builder = MauiApp.CreateBuilder();

#if ANDROID
        builder.Services.AddSingleton<INotificationBuilder, CustomNotificationBuilder>();
#endif

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
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
            .ConfigureStatusBarAndNavigationBarColorsForModalPages()
            .UseDrawnUi()
#endif
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });


#if DEBUG
        builder.Logging.AddDebug();
#endif

        //first use of SkiaSharp is taking significant amount of time of main thread, forcing first execution on another thread seems to make it better
        DashboardViewModel.FinishedSynchronousLoading += InitializeSkiaSharp;

#if ANDROID
        builder.Services.AddSingleton<INotificationBuilder, CustomNotificationBuilder>();
#endif

        builder.Services.AddSingleton<FirebasePushNotificationsService>();

        return builder.Build();
    }

    static void InitializeSkiaSharp()
    {
        Task.Run(() =>
        {
            SKCanvas skCanvas = new(new SKBitmap());
            skCanvas.Dispose();
        });
    }

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