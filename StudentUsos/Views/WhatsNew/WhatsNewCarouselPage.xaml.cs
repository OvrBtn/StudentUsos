namespace StudentUsos.Views.WhatsNew;

public partial class WhatsNewCarouselPage : ContentPage
{
    public WhatsNewCarouselPage()
    {
        InitializeComponent();
        BindingContext = this;

        indicatorView.Count = mainCarousel.ChildrenCount;

        App.NavigationBarColor = Utilities.GetColorFromResources("BackgroundColor2");
    }

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

    private void MainCarousel_SelectedIndexChanged(object sender, int e)
    {
        indicatorView.Position = e;
    }
}