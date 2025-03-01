using StudentUsos.Controls;

namespace StudentUsos.Features.Calendar.Views
{
    public partial class CalendarPage : CustomContentPageNotAnimated
    {
        CalendarViewModel calendarViewModel;
        INavigationService navigationService;
        public CalendarPage(CalendarViewModel calendarViewModel, INavigationService navigationService)
        {
            this.navigationService = navigationService;
            BindingContext = this.calendarViewModel = calendarViewModel;
            _ = calendarViewModel.InitDelayedAsync();
            InitializeComponent();
        }

        private async void SettingsButton_Clicked(object sender, EventArgs e)
        {
            var page = await navigationService.PushAsync<CalendarSettingsPage>();
            page.Init(calendarViewModel);
        }
    }
}