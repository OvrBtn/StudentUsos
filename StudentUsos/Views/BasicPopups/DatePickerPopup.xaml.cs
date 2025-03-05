using StudentUsos.Views;

namespace StudentUsos;

public partial class DatePickerPopup : PopupBase
{
    public event Action<DatePickerPopup, DateOnly> OnPicked;

    DatePickerPopupViewModel viewModel;

    public DatePickerPopup(DatePickerParameters parameters)
    {
        InitializeComponent();
        BindingContext = this.viewModel = new DatePickerPopupViewModel(parameters);
    }

    protected override void OnAppearing()
    {
        var parameters = viewModel.Parameters;

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

        base.OnAppearing();
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
            App.Current?.MainPage?.Navigation.PushModalAsync(new DatePickerPopup(parameters), false);
        });
    }
}