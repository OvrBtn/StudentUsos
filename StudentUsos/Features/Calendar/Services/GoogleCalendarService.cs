using StudentUsos.Features.Calendar.Models;
using StudentUsos.Features.Calendar.Repositories;
using StudentUsos.Features.Calendar.Services.iCalNet;
using System.Collections.ObjectModel;
using System.Globalization;

namespace StudentUsos.Features.Calendar.Services
{
    public class GoogleCalendarService : IGoogleCalendarService
    {
        IGoogleCalendarRepository googleCalendarRepository;
        public GoogleCalendarService(IGoogleCalendarRepository googleCalendarRepository)
        {
            this.googleCalendarRepository = googleCalendarRepository;
        }

        HttpClient httpClient = new();
        public async Task<List<GoogleCalendarEvent>> GetGoogleCalendarEventsAsync(GoogleCalendar calendar)
        {
            try
            {
                using var response = await httpClient.GetAsync(calendar.Url);
                string responseString = await response.Content.ReadAsStringAsync();
                return GetGoogleCalendarEvents(responseString);
            }
            catch (HttpRequestException)
            {
                return new();
            }
            catch
            {
                return new();
            }
        }

        public List<GoogleCalendarEvent> GetGoogleCalendarEvents(string icsFileContent, GoogleCalendar? calendar = null)
        {
            try
            {
                ObservableCollection<GoogleCalendarEvent> parsedEvents = new();
                VCalendar vcalendar = new VCalendar(icsFileContent);

                foreach (var events in vcalendar.VEvents)
                {
                    var e = new GoogleCalendarEvent();
                    foreach (var line in events.ContentLines)
                    {
                        if (line.Key == "DESCRIPTION") e.Description = line.Value.Value;
                        if (line.Key == "DTSTART") e.Start = ReadDate(line.Value.Value);
                        if (line.Key == "DTEND")
                        {
                            e.End = ReadDate(line.Value.Value);
                            //Google Calendar sets full day event ending time as 00:00 next day so this fixes it
                            if (line.Value.Value.Contains("T") == false) e.End = e.End.AddDays(-1);
                        }
                        if (line.Key == "SUMMARY") e.Title = line.Value.Value;
                    }
                    e.Calendar = calendar;
                    parsedEvents.Add(e);
                }

                return parsedEvents.ToList();

                //Parse date from it's .ics format to DateTime
                DateTime ReadDate(string s)
                {
                    if (DateTime.TryParseExact(s.Remove(s.Length - 1, 1), "yyyyMMddTHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                    {
                        return result.ToLocalTime();
                    }
                    int year = int.Parse(s.Substring(0, 4));
                    int month = int.Parse(s.Substring(4, 2));
                    int day = int.Parse(s.Substring(6, 2));
                    return new DateTime(year, month, day);
                }
            }
            catch (Exception ex)
            {
                Utilities.ShowError(ex);
                return new();
            }
        }

        public async Task<GoogleCalendar?> CreateCalendarAsync(string url)
        {
            try
            {
                using (var response = await httpClient.GetAsync(url))
                {
                    if (response.IsSuccessStatusCode == false)
                    {
                        return null;
                    }

                    var calendars = googleCalendarRepository.GetAllCalendars();
                    var responseString = await response.Content.ReadAsStringAsync();
                    VCalendar vcalendar = new VCalendar(responseString);
                    GoogleCalendar googleCalendar = new GoogleCalendar()
                    {
                        Name = vcalendar.Parameters["X-WR-CALNAME"].Value,
                        Url = url
                    };

                    if (calendars.Any(x => x.Name == googleCalendar.Name))
                    {
                        return null;
                    }

                    googleCalendar.ColorString = Utilities.GetRandomColor().ToHex();
                    return googleCalendar;
                }
            }
            catch (HttpRequestException)
            {
                return null;
            }
            catch (Exception ex)
            {
                Utilities.ShowError(ex);
                return null;
            }
        }
    }
}
