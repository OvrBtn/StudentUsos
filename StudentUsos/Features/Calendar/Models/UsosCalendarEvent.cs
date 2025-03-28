using SQLite;
using StudentUsos.Features.Calendar.Helpers;
using System.Globalization;
using System.Text.Json.Serialization;

namespace StudentUsos.Features.Calendar.Models;

[JsonSerializable(typeof(List<UsosCalendarEvent>))]
internal partial class UsosCalendarEventJsonContext : JsonSerializerContext
{ }

public class UsosCalendarEvent : IEquatable<UsosCalendarEvent>
{
    [PrimaryKey, JsonPropertyName("id"), JsonConverter(typeof(JsonStringToIntConverter))]
    //ID from USOS API
    public int Id { get; set; }
    public string Name { get => Utilities.GetLocalizedStringFromJson(NameJson); }
    [JsonPropertyName("name"), JsonConverter(typeof(JsonObjectToStringConverter))]
    public string NameJson { get; set; }
    public int NotificationId { get; set; }
    public bool isPrimaryUpdate { get; set; }
    [Ignore]
    public DateTime Start
    {
        get
        {
            if (start == DateTime.MinValue && DateTime.TryParseExact(StartString, "dd.MM.yyyy HH:mm", null, DateTimeStyles.None, out DateTime result))
            {
                start = result;
            }
            return start;
        }
        set
        {
            start = value;
            startString = start.ToString("dd.MM.yyyy HH:mm");
        }
    }
    DateTime start;
    [Ignore]
    public DateTime End
    {
        get
        {
            if (end == DateTime.MinValue && DateTime.TryParseExact(EndString, "dd.MM.yyyy HH:mm", null, DateTimeStyles.None, out DateTime result))
            {
                end = result;
            }
            return end;
        }
        set
        {
            end = value;
            endString = end.ToString("dd.MM.yyyy HH:mm");
        }
    }
    DateTime end;
    [JsonPropertyName("start_date")]
    public string StartString
    {
        get => startString;
        set
        {
            startString = value;
            if (DateTime.TryParseExact(startString, "yyyy-MM-dd HH:mm:ss", null, DateTimeStyles.None, out DateTime parsed))
            {
                startString = parsed.ToString("dd.MM.yyyy HH:mm");
            }
        }
    }
    string startString;
    [JsonPropertyName("end_date")]
    public string EndString
    {
        get => endString;
        set
        {
            endString = value;
            if (DateTime.TryParseExact(endString, "yyyy-MM-dd HH:mm:ss", null, DateTimeStyles.None, out DateTime parsed))
            {
                endString = parsed.ToString("dd.MM.yyyy HH:mm");
            }
        }
    }
    string endString;

    [Ignore]
    public string MergedStartAndEndString
    {
        get => Utilities.MergeDateTimes(Start, End);
    }

    public string Type { get => CalendarHelper.GetLocalizedEventTypeFromUsosType(TypeJson); }
    //This isn't actually a JSON, just a normal string because USOS API returns only the type in english.
    //It's written like that in case USOS API started returning JSON with other languages
    [JsonPropertyName("type")]
    public string TypeJson { get; set; }
    [JsonPropertyName("is_day_off")]
    public bool IsDayOff { get; set; }
    [Ignore]
    public Color BorderColor { get => CalendarColors.Primary; }


    public static bool AreEqual(UsosCalendarEvent e1, UsosCalendarEvent e2)
    {
        return e1.IsDayOff == e2.IsDayOff && e1.Name == e2.Name && e1.Type == e2.Type && e1.Start.Date == e2.Start.Date && e1.End.Date == e2.End.Date;
    }

    public bool Equals(UsosCalendarEvent? other)
    {
        if (other is null)
        {
            return false;
        }
        return AreEqual(this, other);
    }
}