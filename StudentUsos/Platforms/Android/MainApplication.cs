using Android;
using Android.App;
using Android.Runtime;

[assembly: UsesPermission(Manifest.Permission.WakeLock)]

//Required so that the plugin can reschedule notifications upon a reboot
[assembly: UsesPermission(Manifest.Permission.ReceiveBootCompleted)]
[assembly: UsesPermission(Manifest.Permission.Vibrate)]
//[assembly: UsesPermission("android.permission.SCHEDULE_EXACT_ALARM")]
[assembly: UsesPermission("android.permission.POST_NOTIFICATIONS")]

namespace StudentUsos
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();

            AndroidEnvironment.UnhandledExceptionRaiser += (sender, e) =>
            {
                Logger.Default?.Log(LogLevel.Fatal, e.Exception.ToString());
                e.Handled = true;
            };
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}