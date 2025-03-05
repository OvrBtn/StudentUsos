using CommunityToolkit.Mvvm.Input;
using SQLite;
using StudentUsos.Features.Activities.Views;
using StudentUsos.Features.Person.Views;
using StudentUsos.Resources.LocalizedStrings;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudentUsos.Features.Person.Models;

[JsonSerializable(typeof(Dictionary<string, Lecturer>)), JsonSerializable(typeof(List<string>)), JsonSerializable(typeof(List<Lecturer>))]
internal partial class LecturerJsonContext : JsonSerializerContext
{ }

/// <summary>
/// class used for additional table in local database
/// </summary>
public partial class Lecturer : Person
{
    /// <summary>
    /// Used only when deserializing json, use <see cref="TitlesBefore"/> and <see cref="TitlesAfter"/> instead
    /// </summary>
    [Ignore, JsonPropertyName("titles")]
    public Dictionary<string, string> TitlesFromJson
    {
        get
        {
            return titlesFromJson;
        }
        set
        {
            titlesFromJson = value;
            TitlesBefore = titlesFromJson["before"];
            TitlesAfter = titlesFromJson["after"];
        }
    }
    Dictionary<string, string> titlesFromJson;
    public string TitlesBefore { get; set; }
    public string TitlesAfter { get; set; }
    public string FullNameWithTitles
    {
        get => this.TitlesBefore + " " + this.FirstName + " " + this.LastName + " " + this.TitlesAfter;
    }
    [JsonPropertyName("homepage_url")]
    public string HomePageUrl { get; set; }
    [JsonPropertyName("profile_url")]
    public string ProfileUrl { get; set; }
    [JsonPropertyName("phone_numbers"), JsonConverter(typeof(JsonObjectToStringConverter))]
    public string PhoneNumbersJson { get; set; }
    [JsonPropertyName("office_hours"), JsonConverter(typeof(JsonObjectToStringConverter))]
    public string OfficeHoursJson { get; set; }
    [JsonPropertyName("employment_positions"), JsonConverter(typeof(JsonObjectToStringConverter))]
    public string EmploymentPositionsJson { get; set; }
    [Ignore, JsonIgnore]
    public List<PhoneNumber> PhoneNumbers
    {
        get
        {
            if (phoneNumbers == null)
            {
                phoneNumbers = DeserializePhoneNumbers(PhoneNumbersJson);
            }
            return phoneNumbers;
        }
        set
        {
            phoneNumbers = value;
        }
    }
    List<PhoneNumber>? phoneNumbers = null;
    public string OfficeHours { get => Utilities.GetLocalizedStringFromJson(OfficeHoursJson); }
    [Ignore, JsonIgnore]
    public List<EmploymentPosition> EmploymentPositions
    {
        get
        {
            if (employmentPositions == null)
            {
                employmentPositions = DeserializeEmploymentPositions(EmploymentPositionsJson) ?? new();
            }
            return employmentPositions;
        }
        set
        {
            employmentPositions = value;
        }
    }
    List<EmploymentPosition>? employmentPositions = null;

    public Lecturer(string id, string firstName, string lastName) : base(id, firstName, lastName)
    {

    }

    public Lecturer() : base()
    {

    }

    [RelayCommand]
    async Task StaffTimetableClickedAsync()
    {
        var page = await App.ServiceProvider.GetService<INavigationService>()!.PushAsync<ActivitiesPage>(false);
        page.Init(this);
    }

    [RelayCommand]
    async Task LecturerClickedAsync()
    {
        var navigationService = App.ServiceProvider.GetService<INavigationService>()!;
        await navigationService.PopToRootAsync(false);
        var page = await navigationService.PushAsync<PersonDetailsPage>(false);
        page.Init(this);
    }

    [RelayCommand]
    void CopyEmailClicked()
    {
        Clipboard.Default.SetTextAsync(Email);
        ApplicationService.Default.ShowToast(LocalizedStrings.PersonDetailsPage_EmailCopied);
    }

    [RelayCommand]
    void HomePageUrlClicked()
    {
        Browser.OpenAsync(HomePageUrl, BrowserLaunchMode.SystemPreferred);
    }

    [RelayCommand]
    void ProfileUrlClicked()
    {
        Browser.OpenAsync(ProfileUrl, BrowserLaunchMode.SystemPreferred);
    }

    static List<EmploymentPosition>? DeserializeEmploymentPositions(string employmentPositions)
    {
        try
        {
            return EmploymentPosition.Deserialize(employmentPositions);
        }
        catch (Exception ex)
        {
            Logger.Default?.LogCatchedException(ex);
            return new();
        }
    }

    static List<PhoneNumber> DeserializePhoneNumbers(string phoneNumbers)
    {
        try
        {
            var deserialized = JsonSerializer.Deserialize(phoneNumbers, LecturerJsonContext.Default.ListString);
            if (deserialized is null)
            {
                return new();
            }
            List<PhoneNumber> numbers = new();
            foreach (var item in deserialized)
            {
                numbers.Add(new(item.ToString()));
            }
            return numbers;
        }
        catch (Exception ex)
        {
            Logger.Default?.LogCatchedException(ex);
            return new();
        }
    }

}