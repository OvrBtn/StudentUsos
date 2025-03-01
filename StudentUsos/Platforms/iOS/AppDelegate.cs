using Foundation;
using UIKit;

namespace StudentUsos
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            base.FinishedLaunching(application, launchOptions);

            App.StatusBarHeight = (int)UIApplication.SharedApplication.StatusBarFrame.Height;

            return base.FinishedLaunching(application, launchOptions);
        }
    }
}