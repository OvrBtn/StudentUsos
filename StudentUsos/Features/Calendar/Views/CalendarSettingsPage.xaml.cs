using StudentUsos.Controls;

namespace StudentUsos.Features.Calendar.Views
{
    public partial class CalendarSettingsPage : CustomContentPageNotAnimated
    {
        CalendarViewModel calendarViewModel;
        CalendarSettingsViewModel calendarSettingsViewModel;
        public CalendarSettingsPage(CalendarSettingsViewModel calendarSettingsViewModel)
        {
            BindingContext = this.calendarSettingsViewModel = calendarSettingsViewModel;
            InitializeComponent();
        }

        public void Init(CalendarViewModel calendarViewModel)
        {
            this.calendarViewModel = calendarViewModel;
            calendarSettingsViewModel.Init(calendarViewModel);
        }
    }
}