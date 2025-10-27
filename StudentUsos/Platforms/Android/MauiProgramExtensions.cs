using Android.OS;
using Android.Views;
using AndroidX.Activity;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Platform;
using FragmentManager = AndroidX.Fragment.App.FragmentManager;

namespace StudentUsos;

internal static partial class MauiProgramExtensions
{
    /// <summary>
    /// Workaround for https://github.com/dotnet/maui/issues/28552
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static MauiAppBuilder ConfigureStatusBarAndNavigationBarColorsForModalPages(this MauiAppBuilder builder)
    {
        builder.ConfigureLifecycleEvents(a =>
        {
            a.AddAndroid(builder =>
            {
                builder.OnCreate((activity, bundle) =>
                {
                    if (activity is not ComponentActivity componentActivity)
                    {
                        return;
                    }
                    componentActivity.GetFragmentManager()?.RegisterFragmentLifecycleCallbacks(new MyFragmentLifecycleCallbacks((fm, f) =>
                    {
                        if (f is AndroidX.Fragment.App.DialogFragment dialogFragment)
                        {
                            var activity = Platform.CurrentActivity;

                            if (activity is null)
                            {
                                return;
                            }

                            var statusBarColor = activity.Window!.StatusBarColor;
                            var navigationBarColor = activity.Window!.NavigationBarColor;
                            var platformStatusBarColor = new Android.Graphics.Color(statusBarColor);
                            var platformNavigationBarColor = new Android.Graphics.Color(navigationBarColor);

                            var dialog = dialogFragment.Dialog;
                            if (dialog is null)
                            {
                                return;
                            }

                            var window = dialog.Window;
                            if (window is null)
                            {
                                return;
                            }
                            dialog?.Window?.SetStatusBarColor(platformStatusBarColor);
                            dialog?.Window?.SetNavigationBarColor(platformNavigationBarColor);

                            bool isColorTransparent = platformStatusBarColor == Android.Graphics.Color.Transparent;
                            if (isColorTransparent)
                            {
                                window.ClearFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                                window.SetFlags(WindowManagerFlags.LayoutNoLimits, WindowManagerFlags.LayoutNoLimits);
                            }
                            else
                            {
                                window.ClearFlags(WindowManagerFlags.LayoutNoLimits);
                                window.SetFlags(WindowManagerFlags.DrawsSystemBarBackgrounds, WindowManagerFlags.DrawsSystemBarBackgrounds);
                            }


                            if ((int)Build.VERSION.SdkInt >= 30)
                            {
#pragma warning disable CA1416
                                window.SetDecorFitsSystemWindows(!isColorTransparent);
#pragma warning restore
                            }
                        }

                    }), false);
                });
            });
        });

        return builder;
    }

    public class MyFragmentLifecycleCallbacks(Action<AndroidX.Fragment.App.FragmentManager, AndroidX.Fragment.App.Fragment> onFragmentStarted) : FragmentManager.FragmentLifecycleCallbacks
    {
        public override void OnFragmentStarted(FragmentManager fm, AndroidX.Fragment.App.Fragment f)
        {
            onFragmentStarted?.Invoke(fm, f);
            base.OnFragmentStarted(fm, f);
        }
    }
}
