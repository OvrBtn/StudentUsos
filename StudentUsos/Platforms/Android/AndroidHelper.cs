using Java.Util;

namespace StudentUsos.Platforms.Android
{
    public static class AndroidHelper
    {
        //avoid using DateTime.Now since loading .NET's timezone database on Android can be slow
        public static DateTime GetCurrentDate()
        {
            var calendar = Java.Util.Calendar.Instance;

            int year = calendar.Get(CalendarField.Year);
            int month = calendar.Get(CalendarField.Month) + 1;
            int day = calendar.Get(CalendarField.DayOfMonth);
            int hour = calendar.Get(CalendarField.HourOfDay);
            int minute = calendar.Get(CalendarField.Minute);
            int second = calendar.Get(CalendarField.Second);

            return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Local);
        }

    }
}
