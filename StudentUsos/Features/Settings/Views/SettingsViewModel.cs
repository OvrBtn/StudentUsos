using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentUsos.Features.Calendar;
using StudentUsos.Features.Calendar.Repositories;
using StudentUsos.Features.Settings.Models;
using StudentUsos.Resources.LocalizedStrings;
using System.Globalization;
using System.Text.Json;

namespace StudentUsos.Features.Settings.Views;

public partial class SettingsViewModel : BaseViewModel
{
    [ObservableProperty] bool areNotificationsEnabled = true;
    [ObservableProperty] string notificationsDayPicked = "1";
    [ObservableProperty] TimeSpan notificationsTimePicked = new TimeSpan(15, 0, 0);

    bool areNotificationsEnabledPreviousValue;
    string notificationsDayPickedPreviousValue;
    TimeSpan notificationsTimePickedPreviousValue;

    IUsosCalendarRepository usosCalendarRepository;
    IGoogleCalendarRepository googleCalendarRepository;
    ILocalStorageManager localStorageManager;
    ILogger? logger;
    public SettingsViewModel(IUsosCalendarRepository usosCalendarRepository,
        IGoogleCalendarRepository googleCalendarRepository,
        ILocalStorageManager localStorageManager,
        ILogger? logger = null)
    {
        this.usosCalendarRepository = usosCalendarRepository;
        this.googleCalendarRepository = googleCalendarRepository;
        this.localStorageManager = localStorageManager;

        CalendarSettings.LoadNotificationSettingsAndInitializePreferences();
        areNotificationsEnabledPreviousValue = AreNotificationsEnabled = CalendarSettings.AreCalendarNotificationsEnabled;
        notificationsDayPickedPreviousValue = NotificationsDayPicked = CalendarSettings.DaysBeforeCalendarEventToSendNotification.ToString();
        notificationsTimePickedPreviousValue = NotificationsTimePicked = CalendarSettings.TimeOfDayOfCalendarEventNotification;

        _ = LoadLanguagesAsync();
        this.logger = logger;
    }

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

    [ObservableProperty] string currentLanguageName = CultureInfo.CurrentCulture.NativeName;
    [ObservableProperty] List<Language> languages;
    async Task LoadLanguagesAsync()
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync("supported_languages.json");
        using var reader = new StreamReader(stream);
        string jsonString = reader.ReadToEnd();
        Languages = JsonSerializer.Deserialize(jsonString, LanguageJsonContext.Default.ListLanguage) ?? new();
        if (localStorageManager.TryGettingData(LocalStorageKeys.ChosenLanguageName, out string languageName))
        {
            CurrentLanguageName = languageName;
        }
    }

    [RelayCommand]
    void LanguageButtonClicked()
    {
        var options = Enumerable.Select<Language, string>(Languages, x => x.Name).ToList();
        options.Insert(0, LocalizedStrings.Default);
        PickFromListPopup.CreateAndShow(LocalizedStrings.Language, options, "Loaded", (obj) =>
        {
            if (obj.Index == 0)
            {
                localStorageManager.Remove(LocalStorageKeys.ChosenLanguageName);
                localStorageManager.Remove(LocalStorageKeys.ChosenLanguageCode);
                CurrentLanguageName = LocalizedStrings.Default;
            }
            else
            {
                var chosen = Languages[obj.Index - 1];
                CurrentLanguageName = chosen.Name;
                SetAppLanguage(chosen.Code);
                localStorageManager.SetData(LocalStorageKeys.ChosenLanguageName, chosen.Name);
                localStorageManager.SetData(LocalStorageKeys.ChosenLanguageCode, chosen.Code);
            }
            MessagePopupPage.CreateAndShow(LocalizedStrings.Language, LocalizedStrings.SettingsPage_ChangingLanguageMessage, "ok");

        });
    }

    static void SetAppLanguage(string language)
    {
        CultureInfo culture = new CultureInfo(language);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }
}