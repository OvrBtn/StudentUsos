using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentUsos.Features.Settings.Models;
using StudentUsos.Features.Settings.Views.Subpages;
using StudentUsos.Resources.LocalizedStrings;
using System.Globalization;
using System.Text.Json;

namespace StudentUsos.Features.Settings.Views;

public partial class SettingsViewModel : BaseViewModel
{
    ILocalStorageManager localStorageManager;
    INavigationService navigationService;
    public SettingsViewModel(ILocalStorageManager localStorageManager, INavigationService navigationService)
    {
        this.localStorageManager = localStorageManager;
        this.navigationService = navigationService;

        _ = LoadLanguagesAsync();
    }

    [RelayCommand]
    async Task GoToNotificationsSubpageAsync()
    {
        await navigationService.PushAsync<NotificationsSubpage>();
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