namespace StudentUsos.Views.WhatsNew;

public partial class WhatsNewCarouselPage : ContentPage
{
    public WhatsNewCarouselPage()
    {
        InitializeComponent();
        BindingContext = this;

        items = itemsLayout.Children.ToList();
        screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
        for (int i = 0; i < items.Count; i++)
        {
            var view = items[i] as View;
            view.VerticalOptions = LayoutOptions.FillAndExpand;
            view.HorizontalOptions = LayoutOptions.FillAndExpand;
            if (i == 0)
            {
                continue;
            }
            view.TranslationX = screenWidth;
        }

        if (items.Count <= 1)
        {
            nextButton.IsVisible = false;
        }
    }

    double screenWidth;
    List<IView> items = new();

    public string MainStateKey
    {
        get => mainStateKey;
        set
        {
            mainStateKey = value;
            OnPropertyChanged(nameof(MainStateKey));
        }
    }
    string mainStateKey = "Loading";

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        Dispatcher.Dispatch(() =>
        {
            MainStateKey = "Loaded";
        });
    }

    const int CurrentId = 0;
    public static void Initialize(ILocalStorageManager localStorageManager, INavigationService navigationService)
    {
        if (localStorageManager.TryGettingData(LocalStorageKeys.WhatsNewListLastId, out string lastId) == false
            || lastId == "0"
            || lastId == CurrentId.ToString())
        {
            return;
        }

        navigationService.PushModalAsync<WhatsNewListPage>();
        localStorageManager.SetData(LocalStorageKeys.WhatsNewListLastId, CurrentId.ToString());
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        _ = Navigation.PopModalAsync();
    }

    private void SkipButton_Clicked(object sender, EventArgs e)
    {
        _ = Navigation.PopModalAsync();
    }

    private void NextButton_Clicked(object sender, EventArgs e)
    {
        NextItem();
    }

    int currentItemIndex;
    bool isAnimating = false;
    void NextItem()
    {
        if (currentItemIndex == items.Count - 1 || isAnimating)
        {
            return;
        }

        isAnimating = true;

        var previousIndex = currentItemIndex;
        var previousView = items[previousIndex] as View;
        currentItemIndex++;
        var currentView = items[currentItemIndex] as View;

        if (currentItemIndex == items.Count - 1)
        {
            nextButton.IsVisible = false;
        }

        const int animationLength = 750;
        Utilities.Animate(this, progress =>
        {
            previousView.TranslationX = -1 * screenWidth * progress;
        }, animationLength, Easing.CubicInOut);
        Utilities.Animate(this, progress =>
        {
            currentView.TranslationX = (1 - progress) * screenWidth;
        }, animationLength, Easing.CubicInOut, finished: (_, _) => OnAnimationFinished());
    }

    void OnAnimationFinished()
    {
        isAnimating = false;
    }
}