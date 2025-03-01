using StudentUsos.Resources.LocalizedStrings;

namespace StudentUsos.Features.Calendar.Helpers
{
    static class CalendarHelper
    {
        /// <summary>
        /// Parse from USOS API type localized type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetLocalizedEventTypeFromUsosType(string type)
        {
            switch (type)
            {
                case "rector": return LocalizedStrings.CalendarEventType_Rector;
                case "dean": return LocalizedStrings.CalendarEventType_Dean;
                case "holidays": return LocalizedStrings.CalendarEventType_Holidays;
                case "public_holidays": return LocalizedStrings.CalendarEventType_PublicHolidays;
                case "break": return LocalizedStrings.CalendarEventType_Break;
                case "exam_session": return LocalizedStrings.CalendarEventType_ExamSession;
                default: return "";
            }
        }
    }
}
