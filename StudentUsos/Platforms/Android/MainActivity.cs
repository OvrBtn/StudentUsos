using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using StudentUsos.Platforms.Android;

namespace StudentUsos;

[Activity(Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTask,
    Exported = true,
    ConfigurationChanges = ConfigChanges.ScreenSize
        | ConfigChanges.Orientation
        | ConfigChanges.UiMode
        | ConfigChanges.ScreenLayout
        | ConfigChanges.SmallestScreenSize
        | ConfigChanges.Density)]
[IntentFilter(new[] { Intent.ActionView },
    Categories = new[]
    {
        Intent.ActionView,
        Intent.CategoryDefault,
        Intent.CategoryBrowsable,
    },
    DataScheme = "studenckiusosput",
    DataHost = "",
    DataPathPrefix = "/studenckiusosput")]
public class MainActivity : MauiAppCompatActivity
{
    public static event Action<string> OnLogInCallback;

    //on 1 device this method is throwing exception for unknown reason, hence the try-catches 
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        try
        {
            base.OnCreate(savedInstanceState);
        }
        catch (Exception ex) { Logger.Default?.LogCatchedException(ex); }

        RequestedOrientation = ScreenOrientation.Portrait;

        SetStatusBarHeight();
        SetNavigationBarHeight();

        CheckUri(this.Intent);

        FirebasePushNotificationsService.GetFcmTokenFuncInitialize(() =>
        {
            var firebaseHelper = new FcmTokenHelper();
            return firebaseHelper.GetFirebaseTokenAsync();
        });
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);

        CheckUri(intent);
    }

    public static event Action<Dictionary<string, string?>> OnIntentRecreatedWithData;

    void CheckUri(Intent? intent)
    {
        try
        {
            if (intent is null)
            {
                return;
            }

            var uri = intent?.Data;
            if (uri is null)
            {
                return;
            }

            Dictionary<string, string?> queryParams = new();

            if (uri.QueryParameterNames is not null)
            {
                foreach (var name in uri.QueryParameterNames)
                {
                    var value = uri.GetQueryParameter(name);
                    queryParams[name] = value;
                }
            }

            OnIntentRecreatedWithData?.Invoke(queryParams);

            if (queryParams.TryGetValue("oauth_verifier", out var verifier) && verifier is not null)
            {
                OnLogInCallback?.Invoke(verifier);
            }
        }
        catch (Exception ex) { Logger.Default?.LogCatchedException(ex); }
    }

    void SetStatusBarHeight()
    {
        try
        {
            if (Resources is null || Resources.DisplayMetrics is null)
            {
                return;
            }
            int resourceId = Resources.GetIdentifier("status_bar_height", "dimen", "android");
            if (resourceId > 0)
            {
                App.StatusBarHeight = (int)(Resources.GetDimensionPixelSize(resourceId) / Resources.DisplayMetrics.Density);
            }
        }
        catch (Exception ex) { Logger.Default?.LogCatchedException(ex); }
    }

    void SetNavigationBarHeight()
    {
        if (Resources is null || Resources.DisplayMetrics is null)
        {
            return;
        }
        int resourceId = Resources.GetIdentifier("navigation_bar_height", "dimen", "android");

        if (resourceId > 0)
        {
            App.NavigationBarHeight = (int)(Resources.GetDimensionPixelSize(resourceId) / Resources.DisplayMetrics.Density);
        }
        else
        {
            App.NavigationBarHeight = 0;
        }
    }
}