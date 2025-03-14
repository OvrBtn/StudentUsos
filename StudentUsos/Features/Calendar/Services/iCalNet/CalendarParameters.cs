﻿using System.Text.RegularExpressions;

namespace StudentUsos.Features.Calendar.Services.iCalNet;

public class CalendarParameters : Dictionary<string, CalendarParameter>
{
    private const string ParameterPattern = "(.+?):(.+?)(?=\\r\\n[A-Z]|$)";
    private const RegexOptions ParameteRegexOptions = RegexOptions.Singleline;

    public CalendarParameters(string source)
    {
        MatchCollection parametereMatches = Regex.Matches(source, ParameterPattern, ParameteRegexOptions);
        foreach (Match parametereMatch in parametereMatches)
        {
            string parameterString = parametereMatch.Groups[0].ToString();
            CalendarParameter calendarParameter = new CalendarParameter(parameterString);
            this[calendarParameter.Name] = calendarParameter;
        }
    }
}