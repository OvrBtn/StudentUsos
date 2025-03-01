using Moq;
using StudentUsos.Features.Calendar.Repositories;
using StudentUsos.Features.Calendar.Services;

namespace UnitTests.Features.Calendar
{
    [Category(Categories.Unit_SharedBusinessLogic)]
    public class GoogleCalendarServiceTests
    {
        [Fact]
        public void GetGoogleCalendarEventsAsync_WhenRequestIsSuccessful_ReturnsEvents()
        {
            //Arrange
            var googleCalendarRepositoryMock = new Mock<IGoogleCalendarRepository>();

            var googleCalendarService = new GoogleCalendarService(googleCalendarRepositoryMock.Object);

            var icsFile = MockDataHelper.LoadFile("GoogleEvents.txt", "Calendar");

            //Act
            var events = googleCalendarService.GetGoogleCalendarEvents(icsFile);

            //Assert
            Assert.Equal(2, events.Count);

            Assert.Equal("1: event title", events[0].Title);
            Assert.Equal("2: event title", events[1].Title);

            Assert.Equal("1: event description", events[0].Description);
            Assert.Equal("2: event description", events[1].Description);

            Assert.Equal(new DateTime(2025, 2, 20), events[0].Start);
            Assert.Equal(new DateTime(2025, 2, 21), events[1].Start);
        }
    }
}
