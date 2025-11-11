namespace StudentUsos.Controls;

public partial class CustomTabBarXaml : ContentView
{
    public CustomTabBarXaml()
    {
        InitializeComponent();

        if (primary == null)
        {
            primary = Utilities.GetColorFromResources("Primary");
        }
        UpdateActiveTab();

        Shell.Current.Navigated += Current_Navigated;

        tabbarContainer.Margin = new(0, 0, 0, App.NavigationBarHeight);
    }

    private void Current_Navigated(object? sender, ShellNavigatedEventArgs e)
    {
        UpdateActiveTab();
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
        UpdateActiveTab();
    }

    private void GoToActivitiesPageButton_OnClick(object sender, EventArgs e)
    {
        activeIndex = 1;
        Shell.Current.GoToAsync("//ActivitiesPage", parameters: new ShellNavigationQueryParameters() { { "isShell", "true" } });
        UpdateActiveTab();
    }

    private void GoToMorePageButton_OnClick(object sender, EventArgs e)
    {
        activeIndex = 2;
        Shell.Current.GoToAsync("//MorePage");
        UpdateActiveTab();
    }

    static int activeIndex = 0;

    void UpdateActiveTab()
    {
        mainPageTabImage.IsVisible = true;
        activitiesTabImage.IsVisible = true;
        moreTabImage.IsVisible = true;
        mainPageTabImageFill.IsVisible = false;
        activitiesTabImageFill.IsVisible = false;
        moreTabImageFill.IsVisible = false;
        if (activeIndex == 0)
        {
            mainPageTabImage.IsVisible = false;
            mainPageTabImageFill.IsVisible = true;
        }
        else if (activeIndex == 1)
        {
            activitiesTabImage.IsVisible = false;
            activitiesTabImageFill.IsVisible = true;
        }
        else if (activeIndex == 2)
        {
            moreTabImage.IsVisible = false;
            moreTabImageFill.IsVisible = true;
        }
    }
}