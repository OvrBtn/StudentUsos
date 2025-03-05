using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SQLite;
using StudentUsos.Features.Groups.Views;
using StudentUsos.Features.Person.Models;
using StudentUsos.Resources.LocalizedStrings;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudentUsos.Features.Groups.Models;

[JsonSerializable(typeof(Group))]
internal partial class GroupJsonContext : JsonSerializerContext
{ }

[JsonSerializable(typeof(Dictionary<string, Dictionary<string, string>>))]
public partial class CourseEctsPointsJsonContext : JsonSerializerContext
{ }

public partial class Group : ObservableObject, IEquatable<Group>
{
    [PrimaryKey, JsonPropertyName("course_unit_id")]
    public string CourseUnitId { get; set; }
    [JsonPropertyName("course_id")]
    public string CourseId { get; set; }
    [JsonPropertyName("group_number"), JsonConverter(typeof(JsonObjectToStringConverter))]
    public string GroupNumber { get; set; }
    [JsonIgnore]
    public string CourseName { get => Utilities.GetLocalizedStringFromJson(CourseNameJson); }
    [JsonPropertyName("course_name"), JsonConverter(typeof(JsonObjectToStringConverter))]
    public string CourseNameJson { get; set; }
    [JsonPropertyName("course_is_currently_conducted"), JsonConverter(typeof(JsonObjectToStringConverter))]
    public string CourseIsCurrentlyConducted { get; set; }
    [JsonIgnore]
    public string ClassType { get => Utilities.GetLocalizedStringFromJson(ClassTypeJson); }
    [JsonPropertyName("class_type"), JsonConverter(typeof(JsonObjectToStringConverter))]
    public string ClassTypeJson { get; set; }
    [JsonPropertyName("term_id")]
    public string TermId { get; set; }
    [JsonPropertyName("course_fac_id")]
    public string CourseFacultyId { get; set; }
    public string EctsPoints { get; set; }
    [JsonPropertyName("lecturers"), JsonConverter(typeof(JsonObjectToStringConverter))]
    public string LecturersJson { get; set; }
    bool areLecturersSet = false;
    [Ignore, JsonIgnore]
    public List<Lecturer> Lecturers
    {
        get
        {
            if (areLecturersSet == false)
            {
                areLecturersSet = true;
                SetLecturersFromJson(LecturersJson);
            }
            return lecturers;
        }
        set
        {
            SetProperty(ref lecturers, value);
        }
    }
    List<Lecturer> lecturers = new();
    [JsonPropertyName("participants"), JsonConverter(typeof(JsonObjectToStringConverter))]
    public string ParticipantsJson { get; set; }

    bool areParticipantsSet = false;
    [Ignore, JsonIgnore]
    public List<Person.Models.Person> Participants
    {
        get
        {
            if (areParticipantsSet == false)
            {
                areParticipantsSet = true;
                SetParticipantsFromJson(ParticipantsJson);
            }
            return participants;
        }
        set
        {
            SetProperty(ref participants, value);
        }
    }
    List<Person.Models.Person> participants = new();
    void SetLecturersFromJson(string lecturers)
    {
        if (string.IsNullOrEmpty(lecturers)) return;
        var deserialized = JsonSerializer.Deserialize(lecturers, LecturerJsonContext.Default.ListLecturer);
        Lecturers = deserialized ?? new();
    }

    void SetParticipantsFromJson(string participants)
    {
        if (string.IsNullOrEmpty(participants)) return;
        var deserialized = JsonSerializer.Deserialize(participants, PersonJsonContext.Default.ListPerson);
        Participants = deserialized ?? new();
    }

    public string ClassTypeAndGroupNumber
    {
        get => ClassType + ", " + LocalizedStrings.Group.ToLower() + ": " + GroupNumber;
    }

    [RelayCommand]
    void Clicked()
    {
        var page = App.ServiceProvider.GetService<GroupDetailsPage>()!;
        page.Init(this);
        page.ShowPopup();
    }

    public bool Equals(Group? other)
    {
        if (other is null)
        {
            return false;
        }
        return this.CourseName == other.CourseName && this.ClassType == other.ClassType && this.TermId == other.TermId &&
               this.CourseId == other.CourseId && this.EctsPoints == other.EctsPoints;
    }
}