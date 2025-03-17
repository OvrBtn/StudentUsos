using CommunityToolkit.Maui.Views;

namespace StudentUsos;

public partial class DatePickerPopup : Popup
{
    public event Action<DatePickerPopup, DateOnly> OnPicked;

    DatePickerPopupViewModel viewModel;

    public DatePickerPopup(DatePickerParameters parameters)
    {
        InitializeComponent();
        BindingContext = this.viewModel = new DatePickerPopupViewModel(parameters);

        OnPicked += parameters.OnPicked;

        customCalendar.DayClickedCommand = new((args) =>
        {
            OnPicked?.Invoke(this, args.DateOnly);
        });

        var activeDay = parameters.ActiveDay;
        if (activeDay != null)
        {
            customCalendar.ClickedDay = activeDay.Value;
            customCalendar.CurrentDateTime = activeDay.Value.ToDateTime(TimeOnly.MinValue);
        }
    }

    public struct DatePickerParameters
    {
        public Action<DatePickerPopup, DateOnly> OnPicked { get; set; }
        public DateOnly? ActiveDay { get; set; }
    }

    public static void CreateAndShow(Action<DatePickerPopup, DateOnly> onPicked, DateOnly? activeDay = null)
    {
        var parameters = new DatePickerParameters()
        {
            OnPicked = onPicked,
            ActiveDay = activeDay
        };
        ApplicationService.Default.MainThreadInvoke(() =>
        {
            var popup = new DatePickerPopup(parameters);
            App.Current?.MainPage?.ShowPopup(popup);
        });
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        this.Close();
    }
}