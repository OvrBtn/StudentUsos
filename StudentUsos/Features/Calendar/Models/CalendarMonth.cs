using CommunityToolkit.Mvvm.ComponentModel;
using CustomCalendar;
using System.Collections.ObjectModel;
using System.Globalization;

namespace StudentUsos.Features.Calendar.Models;

public partial class CalendarMonth : ObservableObject
{
    [ObservableProperty] ObservableCollection<CalendarDay> days = new();
    /// <summary>
    /// Title of month displayed above calendar
    /// </summary>
    public string CalendarTitle { get; set; }
    public DateTime FirstDayOfMonth { get; set; }
    public int Month
    {
        get => FirstDayOfMonth.Month;
    }
    public int Year
    {
        get => FirstDayOfMonth.Year;
    }

    public CalendarMonth(DateTime firstDayOfMonth, Action<CalendarDay> onClickAction, CustomObservableEvents customObservableEvents)
    {
        FirstDayOfMonth = firstDayOfMonth;
        CalendarTitle = firstDayOfMonth.ToString("MMMM yyyy");
        int startOffset = DayOfWeekToInt(firstDayOfMonth.DayOfWeek) - 1;

        for (int i = 1; i <= DateTime.DaysInMonth(firstDayOfMonth.Year, firstDayOfMonth.Month); i++)
        {
            int dayOfMonth = startOffset + i;
            int column = (dayOfMonth - 1) % 7;
            int row = (dayOfMonth - 1 < 0 ? 0 : (dayOfMonth - 1) / 7);
            Days.Add(new CalendarDay(i, firstDayOfMonth, onClickAction, customObservableEvents)
            {
                Column = column,
                Row = row
            });
        }
    }

    public void SetEventsFromServer(List<UsosCalendarEvent> events, bool isPrimaryUpdate)
    {
        try
        {
            if (events.Count == 0) return;
            foreach (var day in Days)
            {
                var eventsFound = events.Where(x => Utilities.CheckIfBetweenDates(FirstDayOfMonth.AddDays(day.Day - 1), x.Start, x.End)).ToList();
                Utilities.ListsDifference(day.EventsUsos.Where(x => x.isPrimaryUpdate == isPrimaryUpdate).ToList(), eventsFound, out List<UsosCalendarEvent> localExceptApi,
                    out List<UsosCalendarEvent> apiExceptLocal, UsosCalendarEvent.AreEqual);

                foreach (var item in localExceptApi)
                {
                    day.RemoveUsosEvent(item);
                }

                foreach (var item in apiExceptLocal)
                {
                    day.AddUsosEvent(item);
                }
            }
        }
        catch (Exception ex) { Logger.Default?.LogCatchedException(ex); }
    }

    public void SetEventsFromLocalDatabase(List<UsosCalendarEvent> data)
    {
        try
        {
            foreach (var item in data)
            {
                if (DateTime.TryParseExact(item.StartString, "dd.MM.yyyy", null, DateTimeStyles.None, out DateTime startDate))
                {
                    DateTime endDate = DateTime.ParseExact(item.EndString, "dd.MM.yyyy", null);
                    var found = Days.Where(x => Utilities.CheckIfBetweenDates(FirstDayOfMonthToFullDate(x.Day), startDate, endDate)).ToList();
                    if (found != null)
                    {
                        foreach (var day in found)
                        {
                            day.AddUsosEvent(item);
                        }
                    }

                }
            }
        }
        catch (Exception ex) { Logger.Default?.LogCatchedException(ex); }
    }

    DateTime FirstDayOfMonthToFullDate(int day)
    {
        DateTime date = FirstDayOfMonth;
        return date.AddDays(day - date.Day);
    }

    /// <summary>
    /// Parse DayOfWeek enum value to int value from range from 1 to 7
    /// In this case week always starts on Monday so Monday will be parsed to 1
    /// </summary>
    /// <param name="dayOfWeek"></param>
    /// <returns></returns>
    int DayOfWeekToInt(DayOfWeek dayOfWeek)
    {
        if (dayOfWeek == DayOfWeek.Sunday) return 7;
        else
        {
            return (int)dayOfWeek;
        }
    }
}