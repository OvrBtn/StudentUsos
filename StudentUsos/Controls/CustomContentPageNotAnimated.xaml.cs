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

    public CustomContentPageNotAnimated()
    {
        BindingContext = this;
        InitializeComponent();

        TabBar = (GetTemplateChild("tabBar") as VisualElement) ?? throw new NullReferenceException();
        ContentContainer = (GetTemplateChild("contentContainer") as VisualElement) ?? throw new NullReferenceException();
        DefaultContentStackLayoutLayoutBounds = AbsoluteLayout.GetLayoutBounds(ContentContainer);
        TabBarNotVisibleContentStackLayoutLayoutBounds = new(0, 0, 1, 1);

    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
    }
}