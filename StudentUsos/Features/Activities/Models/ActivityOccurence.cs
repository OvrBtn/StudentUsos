using SQLite;

namespace StudentUsos.Features.Activities.Models;

public class ActivityOccurence
{
    public int Id { get; private set; }
    public string CourseUnitId { get; set; }
    public string GroupNumber { get; set; }
    public string Occurence { get; set; }
    [Ignore]
    public DateOnly OccuranceDateOnly
    {
        get
        {
            if (occuranceDateOnly == DateOnly.MinValue && DateOnly.TryParseExact(Occurence, "yyyy-MM-dd", out DateOnly parsed))
            {
                occuranceDateOnly = parsed;
            }
            return occuranceDateOnly;
        }
        set => occuranceDateOnly = value;
    }
    DateOnly occuranceDateOnly;
    public Activity.ClassTypes ClassType { get; set; }
    public DateTime CreationDate { get; } = DateTime.Now;

    static ClassTypeToColorConverter classTypeToColorConverter = new();
    public Color GetColor()
    {
        return classTypeToColorConverter.Convert(ClassType);
    }

}
