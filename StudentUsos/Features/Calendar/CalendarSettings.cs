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

    public static void LoadNotificationSettingsAndInitializePreferences()
    {
        //getting data about calendar notifications or set the defaults
        if (LocalStorageManager.Default.TryGettingData(LocalStorageKeys.AreNotificationsEnabled, out string result) && bool.TryParse(result, out bool parsedBool))
        {
            areCalendarNotificationsEnabled = parsedBool;
        }
        else
        {
            LocalStorageManager.Default.SetData(LocalStorageKeys.AreNotificationsEnabled, true.ToString());
        }

        if (LocalStorageManager.Default.TryGettingData(LocalStorageKeys.DaysBeforeCalendarEventToSendNotification, out result) && int.TryParse(result, out int parsedInt))
        {
            daysBeforeCalendarEventToSendNotification = parsedInt;
        }
        else
        {
            LocalStorageManager.Default.SetData(LocalStorageKeys.DaysBeforeCalendarEventToSendNotification, 1.ToString());
        }

        if (LocalStorageManager.Default.TryGettingData(LocalStorageKeys.TimeOfDayOfCalendarEventNotification, out result) && TimeSpan.TryParse(result, out TimeSpan parsedTimeSpan))
        {
            timeOfDayOfCalendarEventNotification = parsedTimeSpan;
        }
        else
        {
            LocalStorageManager.Default.SetData(LocalStorageKeys.TimeOfDayOfCalendarEventNotification, new TimeSpan(15, 0, 0).ToString());
        }
    }
}