using StudentUsos.Features.Authorization.Services;
using StudentUsos.Features.Calendar;
using StudentUsos.Resources.LocalizedStrings;
using System.Globalization;

namespace StudentUsos
{
    public partial class App : Application
    {
        public static int StatusBarHeight { get; set; }

        public static IServiceProvider ServiceProvider { get; private set; }
        FirebasePushNotificationsService firebasePushNotificationsService;
        public App(IServiceProvider serviceProvider,
            FirebasePushNotificationsService firebasePushNotificationsService)
        {
            ServiceProvider = serviceProvider;
            this.firebasePushNotificationsService = firebasePushNotificationsService;

            InitializeComponent();

            _ = SetMainPageAsync();

            SetLanguageFromLocalStorage();

            CalendarSettings.LoadNotificationSettingsAndInitializePreferences();

            try
            {
                int a = 5, b = 0;
                int c = a / b;
            }
            catch (Exception e)
            {
                Logger.Default!.LogCatchedException(e);
            }
        }

        //Called after Android MainActivity ctor
        protected override void OnStart()
        {
            base.OnStart();

            BackwardCompatibility.Check();

            _ = firebasePushNotificationsService.InitNotificationsAsync();
        }

        async Task SetMainPageAsync()
        {
            MainPage = new AppShell();
            if (AuthorizationService.CheckIfLoggedIn() == false)
            {
                await Shell.Current.GoToAsync("//LoginPage");
            }
            else
            {
                //delay to let app load
                await Task.Delay(4000);

                //if those keys are set then previous app version wasn't integrated with server, checking if session is active (through internal server) doesn't make sense 
                //since it will always be false until usos access tokens are sent to internal server
                if (Preferences.ContainsKey(AuthorizationService.PreferencesKeys.AccessToken.ToString()) &&
                    Preferences.ContainsKey(AuthorizationService.PreferencesKeys.AccessTokenSecret.ToString()))
                {
                    return;
                }

                if (await AuthorizationService.IsSessionActiveAsync() == false)
                {
                    MessagePopupPage.CreateAndShow(LocalizedStrings.SessionInvalidMessage_Title, LocalizedStrings.SessionInvalidMessage_Description,
                        LocalizedStrings.Logout, LocalizedStrings.Later, () => AuthorizationService.LogoutAsync(), null);
                }
            }
        }

        internal static Action<Color> SetNavigationBarColor { get; set; }
        public static Color? NavigationBarColor
        {
            get => navigationBarColor;
            set
            {
                if (navigationBarColor == value || SetNavigationBarColor == null) return;
                navigationBarColor = value;
                SetNavigationBarColor(value ?? Colors.Gray);
            }
        }
        static Color? navigationBarColor = Utilities.GetColorFromResources("BackgroundColor");
        internal static Action<Color> SetStatusBarColor { get; set; }
        public static Color? StatusBarColor
        {
            get => statusBarColor;
            set
            {
                if (statusBarColor == value || SetNavigationBarColor == null) return;
                statusBarColor = value;
                SetStatusBarColor(value ?? Colors.Gray);
            }
        }
        static Color? statusBarColor = Utilities.GetColorFromResources("BackgroundColor");

        static void SetLanguageFromLocalStorage()
        {
            if (LocalStorageManager.Default.TryGettingData(LocalStorageKeys.ChosenLanguageCode, out string languageCode))
            {
                CultureInfo culture = new CultureInfo(languageCode);
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
            }
        }
    }
}