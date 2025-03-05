using CommunityToolkit.Mvvm.ComponentModel;
using StudentUsos.Features.Calendar.Models;
using StudentUsos.Features.Calendar.Repositories;
using StudentUsos.Features.Calendar.Services;
using StudentUsos.Resources.LocalizedStrings;
using System.Collections.ObjectModel;

namespace StudentUsos.Features.Calendar.Views;

public partial class CalendarSettingsViewModel : BaseViewModel
{
    CalendarViewModel calendarViewModel;
    IGoogleCalendarService googleCalendarService;
    IGoogleCalendarRepository googleCalendarRepository;
    ILogger? logger;
    public CalendarSettingsViewModel(IGoogleCalendarService googleCalendarService,
        IGoogleCalendarRepository googleCalendarRepository,
        ILogger? logger = null)
    {
        this.googleCalendarService = googleCalendarService;
        this.googleCalendarRepository = googleCalendarRepository;
        this.logger = logger;
    }

    public void Init(CalendarViewModel calendarViewModel)
    {
        this.calendarViewModel = calendarViewModel;

        TrashOnClick = new Command<object>(arg =>
        {
            var calendar = (GoogleCalendar)arg;
            Calendars.Remove(calendar);

            googleCalendarRepository.RemoveCalendar(calendar);

            //Remove every Google Calendar event from calendar page
            foreach (var month in calendarViewModel.CalendarMonths)
            {
                foreach (var day in month.Days)
                {
                    var found = day.EventsGoogleCalendar.Where(x => x.Calendar.Name == calendar.Name).ToList();
                    foreach (var e in found)
                    {
                        day.RemoveEvent(e);
                    }
                }
            }
        });

        CreateNewCommand = new Command(CreateNewOnClick);
        HelpCommand = new Command(HelpClicked);

        LoadCalendarsList();
    }

    [ObservableProperty] Command createNewCommand;
    [ObservableProperty] Command helpCommand;
    [ObservableProperty] Command<object> trashOnClick;

    void HelpClicked()
    {
        MessagePopupPage.CreateAndShow(LocalizedStrings.CalendarSettingsPage_HelpTItle, LocalizedStrings.CalendarSettingsPage_HelpDescription, "ok");
    }

    [ObservableProperty] ObservableCollection<GoogleCalendar> calendars = new ObservableCollection<GoogleCalendar>();

    void LoadCalendarsList()
    {
        var calendars = googleCalendarRepository.GetAllCalendars();
        foreach (var calendar in calendars)
        {
            Calendars.Add(calendar);
        }
    }

    /// <summary>
    /// Handle creating new calendar from its url
    /// </summary>
    void CreateNewOnClick()
    {
        EntryPopup.CreateAndShow(LocalizedStrings.CalendarSettingsPage_NewCalendarTitle,
            LocalizedStrings.CalendarSettingsPage_NewCalendarDescription, LocalizedStrings.Add, LocalizedStrings.Cancel, ConfirmedAsync);
    }

    async void ConfirmedAsync(string url)
    {
        if (string.IsNullOrEmpty(url) || url.EndsWith(".ics") == false)
        {
            return;
        }
        try
        {
            var googleCalendar = await googleCalendarService.CreateCalendarAsync(url);
            if (googleCalendar is null)
            {
                return;
            }
            CalendarViewModel.GoogleCalendars.Add(googleCalendar);
            Calendars.Add(googleCalendar);
            googleCalendarRepository.SaveCalendar(googleCalendar);
            //Add events from new calendar to calendar view
            if (calendarViewModel != null)
            {
                var events = await googleCalendarService.GetGoogleCalendarEventsAsync(googleCalendar);
                calendarViewModel.SetGoogleEventsServer(events, calendarViewModel.CalendarMonths);
            }
        }
        catch (Exception ex) { logger?.LogCatchedException(ex); }
    }
}