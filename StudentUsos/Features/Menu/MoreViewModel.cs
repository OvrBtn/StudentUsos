using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentUsos.Features.Authorization.Services;
using StudentUsos.Features.Calendar.Views;
using StudentUsos.Features.Grades.Views;
using StudentUsos.Features.Groups.Views;
using StudentUsos.Features.Payments.Views;
using StudentUsos.Features.SatisfactionSurveys.Views;
using StudentUsos.Features.Settings.Views;
using StudentUsos.Resources.LocalizedStrings;
using System.Collections.ObjectModel;

namespace StudentUsos.Features.Menu;

public partial class MoreViewModel : BaseViewModel
{
    [ObservableProperty] Command logoutOnClick;

    [ObservableProperty] ObservableCollection<MorePageButton> buttons = new();

    INavigationService navigationService;
    public MoreViewModel(INavigationService navigationService)
    {
        this.navigationService = navigationService;

        //Buttons = new()
        //{
        //    new(LocalizedStrings.Calendar, "calendar3_week2.png", new Command(() => GoToPage(new CalendarPage()))),
        //    new(LocalizedStrings.FinalGrades, "grade256.png", new Command(() => GoToPage(new GradesPage()))),
        //    new(LocalizedStrings.Groups, "people_fill2.png", new Command(() => GoToPage(new GroupsPage()))),
        //    new(LocalizedStrings.Surveys, "star.png", new Command(() => GoToPage(new SatisfactionSurveysPage()))),
        //    new(LocalizedStrings.Payments, "credit_card.png", new Command(() => GoToPage(new PaymentsPage()))),
        //    new(LocalizedStrings.Settings, "gear256.png", new Command(() => GoToPage(new SettingsPage())))
        //};

        LogoutOnClick = new Command(LogoutClicked);
    }

    [RelayCommand]
    async Task GoToCalendarPageAsync()
    {
        await navigationService.PushAsync<CalendarPage>();
    }

    [RelayCommand]
    async Task GoToGradesPageAsync()
    {
        await navigationService.PushAsync<GradesPage>();
    }

    [RelayCommand]
    async Task GoToGroupsPageAsync()
    {
        await navigationService.PushAsync<GroupsPage>();
    }

    [RelayCommand]
    async Task GoToSatisfactionSurveysPageAsync()
    {
        await navigationService.PushAsync<SatisfactionSurveysPage>();
    }

    [RelayCommand]
    async Task GoToPaymentsPageAsync()
    {
        await navigationService.PushAsync<PaymentsPage>();
    }

    [RelayCommand]
    async Task GoToSettingsPageAsync()
    {
        await navigationService.PushAsync<SettingsPage>();
    }

    async void GoToPageAsync<T>() where T : ContentPage
    {
        await navigationService.PushAsync<SettingsPage>();
    }

    void LogoutClicked()
    {
        MessagePopupPage.CreateAndShow(LocalizedStrings.LoggingOut, LocalizedStrings.LoggingOut_Confirmation, LocalizedStrings.Yes, LocalizedStrings.No, () => AuthorizationService.LogoutAsync(), null);
    }

}

public class MorePageButton
{
    public string Text { get; set; }
    public string ImageName { get; set; }
    public Command OnClickCommand { get; set; }

    public MorePageButton(string text, string imageName, Command onClickCommand)
    {
        Text = text;
        ImageName = imageName;
        OnClickCommand = onClickCommand;
    }
}