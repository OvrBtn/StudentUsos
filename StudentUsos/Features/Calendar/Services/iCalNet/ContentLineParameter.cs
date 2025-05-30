﻿using System.Text.RegularExpressions;

namespace StudentUsos.Features.Calendar.Services.iCalNet;

public class ContentLineParameter
{
    private const string NameValuePattern = "(.+?)=(.+)";
    private const string ValueListPattern = "([^,]+)(?=,|$)";

    public string Name { get; set; }
    public List<string> Values { get; set; } = new List<string>();

    public ContentLineParameter(string source)
    {
        Match match = Regex.Match(source, NameValuePattern);
        Name = match.Groups[1].ToString().Trim();
        string valueString = match.Groups[2].ToString();
        MatchCollection matches = Regex.Matches(valueString, ValueListPattern);
        foreach (Match paramMatch in matches)
            Values.Add(paramMatch.Groups[1].ToString().Trim());
    }

}