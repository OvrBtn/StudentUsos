using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentUsos.Features.Settings.Models;
using StudentUsos.Resources.LocalizedStrings;
using System.Globalization;
using System.Text.Json;

namespace StudentUsos.Features.Settings.Views.Subpages
{
    public partial class ApplicationSubpageViewModel : BaseViewModel
    {
        ILocalStorageManager localStorageManager;
        public ApplicationSubpageViewModel(ILocalStorageManager localStorageManager)
        {
            _ = LoadLanguagesAsync();
            this.localStorageManager = localStorageManager;
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
}
