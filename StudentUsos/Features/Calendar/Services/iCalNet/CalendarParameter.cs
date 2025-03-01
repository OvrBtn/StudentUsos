using System.Text.RegularExpressions;

namespace StudentUsos.Features.Calendar.Services.iCalNet
{
    public class CalendarParameter
    {
        private const string NameValuePattern = "(.+?):(.+)";

        public string Name { get; set; }
        public string Value { get; set; }

        public CalendarParameter(string source)
        {
            string unfold = ContentLine.UnfoldAndUnescape(source);
            Match nameValueMatch = Regex.Match(unfold, NameValuePattern);
            Name = nameValueMatch.Groups[1].ToString().Trim();
            Value = nameValueMatch.Groups[2].ToString().Trim();
        }

    }
}
