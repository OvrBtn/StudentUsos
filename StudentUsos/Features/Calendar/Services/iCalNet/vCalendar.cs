using System.Text.RegularExpressions;

namespace StudentUsos.Features.Calendar.Services.iCalNet;

public class VCalendar
{
    private const string CalendarParameterPattern = "BEGIN:VCALENDAR\\r\\n(.+?)\\r\\nBEGIN:VEVENT";
    private const RegexOptions CalendarParameterRegexOptions = RegexOptions.Singleline;

    public const string VEventPattern = "(BEGIN:VEVENT.+?END:VEVENT)";
    public const RegexOptions VEventRegexOptions = RegexOptions.Singleline;

    public string Source { get; set; }
    public CalendarParameters Parameters { get; set; }
    public List<VEvent> VEvents { get; set; } = new List<VEvent>();

    public VCalendar(string source)
    {
        Source = source;
        Match parameterMatch = Regex.Match(source, CalendarParameterPattern, CalendarParameterRegexOptions);
        string parameterString = parameterMatch.Groups[1].ToString();
        Parameters = new CalendarParameters(parameterString);
        foreach (Match vEventMatch in Regex.Matches(source, VEventPattern, VEventRegexOptions))
        {
            string vEventString = vEventMatch.Groups[1].ToString();
            VEvents.Add(new VEvent(vEventString));
        }
    }


}