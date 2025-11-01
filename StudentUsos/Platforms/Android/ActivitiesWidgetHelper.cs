using Java.Util;

namespace StudentUsos.Platforms.Android
{
    public static class ActivitiesWidgetHelper
    {
        //avoid using DateTime.Now since loading .NET's timezone database on Android can be slow
        public static DateTime GetCurrentDate()
        {
            var calendar = Calendar.Instance;
            int year = calendar.Get(CalendarField.Year);
            int month = calendar.Get(CalendarField.Month) + 1;
            int day = calendar.Get(CalendarField.DayOfMonth);
            return new(year, month, day);
        }
    }
}
