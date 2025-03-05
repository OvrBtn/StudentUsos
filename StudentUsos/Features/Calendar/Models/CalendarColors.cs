namespace StudentUsos.Features.Calendar.Models;

public static class CalendarColors
{
    public static Color Primary { get; } = Utilities.GetColorFromResources("Primary");
    public static Color GoogleDefault { get; } = Utilities.GetColorFromResources("Random4");
    public static Color Default { get; } = Utilities.GetColorFromResources("BackgroundColor");
    public static Color Background { get; } = Utilities.GetColorFromResources("BackgroundColor2");
}