using DevExpress.Maui.Controls;
using DevExpress.Maui.Core;

namespace StudentUsos.Controls;

public partial class CustomBottomSheet : ContentPage
{
    public static readonly BindableProperty OnCloseProperty = BindableProperty.Create(nameof(OnClose), typeof(Action), typeof(CustomBottomSheet));
    /// <summary>
    /// Action fired when popup is closed
    /// </summary>
    public Action OnClose
    {
        get => (Action)GetValue(OnCloseProperty);
        set => SetValue(OnCloseProperty, value);
    }

    public static readonly BindableProperty OnScrollViewCloseToEndProperty = BindableProperty.Create(nameof(OnClose), typeof(Action), typeof(CustomBottomSheet));
    /// <summary>
    /// Action fired when user is almost at the end of ScrollView
    /// </summary>
    public Action OnScrollViewCloseToEnd
    {
        get => (Action)GetValue(OnScrollViewCloseToEndProperty);
        set => SetValue(OnScrollViewCloseToEndProperty, value);
    }

    public static readonly BindableProperty HalfExpandedRatioProperty =
        BindableProperty.Create(nameof(HalfExpandedRatio), typeof(float), typeof(CustomBottomSheet), 0.5f, BindingMode.Default, ValidateSnapPoint);
    public float HalfExpandedRatio
    {
        get => (float)GetValue(HalfExpandedRatioProperty);
        set => SetValue(HalfExpandedRatioProperty, value);
    }

    private static bool ValidateSnapPoint(BindableObject bindable, object value)
    {
        if (float.TryParse(value.ToString(), out float parsed))
        {
            if (parsed < 0 || parsed > 1) throw new Exception("Snap point must be a value between 0 and 1");
            return true;
        }
        return false;
    }

    public CustomBottomSheet()
    {
        InitializeComponent();
    }

    public static CustomBottomSheet? CurrentInstance { get; private set; } = null;

    public void ShowPopup()
    {
        if (CurrentInstance != null)
        {
            return;
        }
        CurrentInstance = this;
        App.Current?.MainPage?.Navigation.PushModalAsync(this);
        ShowDelayedAsync();
    }

    BottomSheet devExpressBottomSheet;
    async void ShowDelayedAsync()
    {
        await Task.Delay(100);
        var bottomSheet = GetTemplateChild("bottomSheet") as BottomSheet;
        if (bottomSheet is null)
        {
            throw new NullReferenceException();
        }
        devExpressBottomSheet = bottomSheet;
        devExpressBottomSheet.Show();
        devExpressBottomSheet.StateChanged += BottomSheet_StateChanged;
    }

    private void BottomSheet_StateChanged(object? sender, ValueChangedEventArgs<BottomSheetState> e)
    {
        if (e.NewValue == BottomSheetState.Hidden)
        {
            Close();
        }
    }

    //When user clicks outside the area of popup
    public void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        Close();
    }

    public void Close()
    {
        CurrentInstance = null;
        devExpressBottomSheet?.Close();
        OnClose?.Invoke();
        if (App.Current?.MainPage?.Navigation.ModalStack.Count > 0)
        {
            App.Current?.MainPage?.Navigation.PopModalAsync();
        }
    }

    protected override void OnDisappearing()
    {
        CurrentInstance = null;
        devExpressBottomSheet?.Close();
        OnClose?.Invoke();
    }

    bool scrolledToEnd = false;
    public void ScrollView_Scrolled(object sender, ScrolledEventArgs e)
    {
        if (!(sender is ScrollView scrollView)) return;
        var scrollSpace = scrollView.ContentSize.Height - scrollView.Height;
        if (scrollSpace - e.ScrollY > 50) scrolledToEnd = false;
        if (scrollSpace - 50 > e.ScrollY || scrolledToEnd) return;
        OnScrollViewCloseToEnd?.Invoke();
        scrolledToEnd = true;

    }
}