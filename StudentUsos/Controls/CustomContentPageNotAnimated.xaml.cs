namespace StudentUsos.Controls;

public partial class CustomContentPageNotAnimated : ContentPage
{
    public bool IsTabBarVisible
    {
        get => isTabBarVisible;
        set
        {
            isTabBarVisible = value;
            TabBar.IsVisible = value;
            if (value)
            {
                AbsoluteLayout.SetLayoutBounds(ContentContainer, DefaultContentStackLayoutLayoutBounds);
            }
            else
            {
                AbsoluteLayout.SetLayoutBounds(ContentContainer, TabBarNotVisibleContentStackLayoutLayoutBounds);
            }
        }
    }
    bool isTabBarVisible;

    VisualElement TabBar { get; init; }
    VisualElement ContentContainer { get; init; }
    Rect DefaultContentStackLayoutLayoutBounds { get; init; }
    Rect TabBarNotVisibleContentStackLayoutLayoutBounds { get; init; }

    public static VisualElement? SnackBarSafeArea { get; private set; }

    public CustomContentPageNotAnimated()
    {
        BindingContext = this;
        InitializeComponent();

        TabBar = (GetTemplateChild("tabBar") as VisualElement) ?? throw new NullReferenceException();
        ContentContainer = (GetTemplateChild("contentContainer") as VisualElement) ?? throw new NullReferenceException();
        DefaultContentStackLayoutLayoutBounds = AbsoluteLayout.GetLayoutBounds(ContentContainer);
        TabBarNotVisibleContentStackLayoutLayoutBounds = new(0, 0, 1, 1);

        Loaded += CustomContentPageNotAnimated_Loaded;
    }

    static Size? tabBarSize = null;

    private void CustomContentPageNotAnimated_Loaded(object? sender, EventArgs e)
    {
        if (tabBarSize is null)
        {
            tabBarSize = TabBar.Measure(double.PositiveInfinity, double.PositiveInfinity);
        }
        SnackBarSafeArea = (GetTemplateChild("snackBarSafeArea") as VisualElement) ?? throw new NullReferenceException();

        double safeAreaHeight;
        if (TabBar.IsVisible)
        {
            safeAreaHeight = tabBarSize?.Height ?? 0;
        }
        else
        {
            safeAreaHeight = App.NavigationBarHeight;
        }

        SnackBarSafeArea.HeightRequest = safeAreaHeight;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
    }
}