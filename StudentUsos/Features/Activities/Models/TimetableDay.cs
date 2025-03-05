using SQLite;

namespace StudentUsos.Features.Activities.Models;

public class TimetableDay
{
    [PrimaryKey]
    public string DateString { get; set; }
    [Ignore]
    public DateTime Date
    {
        get
        {
            if (date != DateTime.MinValue) return date;
            if (DateTime.TryParse(DateString, out DateTime result)) return result;
            return DateTime.MinValue;
        }
        private set
        {
            date = value;
            DateString = date.Date.ToString();
        }
    }
    DateTime date;
    [Ignore]
    public List<Activity> Activities { get; init; } = new();
    public string CreationDateString { get; set; }
    [Ignore]
    public DateTime CreationDate
    {
        get
        {
            if (creationDate != DateTime.MinValue) return creationDate;
            if (DateTime.TryParse(CreationDateString, out DateTime result)) return result;
            return DateTime.MinValue;
        }
        set
        {
            creationDate = value;
            CreationDateString = creationDate.ToString();
        }
    }
    DateTime creationDate;
    public bool IsDayOff { get => Activities.Count == 0; }

    public TimetableDay(DateTime date)
    {
        Date = date.Date;
        CreationDate = DateTimeOffset.Now.DateTime;
    }

    /// <summary>
    /// Empty constructor for SQLite
    /// </summary>
    public TimetableDay()
    {

    }
}