using CommunityToolkit.Mvvm.ComponentModel;
using StudentUsos.Features.Calendar;
using StudentUsos.Features.Calendar.Repositories;

namespace StudentUsos.Features.Settings.Views.Subpages
{
    public partial class NotificationsSubpageViewModel : BaseViewModel
    {
        IUsosCalendarRepository usosCalendarRepository;
        IGoogleCalendarRepository googleCalendarRepository;
        ILocalStorageManager localStorageManager;
        ILogger? logger;
        public NotificationsSubpageViewModel(IUsosCalendarRepository usosCalendarRepository,
            IGoogleCalendarRepository googleCalendarRepository,
            ILocalStorageManager localStorageManager,
            ILogger? logger = null)
        {
            this.usosCalendarRepository = usosCalendarRepository;
            this.googleCalendarRepository = googleCalendarRepository;
            this.localStorageManager = localStorageManager;
        }

        public void Init()
        {
            CalendarSettings.LoadNotificationSettingsAndInitializePreferences();

            areNotificationsEnabledPreviousValue = AreNotificationsEnabled = CalendarSettings.AreCalendarNotificationsEnabled;
            notificationsDayPickedPreviousValue = NotificationsDayPicked = CalendarSettings.DaysBeforeCalendarEventToSendNotification.ToString();
            notificationsTimePickedPreviousValue = NotificationsTimePicked = CalendarSettings.TimeOfDayOfCalendarEventNotification;
        }

        [ObservableProperty] bool areNotificationsEnabled = true;
        [ObservableProperty] string notificationsDayPicked = "1";
        [ObservableProperty] TimeSpan notificationsTimePicked = new TimeSpan(15, 0, 0);

        bool areNotificationsEnabledPreviousValue;
        string notificationsDayPickedPreviousValue;
        TimeSpan notificationsTimePickedPreviousValue;

        public async void SettingsPage_Disappearing(object? sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(NotificationsDayPicked)) NotificationsDayPicked = notificationsDayPickedPreviousValue;

                bool didCalendarNotificationsSettingsChanged = AreNotificationsEnabled != areNotificationsEnabledPreviousValue ||
                                                               NotificationsDayPicked != notificationsDayPickedPreviousValue || NotificationsTimePicked != notificationsTimePickedPreviousValue;

                if (didCalendarNotificationsSettingsChanged)
                {
                    localStorageManager.SetData(LocalStorageKeys.AreNotificationsEnabled, AreNotificationsEnabled.ToString());
                    localStorageManager.SetData(LocalStorageKeys.DaysBeforeCalendarEventToSendNotification, NotificationsDayPicked.ToString());
                    localStorageManager.SetData(LocalStorageKeys.TimeOfDayOfCalendarEventNotification, NotificationsTimePicked.ToString());

                    int notificationsDayPickedInt = int.Parse(NotificationsDayPicked);
                    await usosCalendarRepository.RefreshNotificationsAsync(AreNotificationsEnabled, notificationsDayPickedInt, NotificationsTimePicked);
                    await googleCalendarRepository.RefreshNotificationsAsync(AreNotificationsEnabled, notificationsDayPickedInt, NotificationsTimePicked);
                }
            }
            catch (Exception ex)
            {
                logger?.LogCatchedException(ex);
            }
        }
    }
}
