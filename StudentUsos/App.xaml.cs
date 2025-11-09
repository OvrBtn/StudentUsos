using Plugin.LocalNotification;
using StudentUsos.Features.Authorization.Services;
using StudentUsos.Features.Groups.Repositories;
using StudentUsos.Features.Groups.Services;
using StudentUsos.Resources.LocalizedStrings;
using StudentUsos.Views.WhatsNew;
using System.Globalization;

namespace StudentUsos;

public partial class App : Application
{
    public static int StatusBarHeight { get; set; }

    public static IServiceProvider ServiceProvider { get; private set; }
    FirebasePushNotificationsService firebasePushNotificationsService;
    ILogger? logger;
    public App(IServiceProvider serviceProvider,
        FirebasePushNotificationsService firebasePushNotificationsService,
        ILogger? logger = null)
    {
        ServiceProvider = serviceProvider;
        this.firebasePushNotificationsService = firebasePushNotificationsService;
        this.logger = logger;

        InitializeComponent();

        SetLanguageFromLocalStorage();

#if RELEASE
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            logger?.Log(LogLevel.Fatal, e.ExceptionObject.ToString()!);
        };

        TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            logger?.Log(LogLevel.Fatal, e.Exception.ToString());
            e.SetObserved();
        };
#endif

    }

    //Called after Android MainActivity ctor
    protected override void OnStart()
    {
        base.OnStart();

        _ = BackwardCompatibility.Check();

        _ = firebasePushNotificationsService.InitNotificationsAsync();

        _ = CheckPermissionsAsync();

        _ = DelayedInitializationAsync();
    }

    async Task DelayedInitializationAsync()
    {
        await Task.Delay(2500);

        if (CheckIfAppRunningForTheFirstTime())
        {
            _ = HandleEmptyLocalDatabaseAsync().ConfigureAwait(false);
        }

        AuthorizationService.CheckForMissingScopes();

        await Task.Delay(3000);

        var localStorageManager = ServiceProvider.GetService<ILocalStorageManager>()!;
        var navigationService = ServiceProvider.GetService<INavigationService>()!;
        WhatsNewCarouselPage.Initialize(localStorageManager, navigationService);
        WhatsNewListPage.Initialize(localStorageManager, navigationService);
    }

    async Task HandleEmptyLocalDatabaseAsync()
    {
        var localStorageManager = ServiceProvider.GetService<ILocalStorageManager>()!;
        var groupsService = ServiceProvider.GetService<IGroupsService>()!;
        var groupsRepository = ServiceProvider.GetService<IGroupsRepository>()!;

        localStorageManager.SetString(LocalStorageKeys.IsAppRunningForTheFirstTime, false.ToString());

        var groupsServer = await groupsService.GetGroupedGroupsServerAsync(false, true);
        if (groupsServer == null)
        {
            return;
        }
        groupsServer.GroupsGrouped.Reverse();
        await groupsService.SetEctsPointsAsync(groupsServer.Groups);
        groupsRepository.InsertOrReplace(groupsServer.Terms);
        groupsRepository.InsertOrReplace(groupsServer.Groups);
    }

    static bool CheckIfAppRunningForTheFirstTime()
    {
        if (LocalStorageManager.Default.TryGettingString(LocalStorageKeys.IsAppRunningForTheFirstTime, out string result))
        {
            return bool.Parse(result);
        }
        else
        {
            LocalStorageManager.Default.SetString(LocalStorageKeys.IsAppRunningForTheFirstTime, bool.TrueString);
            return true;
        }
    }

    async Task CheckPermissionsAsync()
    {
        await Task.Delay(4000);
        if (await LocalNotificationCenter.Current.AreNotificationsEnabled() == false)
        {
            await LocalNotificationCenter.Current.RequestNotificationPermission();
        }
        return;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var shell = new AppShell();
        shell.GoToAsync("//MainLoadingPage");
        _ = PostCreateWindowInitialization(shell);
        return new Window(shell);
    }

    async Task PostCreateWindowInitialization(AppShell shell)
    {
        if (AuthorizationService.CheckIfSignedInAndRetrieveTokens() == false)
        {
            await shell.GoToAsync("//LoginPage");
            return;
        }

        //delay to let app load
        await Task.Delay(5000);

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
        if (LocalStorageManager.Default.TryGettingString(LocalStorageKeys.ChosenLanguageCode, out string languageCode))
        {
            CultureInfo culture = new CultureInfo(languageCode);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }
    }
}