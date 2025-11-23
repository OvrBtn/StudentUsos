using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SQLite;
using StudentUsos.Features.Groups.Repositories;
using StudentUsos.Features.Groups.Views;
using System.Globalization;
using System.Text.Json.Serialization;

namespace StudentUsos.Features.Activities.Models;

[JsonSerializable(typeof(List<Activity>))]
internal partial class ActivityJsonContext : JsonSerializerContext
{ }

public partial class Activity : ObservableObject
{
    //This id is here only as a workaround for a bug which causes activities in local database to be doubled.
    //It's likely caused by background worker but reproducing it and fixing the exact source of the issue 
    //turned out to be too problematic. 
    //Also yes it has to be built from so many properties because usos doesn't provide any id and using UnitId/CourseId is not reliable.
    [PrimaryKey]
    public string InternalId => UnitId + CourseId + StartDateTime.ToString(CultureInfo.InvariantCulture) +
        EndDateTime.ToString(CultureInfo.InvariantCulture);

    //data saved to local database
    [JsonPropertyName("unit_id"), JsonConverter(typeof(JsonObjectToStringConverter))]
    public string UnitId { get; set; }
    [JsonPropertyName("start_time")]
    public string StartDateTimeString
    {
        get => startDateTimeString;
        set
        {
            startDateTimeString = value;
            if (DateTime.TryParse(startDateTimeString, out var parsed))
            {
                StartTime = parsed.ToString("HH:mm");
            }

        }
    }
    string startDateTimeString;
    public string StartTime { get; set; }
    [JsonPropertyName("end_time")]
    public string EndDateTimeString
    {
        get => endDateTimeString;
        set
        {
            endDateTimeString = value;
            if (DateTime.TryParse(endDateTimeString, out var parsed))
            {
                EndTime = parsed.ToString("HH:mm");
            }
        }
    }
    string endDateTimeString;
    public string EndTime { get; set; }
    public string Name
    {
        get
        {
            string name = Utilities.GetLocalizedStringFromJson(NameJson);
            if (name.Contains(ClassTypeName)) name = name.Remove(name.LastIndexOf(ClassTypeName));
            if (name.Contains(" - ")) name = name.Remove(name.LastIndexOf(" - "));
            return name;
        }
    }
    [JsonPropertyName("name"), JsonConverter(typeof(JsonObjectToStringConverter))]
    public string NameJson { get; set; }
    public string BuildingName { get => Utilities.GetLocalizedStringFromJson(BuildingNameJson); }
    [JsonPropertyName("building_name"), JsonConverter(typeof(JsonObjectToStringConverter))]
    public string BuildingNameJson { get; set; }
    [JsonPropertyName("room_number")]
    public string RoomNumber { get; set; }
    [JsonPropertyName("group_number"), JsonConverter(typeof(JsonObjectToStringConverter))]
    public string GroupNumber { get; set; }
    [JsonPropertyName("course_id")]
    public string CourseId { get; set; }
    public string ClassTypeName { get => Utilities.GetLocalizedStringFromJson(ClassTypeNameJson); }
    [JsonPropertyName("classtype_name"), JsonConverter(typeof(JsonObjectToStringConverter))]
    public string ClassTypeNameJson { get; set; }
    [JsonPropertyName("lecturer_ids"), JsonConverter(typeof(JsonObjectToStringConverter))]
    public string LecturerIds { get; set; }

