using Android.App;
using Android.Content;
using Android.OS;
using StudentUsos.Resources.LocalizedStrings;

namespace StudentUsos;

public static class NotificationsDiagnosisHelper
{
    public static bool IsBackgroundRestricted()
    {
        try
        {
            if ((int)Build.VERSION.SdkInt < 28)
            {
                return false;
            }

            var context = MainApplication.Context;
            var appOps = (AppOpsManager?)context.GetSystemService(MainApplication.AppOpsService);
            if (appOps is null)
            {
                return false;
            }

            const string opstrRunAnyInBackground = "android:run_any_in_background";
#pragma warning disable CA1416
            var mode = appOps.UnsafeCheckOpNoThrow(opstrRunAnyInBackground,
                Android.OS.Process.MyUid(),
                context.PackageName!);
#pragma warning restore
            return mode == AppOpsManagerMode.Ignored;
        }
        catch (Exception e)
        {
            Logger.Default?.LogCatchedException(e);
            return false;
        }
    }

    public static void RequestDisableBackgroundRestrictions()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.P)
        {
            return;
        }
        var context = MainApplication.Context;
        var intent = new Intent(Android.Provider.Settings.ActionApplicationDetailsSettings);
        intent.SetData(Android.Net.Uri.Parse("package:" + context.PackageName));
        intent.SetFlags(ActivityFlags.NewTask);
        context.StartActivity(intent);
    }

    public static bool IsBatteryOptimizationEnabled()
    {
        if ((int)Build.VERSION.SdkInt < 23)
        {
            return false;
        }

        var powerManager = (PowerManager?)Android.App.Application.Context.GetSystemService(Context.PowerService);
        if (powerManager is null)
        {
            return false;
        }

        var context = Android.App.Application.Context;

#pragma warning disable CA1416
        return powerManager.IsIgnoringBatteryOptimizations(context.PackageName) == false;
#pragma warning restore
    }

    public static void RequestDisableBatteryOptimization()
    {
        if ((int)Build.VERSION.SdkInt < 23)
        {
            return;
        }
        var context = Android.App.Application.Context;
#pragma warning disable CA1416
        var intent = new Intent(Android.Provider.Settings.ActionIgnoreBatteryOptimizationSettings);
#pragma warning restore
        intent.SetFlags(ActivityFlags.NewTask);
        context.StartActivity(intent);
    }

    public static bool CheckIfManufacturerWithAutoStartPermission()
    {
        var manufacturer = Build.Manufacturer?.ToLower();
        if (manufacturer is null)
        {
            return false;
        }

        return manufacturer.Contains("xiaomi")
            || manufacturer.Contains("oppo")
            || manufacturer.Contains("vivo")
            || manufacturer.Contains("oneplus")
            || manufacturer.Contains("samsung");
    }

    public static void OpenAutoStartSettings()
    {
        var context = MainApplication.Context;
        var manufacturer = Build.Manufacturer?.ToLower();
        if (manufacturer is null)
        {
            return;
        }

        try
        {
            Intent intent = new Intent();

            if (manufacturer.Contains("xiaomi"))
            {
                intent.SetComponent(new ComponentName("com.miui.securitycenter",
                    "com.miui.permcenter.autostart.AutoStartManagementActivity"));
            }
            else if (manufacturer.Contains("oppo"))
            {
                intent.SetComponent(new ComponentName("com.coloros.safecenter",
                    "com.coloros.safecenter.permission.startup.StartupAppListActivity"));
            }
            else if (manufacturer.Contains("vivo"))
            {
                intent.SetComponent(new ComponentName("com.vivo.permissionmanager",
                    "com.vivo.permissionmanager.activity.BgStartUpManagerActivity"));
            }
            else if (manufacturer.Contains("oneplus"))
            {
                intent.SetComponent(new ComponentName("com.oneplus.security",
                    "com.oneplus.security.chainlaunch.view.ChainLaunchAppListActivity"));
            }
            else if (manufacturer.Contains("samsung"))
            {
                intent.SetComponent(new ComponentName("com.samsung.android.lool",
                    "com.samsung.android.sm.ui.battery.BatteryActivity"));
            }

            intent.SetFlags(ActivityFlags.NewTask);
            context.StartActivity(intent);
        }
        catch
        {
            ApplicationService.Default?.ShowToast(LocalizedStrings.NotificationsDiagnosis_AutoStartCouldNotOpenSettings);
        }
    }
}