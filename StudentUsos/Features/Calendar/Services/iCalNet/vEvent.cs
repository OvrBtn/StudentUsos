using System.Text.RegularExpressions;

namespace StudentUsos.Features.Calendar.Services.iCalNet;

public class VEvent
{
    private const string VEventContentPattern = "BEGIN:VEVENT\\r\\n(.+)\\r\\nEND:VEVENT";
    private const RegexOptions VEventContentRegexOptions = RegexOptions.Singleline;
    private const string ContentLinePattern = "(.+?):(.+?)(?=\\r\\n[A-Z]|$)";
    private const RegexOptions ContentLineTRegexOptions = RegexOptions.Singleline;

    public Dictionary<string, ContentLine> ContentLines { get; set; }

    public VEvent(string source)
    {
        Match contentMatch = Regex.Match(source, VEventContentPattern, VEventContentRegexOptions);
        string content = contentMatch.Groups[1].ToString();
        MatchCollection matches = Regex.Matches(content, ContentLinePattern, ContentLineTRegexOptions);
        ContentLines = new Dictionary<string, ContentLine>();
        foreach (Match match in matches)
        {
            string contentLineString = match.Groups[0].ToString();
            ContentLine contentLine = new ContentLine(contentLineString);
            ContentLines[contentLine.Name] = contentLine;
        }

    }

}