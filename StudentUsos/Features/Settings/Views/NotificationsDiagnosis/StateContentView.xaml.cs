namespace StudentUsos.Features.Settings.Views.NotificationsDiagnosis;

public partial class StateContentView : ContentView
{
    public static readonly BindableProperty CurrentStateProperty = BindableProperty.Create(nameof(CurrentState), typeof(States), typeof(StateContentView), States.Loading,
         propertyChanged: CurrentStatePropertyChanged);
    public States CurrentState
    {
        get => (States)GetValue(CurrentStateProperty);
        set => SetValue(CurrentStateProperty, value);
    }

    static void CurrentStatePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StateContentView stateContentView)
        {
            stateContentView.OnPropertyChanged(nameof(stateContentView.CurrentStateString));
        }
    }

    public string CurrentStateString
    {
        get => CurrentState.ToString();
    }

    public StateContentView()
    {
        InitializeComponent();
    }
}