    //SQLite doesn't support DateTime
    [Ignore]
    public DateTime StartDateTime
    {
        get
        {
            if (startDateTime != DateTime.MinValue) return startDateTime;
            if (DateTime.TryParse(StartDateTimeString, out DateTime result)) return result;
            return DateTime.MinValue;
        }
        set
        {
            startDateTime = value;
            StartDateTimeString = startDateTime.ToString();
        }
    }
    DateTime startDateTime;
    [Ignore]
    public DateTime EndDateTime
    {
        get
        {
            if (endDateTime != DateTime.MinValue) return endDateTime;
            if (DateTime.TryParse(EndDateTimeString, out DateTime result)) return result;
            return DateTime.MinValue;
        }
        set
        {
            endDateTime = value;
            EndDateTimeString = endDateTime.ToString();
        }
    }
    DateTime endDateTime;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsTimerVisibleNegation))]
    bool isTimerVisible = false;
    [Ignore]
    public bool IsTimerVisibleNegation
    {
        get => !IsTimerVisible;
    }

    [ObservableProperty]
    string timerValue = "";

    /// <summary>
    /// Value from 0 to 1
    /// </summary>
    [ObservableProperty, NotifyPropertyChangedFor(nameof(TimerProgressRect))]
    float timerProgress = 0;

    [Ignore]
    public Rect TimerProgressRect
    {
        get => new(0, 0, TimerProgress, 1);
    }


    static ClassTypeToColorConverter classTypeToColorConverter = new();
    public Color GetRibbonColor()
    {
        return (Color)classTypeToColorConverter.Convert(ClassTypeNameJson, typeof(Activity), null, CultureInfo.InvariantCulture);
    }

    public enum ActivityType
    {
        Student,
        Staff
    }

    public ActivityType Type { get; set; } = ActivityType.Student;

    public enum ClassTypes
    {
        Laboratory,
        Lecture,
        Classes,
        Other
    }

    public ClassTypes ClassType
    {
        get
        {
            string classTypeString = ClassTypeNameJson.ToString()!.ToLower();
            if (classTypeString.Contains("lecture"))
            {
                return ClassTypes.Lecture;
            }
            else if (classTypeString.Contains("laboratory"))
            {
                return ClassTypes.Laboratory;
            }
            else if (classTypeString.Contains("classes"))
            {
                return ClassTypes.Classes;
            }
            return ClassTypes.Other;
        }
    }

    bool isBusy = false;
    [RelayCommand]
    async Task OnClicked()
    {
        if (isBusy) return;
        isBusy = true;
        await Task.Delay(200);
        if (Type == ActivityType.Staff)
        {
            var page = App.ServiceProvider.GetService<StaffGroupDetailsPage>()!;
            page.Init(this, () => isBusy = false);
            page.ShowPopup();
        }
        else
        {
            var groupsRepository = App.ServiceProvider.GetService<IGroupsRepository>()!;
            var group = groupsRepository.GetGroup(CourseId, ClassTypeName) ?? new();
            var page = App.ServiceProvider.GetService<GroupDetailsPage>()!;
            page.Init(group, () => isBusy = false);
            page.ShowPopup();
        }
    }

    /// <summary>
    /// Compare actities using relevant properties
    /// </summary>
    /// <param name="a1"></param>
    /// <param name="a2"></param>
    /// <returns></returns>
    public static bool Comparer(Activity a1, Activity a2)
    {
        return a1.Name == a2.Name &&
            a1.StartDateTime == a2.StartDateTime &&
            a1.EndDateTime == a2.EndDateTime &&
            a1.CourseId == a2.CourseId &&
            a1.RoomNumber == a2.RoomNumber &&
            a1.BuildingName == a2.BuildingName &&
            a1.ClassTypeName == a2.ClassTypeName;
    }

    [ObservableProperty]
    [property: Ignore]
    Rect layoutBounds;

    public Rect CalculateLayoutBounds()
    {
        float oneHourSpace = Views.ActivitiesViewModel.TimetableHourHeight;
        var rect = new Rect();
        rect.X = 0;
        rect.Width = 1;
        var timeDifference = EndDateTime - StartDateTime;
        double height = oneHourSpace * timeDifference.TotalHours;
        rect.Height = height;
        float startHour = StartDateTime.Hour + StartDateTime.Minute / 60f;
        rect.Y = startHour * oneHourSpace;
        return rect;
    }
}