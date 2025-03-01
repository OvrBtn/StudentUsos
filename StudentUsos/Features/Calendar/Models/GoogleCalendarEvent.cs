using SQLite;
using System.Globalization;

namespace StudentUsos.Features.Calendar.Models
{
    public class GoogleCalendarEvent : IEquatable<GoogleCalendarEvent>
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int NotificationId { get; set; }
        [Ignore]
        public GoogleCalendar? Calendar
        {
            get
            {
                return calendar;
            }
            set
            {
                calendar = value;
                CalendarName = calendar?.Name ?? string.Empty;
            }
        }
        GoogleCalendar? calendar;

        public string CalendarName { get; set; }
        [Ignore]
        public DateTime Start
        {
            get
            {
                if (start == DateTime.MinValue)
                {
                    if (DateTime.TryParseExact(StartString, "HH:mm dd.MM.yyyy", null, DateTimeStyles.None, out DateTime result)) start = result;
                    else if (DateTime.TryParse(StartString, out result)) start = result;
                }
                return start;
            }
            set
            {
                start = value;
                StartString = value.ToString("HH:mm dd.MM.yyyy");
            }
        }
        DateTime start = DateTime.MinValue;
        [Ignore]
        public DateTime End
        {
            get
            {
                if (end == DateTime.MinValue)
                {
                    if (DateTime.TryParseExact(EndString, "HH:mm dd.MM.yyyy", null, DateTimeStyles.None, out DateTime result)) end = result;
                    else if (DateTime.TryParse(EndString, out result)) end = result;
                }
                return end;
            }
            set
            {
                end = value;
                EndString = value.ToString("HH:mm dd.MM.yyyy");
            }
        }
        DateTime end = DateTime.MinValue;
        public string StartString { get; set; }
        public string EndString { get; set; }
        [Ignore]
        public string StartBindable
        {
            get
            {
                if (StartString == EndString) return Start.Date.ToString("dd.MM.yyyy");
                return StartString;
            }
        }
        [Ignore]
        public string EndBindable
        {
            get
            {
                if (StartString == EndString) return "";
                return EndString;
            }
        }
        [Ignore]
        public Color BorderColor { get => CalendarColors.GoogleDefault; }

        public static bool AreEqual(GoogleCalendarEvent e1, GoogleCalendarEvent e2)
        {
            bool result = e1.Title == e2.Title
                && e1.Description == e2.Description
                && e1.CalendarName == e2.CalendarName
                && e1.Start == e2.Start
                && e1.End == e2.End;
            return result;
        }

        public bool Equals(GoogleCalendarEvent? other)
        {
            if (other is null)
            {
                return false;
            }
            return AreEqual(this, other);
        }
    }
}
