using StudentUsos.Views;

namespace StudentUsos;

public partial class PickFromListPopup : PopupBase
{
    public string PopupTitle
    {
        get => popupTitle;
        set
        {
            popupTitle = value;
            OnPropertyChanged(nameof(PopupTitle));
        }
    }
    string popupTitle = "";
    public IEnumerable<string> Options
    {
        get => options;
        set
        {
            options = value;
            OnPropertyChanged(nameof(Options));
        }
    }
    IEnumerable<string> options;

    public event Action<PickedItem>? OnPicked;
    public event Action OnClosed;

    public string CollectionStateKey
    {
        get => stateKey;
        set
        {
            stateKey = value;
            OnPropertyChanged(nameof(CollectionStateKey));
        }
    }
    string stateKey;

    public class PickedItem
    {
        public string Value { get; private set; }
        public int Index { get; private set; }

        public PickedItem(string value, int index)
        {
            Value = value;
            Index = index;
        }
    }

    public PickFromListPopup()
    {
        InitializeComponent();
        BindingContext = this;
    }

    public PickFromListPopup(string title, IEnumerable<string> options, string stateKey = StateKey.Loaded, Action<PickedItem>? onPicked = null)
    {
        InitializeComponent();
        BindingContext = this;

        PopupTitle = title;
        Options = options;
        CollectionStateKey = stateKey;
        OnPicked += onPicked;
    }

    public static PickFromListPopup CreateAndShow(string title, IEnumerable<string> options, string stateKey = StateKey.Loaded, Action<PickedItem>? onPicked = null)
    {
        var popup = new PickFromListPopup(title, options, stateKey, onPicked);
        ApplicationService.Default.MainThreadInvoke(() =>
        {
            App.Current?.MainPage?.Navigation.PushModalAsync(popup, false);
        });
        return popup;
    }

    public override void Close()
    {
        OnClosed?.Invoke();
        Content.Opacity = 0;
        base.Close();
    }



    private void OptionsButton_Clicked(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        string value = button.Text;
        int index = Options.ToList().IndexOf(value);
        PickedItem pickedItem = new(value, index);
        Close();
        OnPicked?.Invoke(pickedItem);
    }

    private void BackgroundTapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        Close();
    }

    public static class StateKey
    {
        public const string Loading = nameof(Loading);
        public const string Loaded = nameof(Loaded);
        public const string Empty = nameof(Empty);
        public const string LoadingError = nameof(LoadingError);
    }

    protected override void OnDisappearing()
    {
        Content.Opacity = 0;
        base.OnDisappearing();
    }
}