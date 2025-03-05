using CommunityToolkit.Mvvm.ComponentModel;
using CustomCalendar;
using System.Collections.ObjectModel;

namespace StudentUsos.Features.Calendar.Models;

public partial class CalendarDay : ObservableObject
{
    [ObservableProperty] int day = 0;
    public string DayString { get => Day == 0 ? "" : Day.ToString(); }
    [ObservableProperty] Color backgroundColor;
    [ObservableProperty] Color borderColor;

    [ObservableProperty] ObservableCollection<EventIndicator> eventsIndicators = new();

    [ObservableProperty] ObservableCollection<UsosCalendarEvent> eventsUsos = new();
    [ObservableProperty] ObservableCollection<GoogleCalendarEvent> eventsGoogleCalendar = new();

    public void AddUsosEvent(UsosCalendarEvent e)
    {
        if (EventsUsos.Count == 0 && EventsIndicators.Any(x => x.Color == CalendarColors.Primary) == false)
        {
            var eventIndicator = new EventIndicator() { Color = CalendarColors.Primary };
            EventsIndicators.Add(eventIndicator);
            if (customObservableEvents != null)
            {
                var customEvent = new CustomEvent(DateOnly.FromDateTime(FullDateTime), DateOnly.FromDateTime(FullDateTime), eventIndicator.Color);
                customEvent.OriginalEventReference = e;
                customEvent.OriginalDayReference = this;
                customObservableEvents.Add(customEvent);
            }
            if (!isToday) BorderColor = CalendarColors.Primary;
        }
        EventsUsos.Add(e);
    }

    public void RemoveUsosEvent(UsosCalendarEvent e)
    {
        EventsUsos.Remove(e);
        if (EventsUsos.Count == 0)
        {
            var found = EventsIndicators.Where(x => x.Color == CalendarColors.Primary).ToList();
            if (found.Count > 0)
            {
                EventsIndicators.Remove(found[0]);
                customObservableEvents?.RemoveAll(x => x.OriginalEventReference == e);
                if (!isToday)
                {
                    if (EventsIndicators.Count > 0) BorderColor = EventsIndicators[EventsIndicators.Count - 1].Color;
                    else BorderColor = CalendarColors.Default;
                }
            }
        }
    }

    public void AddEvent(GoogleCalendarEvent googleGoogleCalendarEvent)
    {
        if (EventsGoogleCalendar.Any(x => x.CalendarName == googleGoogleCalendarEvent.Calendar.Name) == false)
        {
            EventIndicator? eventIndicator = null;
            if (googleGoogleCalendarEvent.Calendar is not null)
            {
                eventIndicator = new EventIndicator() { Color = googleGoogleCalendarEvent.Calendar.Color, CalendarName = googleGoogleCalendarEvent.Calendar.Name };
                EventsIndicators.Add(eventIndicator);
            }
            if (customObservableEvents != null)
            {
                var customEvent = new CustomEvent(DateOnly.FromDateTime(FullDateTime), DateOnly.FromDateTime(FullDateTime), eventIndicator?.Color ?? Colors.Gray);
                customEvent.OriginalEventReference = googleGoogleCalendarEvent;
                customEvent.OriginalDayReference = this;
                customObservableEvents.Add(customEvent);
            }
            if (isToday == false && googleGoogleCalendarEvent.Calendar is not null)
            {
                BorderColor = googleGoogleCalendarEvent.Calendar.Color;
            }
        }
        EventsGoogleCalendar.Add(googleGoogleCalendarEvent);
    }

    public void RemoveEvent(GoogleCalendarEvent googleGoogleCalendarEvent)
    {
        EventsGoogleCalendar.Remove(googleGoogleCalendarEvent);
        var found = EventsIndicators.Where(x => x.Color == googleGoogleCalendarEvent.Calendar.Color).ToList();
        if (found.Count > 0)
        {
            EventsIndicators.Remove(found[0]);
            customObservableEvents?.RemoveAll(x => x.OriginalEventReference == googleGoogleCalendarEvent);
            if (!isToday)
            {
                if (EventsIndicators.Count > 0) BorderColor = EventsIndicators[EventsIndicators.Count - 1].Color;
                else BorderColor = CalendarColors.Default;
            }
        }
    }

    public Command OnClick { get; private set; }
    public DateTime FullDateTime { get; init; }
    public int Column { get; init; }
    public int Row { get; init; }
    bool isToday = false;
    Action<CalendarDay> onClickAction;
    //temporary solution to use custom package for calendar until MAUI's performance improves
    CustomObservableEvents customObservableEvents;
    public CalendarDay(int value, DateTime firstDayOfMonth, Action<CalendarDay> onClickAction, CustomObservableEvents customObservableEvents)
    {
        this.onClickAction = onClickAction;
        this.customObservableEvents = customObservableEvents;

        Day = value;
        FullDateTime = firstDayOfMonth.AddDays(Day - 1);

        if (FullDateTime.Date == DateTime.Today.Date)
        {
            BackgroundColor = CalendarColors.Primary;
            isToday = true;
            onClickAction?.Invoke(this);
        }
        else BackgroundColor = CalendarColors.Background;

        BorderColor = CalendarColors.Background;
        DefaultBackgroundColor = BackgroundColor;

        OnClick = new Command(() => { Clicked(); });
    }

    public Color Color { get; set; } = CalendarColors.GoogleDefault;

    public Color DefaultBackgroundColor;
    public static CalendarDay previousClickedCalendarDay;

    void Clicked()
    {
        onClickAction?.Invoke(this);

        if (previousClickedCalendarDay != null) previousClickedCalendarDay.BackgroundColor = previousClickedCalendarDay.DefaultBackgroundColor;
        previousClickedCalendarDay = this;
        BackgroundColor = new Color(0, 0, 0, 0.2f);
    }
}