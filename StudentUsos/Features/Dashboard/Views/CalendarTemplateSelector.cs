using StudentUsos.Features.Dashboard.Models;

namespace StudentUsos.Features.Dashboard.Views
{
    public class CalendarTemplateSelector : DataTemplateSelector
    {
        public DataTemplate UsosapiTemplate { get; set; }
        public DataTemplate GoogleCalendarTemplate { get; set; }
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            return ((CalendarEvent)item).CalendarEventType == CalendarEvent.EventType.Usosapi ? UsosapiTemplate : GoogleCalendarTemplate;
        }
    }
}
