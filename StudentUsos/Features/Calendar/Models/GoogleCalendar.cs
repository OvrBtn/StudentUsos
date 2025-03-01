using SQLite;

namespace StudentUsos.Features.Calendar.Models
{
    public class GoogleCalendar
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; } = -1;
        public string Url { get; set; }
        public string Name { get; set; }
        public string ColorString { get; set; }
        [Ignore]
        public Color Color
        {
            get
            {
                if (string.IsNullOrEmpty(ColorString)) return CalendarColors.GoogleDefault;
                else if (color == null) color = Color.FromArgb(ColorString);
                return color;
            }
        }
        Color color;
        public GoogleCalendar()
        {

        }
    }
}
