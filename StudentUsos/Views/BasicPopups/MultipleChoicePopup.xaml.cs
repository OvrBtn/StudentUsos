using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;

namespace StudentUsos;

public partial class MultipleChoicePopup : Popup
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
    public IEnumerable<Item> Options
    {
        get => options;
        set
        {
            options = value;
            OnPropertyChanged(nameof(Options));
        }
    }
    IEnumerable<Item> options;

    public event Action<List<Item>> OnConfirmed;
    public event Action<List<Item>> OnCanceled;

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

    public class Item : ObservableObject
    {
        public string Id { get; set; }
        public string Value { get; private set; }
        public bool IsChecked
        {
            get => isChecked;
            set
            {
                isChecked = value;
                OnPropertyChanged(nameof(IsChecked));
            }
        }
        bool isChecked = false;
        public Command ItemClicked { get; set; }
        public Item(string id, string value, bool isChecked = false)
        {
            Id = id;
            Value = value;
            IsChecked = isChecked;
            ItemClicked = new(() => IsChecked = !IsChecked);
        }

        public Item(object id, string value, bool isChecked = false)
        {
            Id = id.ToString()!;
            Value = value;
            IsChecked = isChecked;
            ItemClicked = new(() => IsChecked = !IsChecked);
        }
    }

    public MultipleChoicePopup(string title, IEnumerable<Item> options, string stateKey = StateKey.Loaded)
    {
        InitializeComponent();
        BindingContext = this;

        PopupTitle = title;
        Options = options;
        CollectionStateKey = stateKey;
    }

    public MultipleChoicePopup()
    {
        InitializeComponent();
        BindingContext = this;
    }

    public static MultipleChoicePopup CreateAndShow(string title, IEnumerable<Item> options, string stateKey = StateKey.Loaded)
    {
        MultipleChoicePopup popup = new(title, options, stateKey);
        ApplicationService.Default.MainThreadInvoke(() =>
        {
            App.Current?.Windows[0].Page?.ShowPopup(popup);
        });
        return popup;
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

    private void CancelButton_Clicked(object sender, EventArgs e)
    {
        Close();
        OnCanceled?.Invoke(Options.ToList());
    }

    private void ConfirmButton_Clicked(object sender, EventArgs e)
    {
        Close();
        OnConfirmed?.Invoke(Options.ToList());
    }
}