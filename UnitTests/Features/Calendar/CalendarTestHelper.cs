using Moq;
using StudentUsos.Features.Calendar;
using StudentUsos.Features.Calendar.Models;
using StudentUsos.Features.StudentProgrammes.Services;
using StudentUsos.Services.ServerConnection;
using System.Text.Json;

namespace UnitTests.Features.Calendar
{
    public static class CalendarTestHelper
    {
        public static Mock<IStudentProgrammeService> GetStudentProgrammeServiceMock()
        {
            var studentProgrammeServiceMock = new Mock<IStudentProgrammeService>();
            studentProgrammeServiceMock.Setup(x => x.GetCurrentStudentProgrammeAsync())
                .ReturnsAsync(new StudentUsos.Features.StudentProgrammes.Models.StudentProgramme() { FacultyId = "000" });
            return studentProgrammeServiceMock;
        }

        public static Mock<IServerConnectionManager> GetServerConnectionManagetMockForFetchingUsosCalendarEvents(
            int firstEventAddDays = 0,
            int secondEventAddDays = 5,
            int startId = 1000)
        {
            var serverConnectionMock = new Mock<IServerConnectionManager>();
            DateTime date = new(DateTime.Now.Year, DateTime.Now.Month, 1);
            int id = startId;
            for (int i = 0; i < CalendarSettings.MonthsToGetInTotal; i++)
            {
                string dateString = date.ToString("yyyy-MM-dd");
                List<UsosCalendarEvent> events = new()
                {
                    new()
                    {
                        Id = id++,
                        Start = date.AddDays(firstEventAddDays),
                        End = date.AddDays(firstEventAddDays),
                        NameJson = "{\"pl\":\"name\",\"en\":\"name\"}",
                        TypeJson = "{\"pl\":\"type\",\"en\":\"type\"}",
                    },
                    new()
                    {
                        Id = id++,
                        Start = date.AddDays(secondEventAddDays),
                        End = date.AddDays(secondEventAddDays),
                        NameJson = "{\"pl\":\"name\",\"en\":\"name\"}",
                        TypeJson = "{\"pl\":\"type\",\"en\":\"type\"}",
                    }
                };
                serverConnectionMock.Setup(x => x.SendRequestToUsosAsync(
                        It.IsAny<string>(),
                        It.Is<Dictionary<string, string>>(x => x["start_date"] == dateString),
                        It.IsAny<Action<string>>(),
                        It.IsAny<int>()))
                    .ReturnsAsync(JsonSerializer.Serialize(events));
                date = date.AddMonths(1);
            }

            return serverConnectionMock;
        }
    }
}
