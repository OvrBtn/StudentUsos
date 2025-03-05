namespace StudentUsos.Controls;

public partial class CustomTabBarXaml : ContentView
{
    public CustomTabBarXaml()
    {
        InitializeComponent();

        if (primary == null) primary = Utilities.GetColorFromResources("Primary");
        SetBackgroundColors();

        Shell.Current.Navigated += Current_Navigated;
    }

    private void Current_Navigated(object? sender, ShellNavigatedEventArgs e)
    {
        SetBackgroundColors();
    }

    ~CustomTabBarXaml()
    {
        Shell.Current.Navigated -= Current_Navigated;
    }

    static Color primary;

    private void GoToMainPageButton_OnClick(object sender, EventArgs e)
    {
        activeIndex = 0;
        Shell.Current.GoToAsync("//DashboardPage");
        SetBackgroundColors();
    }

    private void GoToActivitiesPageButton_OnClick(object sender, EventArgs e)
    {
        activeIndex = 1;
        Shell.Current.GoToAsync("//ActivitiesPage", parameters: new ShellNavigationQueryParameters() { { "isShell", "true" } });
        SetBackgroundColors();
    }

    private void GoToMorePageButton_OnClick(object sender, EventArgs e)
    {
        activeIndex = 2;
        Shell.Current.GoToAsync("//MorePage");
        SetBackgroundColors();
    }

    static int activeIndex = 0;

    void SetBackgroundColors()
    {
        mainPageTab.BackgroundColor = Colors.Transparent;
        activitiesTab.BackgroundColor = Colors.Transparent;
        moreTab.BackgroundColor = Colors.Transparent;
        if (activeIndex == 0)
        {
            mainPageTab.BackgroundColor = primary;
        }
        else if (activeIndex == 1)
        {
            activitiesTab.BackgroundColor = primary;
        }
        else if (activeIndex == 2)
        {
            moreTab.BackgroundColor = primary;
        }
    }
}