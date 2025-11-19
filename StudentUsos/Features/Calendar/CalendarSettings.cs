using System.Globalization;

namespace StudentUsos.Features.Calendar;

public static class CalendarSettings
{
    public const int PrimaryEventsUpdateInterval = 1;
    public const int SecondaryEventsUpdateInterval = 7;
    /// <summary>
    /// Amount of months from now that will be updated every <see cref="PrimaryEventsUpdateInterval"/>
    /// </summary>
    public const int PrimaryUpdateMonths = 2;
    public const int SecondaryUpdateMonths = MonthsToGetInTotal - PrimaryUpdateMonths;
    /// <summary>
    /// Amount of months to retrieve in total
    /// </summary>
    public const int MonthsToGetInTotal = 7;

    public static bool AreCalendarNotificationsEnabled
    {
        get => areCalendarNotificationsEnabled;
        private set => areCalendarNotificationsEnabled = value;
    }
    static bool areCalendarNotificationsEnabled = true;

    public static int DaysBeforeCalendarEventToSendNotification
    {
        get => daysBeforeCalendarEventToSendNotification;
        private set => daysBeforeCalendarEventToSendNotification = value;
    }
    static int daysBeforeCalendarEventToSendNotification = 1;

    public static TimeSpan TimeOfDayOfCalendarEventNotification
    {
        get => timeOfDayOfCalendarEventNotification;
        private set => timeOfDayOfCalendarEventNotification = value;
    }
    static TimeSpan timeOfDayOfCalendarEventNotification = new TimeSpan(15, 0, 0);

    static CalendarSettings()
    {
        LoadNotificationSettingsAndInitializePreferences();
    }

    public static void LoadNotificationSettingsAndInitializePreferences()
    {
        //unit tests will trigger the cctor but since in unit tests
        //environment there is no local storage then this should just return
        if (LocalStorageManager.Default is null)
        {
            return;
        }

        //getting data about calendar notifications or set the defaults
        if (LocalStorageManager.Default.TryGettingString(LocalStorageKeys.AreCalendarNotificationsEnabled, out string result)
            && bool.TryParse(result, out bool parsedBool))
        {
            areCalendarNotificationsEnabled = parsedBool;
        }
        else
        {
            LocalStorageManager.Default.SetString(LocalStorageKeys.AreCalendarNotificationsEnabled, true.ToString());
        }

        if (LocalStorageManager.Default.TryGettingString(LocalStorageKeys.DaysBeforeCalendarEventToSendNotification, out result)
            && int.TryParse(result, out int parsedInt))
        {
            daysBeforeCalendarEventToSendNotification = parsedInt;
        }
        else
        {
            LocalStorageManager.Default.SetString(LocalStorageKeys.DaysBeforeCalendarEventToSendNotification, 1.ToString());
        }

        if (LocalStorageManager.Default.TryGettingString(LocalStorageKeys.TimeOfDayOfCalendarEventNotification, out result)
            && TimeSpan.TryParse(result, CultureInfo.InvariantCulture, out TimeSpan parsedTimeSpan))
        {
            timeOfDayOfCalendarEventNotification = parsedTimeSpan;
        }
        else
        {
            LocalStorageManager.Default.SetString(LocalStorageKeys.TimeOfDayOfCalendarEventNotification,
                new TimeSpan(15, 0, 0).ToString("c", CultureInfo.InvariantCulture));
        }
    }
}