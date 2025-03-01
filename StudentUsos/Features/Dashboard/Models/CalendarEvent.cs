using StudentUsos.Features.Calendar.Models;

namespace StudentUsos.Features.Dashboard.Models
{
    public class CalendarEvent
    {
        public string Title { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string FullDate { get; set; }
        public EventType CalendarEventType { get; set; }

        //USOS API only
        public string Type { get; set; }
        public bool IsDayOff { get; set; }

        //Google Calendar only
        public string CalendarName { get; set; }
        public string Description { get; set; }

        public CalendarEvent(UsosCalendarEvent usosCalendarEvent)
        {
            Title = usosCalendarEvent.Name;
            Start = usosCalendarEvent.StartString;
            StartDateTime = usosCalendarEvent.Start;
            EndDateTime = usosCalendarEvent.End;
            FullDate = Utilities.MergeDateTimes(StartDateTime, EndDateTime);
            End = usosCalendarEvent.EndString;
            Type = usosCalendarEvent.Type;
            IsDayOff = usosCalendarEvent.IsDayOff;
            CalendarEventType = EventType.Usosapi;
        }

        public CalendarEvent(GoogleCalendarEvent googleCalendarEventData)
        {
            Title = googleCalendarEventData.Title;
            Start = googleCalendarEventData.StartBindable;
            StartDateTime = googleCalendarEventData.Start;
            EndDateTime = googleCalendarEventData.End;
            FullDate = Utilities.MergeDateTimes(StartDateTime, EndDateTime);
            End = googleCalendarEventData.EndBindable;
            CalendarName = googleCalendarEventData.CalendarName;
            Description = googleCalendarEventData.Description;
            CalendarEventType = EventType.GoogleCalendar;
        }

        public enum EventType
        {
            Usosapi,
            GoogleCalendar
        }

        public static bool AreEqual(CalendarEvent e1, CalendarEvent e2)
        {
            return e1.Title == e2.Title && e1.Start == e2.Start && e1.End == e2.End && e1.CalendarEventType == e2.CalendarEventType;
        }
    }
}
