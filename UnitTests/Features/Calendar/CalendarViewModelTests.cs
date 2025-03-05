using Moq;
using StudentUsos.Features.Calendar;
using StudentUsos.Features.Calendar.Models;
using StudentUsos.Features.Calendar.Repositories;
using StudentUsos.Features.Calendar.Services;
using StudentUsos.Features.Calendar.Views;
using StudentUsos.Services.LocalDatabase;
using StudentUsos.Services.LocalNotifications;

namespace UnitTests.Features.Calendar;

[Category(Categories.Unit_SharedBusinessLogic)]
public class CalendarViewModelTests
{
    Mock<IGoogleCalendarRepository> mockGoogleCalendarRepo = new();
    Mock<IGoogleCalendarService> mockGoogleCalendarService = new();
    Mock<IUsosCalendarRepository> mockUsosCalendarRepo = new();
    Mock<IUsosCalendarService> mockUsosCalendarService = new();
    ApplicationServiceMock mockApplicationService = new();


    List<GoogleCalendarEvent> googleEventsServer = new();
    List<GoogleCalendarEvent> googleEventsLocal = new();
    GoogleCalendar googleCalendar;

    const int MonthsInTotal = 4;
    const int FirstAnalyzedDayIndex = 5;
    const int SecondAnalyzedDayIndex = 6;

    List<UsosCalendarEvent> usosEventsLocal = new();
    public CalendarViewModelTests()
    {
        //Default mocks to avoid null reference in viewmodel
        mockGoogleCalendarRepo.Setup(x => x.GetAllEvents())
            .Returns(new List<GoogleCalendarEvent>());
        mockGoogleCalendarService.Setup(x => x.GetGoogleCalendarEventsAsync(It.IsAny<GoogleCalendar>()))
            .ReturnsAsync(new List<GoogleCalendarEvent>());
        mockGoogleCalendarRepo.Setup(x => x.GetAllCalendars())
            .Returns(new List<GoogleCalendar>() { googleCalendar });

        mockUsosCalendarRepo.Setup(x => x.GetEvents(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(new List<UsosCalendarEvent>());

        var date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        string calendarName = "calendar";
        googleCalendar = new GoogleCalendar()
        {
            Name = calendarName
        };

        int usosEventId = 0;
        string defaultTitle = "title";
        for (int i = 0; i < MonthsInTotal; i++)
        {
            GoogleCalendarEvent event1Google = new()
            {
                CalendarName = calendarName,
                Calendar = new()
                {
                    Name = calendarName
                },
                Title = defaultTitle,
                Description = "desc",
                Start = date.AddDays(FirstAnalyzedDayIndex).AddHours(12).AddMonths(i),
                End = date.AddDays(FirstAnalyzedDayIndex).AddHours(14).AddMonths(i)
            };
            googleEventsLocal.Add(event1Google);
            var event1GoogleCopy = GeneralTestHelper.DeepCopyReflection(event1Google);
            event1GoogleCopy.Start = event1GoogleCopy.Start.AddMinutes(20);
            googleEventsServer.Add(event1GoogleCopy);

            GoogleCalendarEvent event2Google = new()
            {
                CalendarName = calendarName,
                Calendar = new()
                {
                    Name = calendarName
                },
                Title = defaultTitle,
                Description = "desc",
                Start = date.AddDays(SecondAnalyzedDayIndex).AddHours(15).AddMonths(i),
                End = date.AddDays(SecondAnalyzedDayIndex).AddHours(16).AddMonths(i)
            };
            googleEventsLocal.Add(event2Google);
            var event2GoogleCopy = GeneralTestHelper.DeepCopyReflection(event2Google);
            event2GoogleCopy.Start = event2GoogleCopy.Start.AddMinutes(20);
            googleEventsServer.Add(event2GoogleCopy);

            UsosCalendarEvent event1Usos = new()
            {
                Id = usosEventId++,
                NameJson = "{\"pl\":\"name\",\"en\":\"name\"}",
                TypeJson = "{\"pl\":\"type\",\"en\":\"type\"}",
                isPrimaryUpdate = i < CalendarSettings.PrimaryUpdateMonths,
                Start = date.AddDays(FirstAnalyzedDayIndex).AddMonths(i),
                End = date.AddDays(FirstAnalyzedDayIndex).AddMonths(i)
            };
            usosEventsLocal.Add(event1Usos);

            UsosCalendarEvent event2Usos = new()
            {
                Id = usosEventId++,
                NameJson = "{\"pl\":\"name\",\"en\":\"name\"}",
                TypeJson = "{\"pl\":\"type\",\"en\":\"type\"}",
                isPrimaryUpdate = i < CalendarSettings.PrimaryUpdateMonths,
                Start = date.AddDays(SecondAnalyzedDayIndex).AddMonths(i),
                End = date.AddDays(SecondAnalyzedDayIndex).AddMonths(i)
            };
            usosEventsLocal.Add(event2Usos);
        }
    }

    #region GoogleCalendar

    [Fact]
    public async Task GoogleCalendar_WhenLocalDatabaseOnly_AssignsCorrectly()
    {
        //Arrange
        mockGoogleCalendarRepo.Setup(x => x.GetAllEvents())
            .Returns(googleEventsLocal);
        mockGoogleCalendarRepo.Setup(x => x.GetAllCalendars())
            .Returns(new List<GoogleCalendar>() { googleCalendar });

        CalendarViewModel calendarViewModel = new(mockUsosCalendarService.Object,
            mockUsosCalendarRepo.Object,
            mockGoogleCalendarService.Object,
            mockGoogleCalendarRepo.Object,
            mockApplicationService);

        //Act
        await calendarViewModel.InitAsync();

        //Assert
        for (int i = 0; i < MonthsInTotal; i++)
        {
            Assert.Equal(1, calendarViewModel.CalendarMonths[i].Days[FirstAnalyzedDayIndex].EventsGoogleCalendar.Count);
            Assert.Equal(1, calendarViewModel.CalendarMonths[i].Days[SecondAnalyzedDayIndex].EventsGoogleCalendar.Count);

            Assert.Equal(12, calendarViewModel.CalendarMonths[i].Days[FirstAnalyzedDayIndex].EventsGoogleCalendar[0].Start.Hour);
            Assert.Equal(15, calendarViewModel.CalendarMonths[i].Days[SecondAnalyzedDayIndex].EventsGoogleCalendar[0].Start.Hour);
        }

    }

    [Fact]
    public async Task GoogleCalendar_WhenServerOnly_AssignsCorrectly()
    {
        //Arrange
        mockGoogleCalendarRepo.Setup(x => x.GetAllCalendars())
            .Returns(new List<GoogleCalendar>() { googleCalendar });
        mockGoogleCalendarService.Setup(x => x.GetGoogleCalendarEventsAsync(It.IsAny<GoogleCalendar>()))
            .ReturnsAsync(googleEventsServer);

        CalendarViewModel calendarViewModel = new(mockUsosCalendarService.Object,
            mockUsosCalendarRepo.Object,
            mockGoogleCalendarService.Object,
            mockGoogleCalendarRepo.Object,
            mockApplicationService);

        //Act
        await calendarViewModel.InitAsync();

        //Assert
        for (int i = 0; i < MonthsInTotal; i++)
        {
            Assert.Equal(1, calendarViewModel.CalendarMonths[i].Days[FirstAnalyzedDayIndex].EventsGoogleCalendar.Count);
            Assert.Equal(1, calendarViewModel.CalendarMonths[i].Days[SecondAnalyzedDayIndex].EventsGoogleCalendar.Count);

            Assert.Equal(12, calendarViewModel.CalendarMonths[i].Days[FirstAnalyzedDayIndex].EventsGoogleCalendar[0].Start.Hour);
            Assert.Equal(20, calendarViewModel.CalendarMonths[i].Days[FirstAnalyzedDayIndex].EventsGoogleCalendar[0].Start.Minute);
            Assert.Equal(15, calendarViewModel.CalendarMonths[i].Days[SecondAnalyzedDayIndex].EventsGoogleCalendar[0].Start.Hour);
            Assert.Equal(20, calendarViewModel.CalendarMonths[i].Days[SecondAnalyzedDayIndex].EventsGoogleCalendar[0].Start.Minute);
        }
    }

    [Fact]
    public async Task GoogleCalendar_WhenLocalAndServer_AssignsCorrectly()
    {
        //Arrange
        List<GoogleCalendarEvent> allEvents = new(googleEventsLocal);
        allEvents.AddRange(googleEventsServer);

        mockGoogleCalendarRepo.Setup(x => x.GetAllCalendars())
            .Returns(new List<GoogleCalendar>() { googleCalendar });
        mockGoogleCalendarRepo.Setup(x => x.GetAllEvents())
            .Returns(googleEventsLocal);
        mockGoogleCalendarService.Setup(x => x.GetGoogleCalendarEventsAsync(It.IsAny<GoogleCalendar>()))
            .ReturnsAsync(allEvents);

        CalendarViewModel calendarViewModel = new(mockUsosCalendarService.Object,
            mockUsosCalendarRepo.Object,
            mockGoogleCalendarService.Object,
            mockGoogleCalendarRepo.Object,
            mockApplicationService);

        //Act
        await calendarViewModel.InitAsync();

        //Assert
        for (int i = 0; i < MonthsInTotal; i++)
        {
            Assert.Equal(2, calendarViewModel.CalendarMonths[i].Days[FirstAnalyzedDayIndex].EventsGoogleCalendar.Count);
            Assert.Equal(2, calendarViewModel.CalendarMonths[i].Days[SecondAnalyzedDayIndex].EventsGoogleCalendar.Count);

            Assert.Equal(12, calendarViewModel.CalendarMonths[i].Days[FirstAnalyzedDayIndex].EventsGoogleCalendar[0].Start.Hour);
            Assert.Equal(20, calendarViewModel.CalendarMonths[i].Days[FirstAnalyzedDayIndex].EventsGoogleCalendar[1].Start.Minute);
            Assert.Equal(15, calendarViewModel.CalendarMonths[i].Days[SecondAnalyzedDayIndex].EventsGoogleCalendar[0].Start.Hour);
            Assert.Equal(20, calendarViewModel.CalendarMonths[i].Days[SecondAnalyzedDayIndex].EventsGoogleCalendar[1].Start.Minute);
        }
    }

    #endregion

    #region UsosCalendar

    [Fact]
    public async Task UsosCalendar_WhenLocalDatabaseOnly_AssignsCorrectly()
    {
        //Arrange
        var localDatabaseManager = new LocalDatabaseManager(LocalDatabaseOptions.InMemory);
        var usosCalendarRepo = new UsosCalendarRepository(new Mock<ILocalNotificationsService>().Object, localDatabaseManager);
        usosCalendarRepo.SaveEvents(usosEventsLocal);

        CalendarViewModel calendarViewModel = new(mockUsosCalendarService.Object,
            usosCalendarRepo,
            mockGoogleCalendarService.Object,
            mockGoogleCalendarRepo.Object,
            mockApplicationService);

        //Act
        await calendarViewModel.InitAsync();

        //Assert
        for (int i = 0; i < MonthsInTotal; i++)
        {
            Assert.Equal(1, calendarViewModel.CalendarMonths[i].Days[FirstAnalyzedDayIndex].EventsUsos.Count);
            Assert.Equal(1, calendarViewModel.CalendarMonths[i].Days[SecondAnalyzedDayIndex].EventsUsos.Count);

            Assert.Equal(FirstAnalyzedDayIndex + 1, calendarViewModel.CalendarMonths[i].Days[FirstAnalyzedDayIndex].EventsUsos[0].Start.Day);
            Assert.Equal(SecondAnalyzedDayIndex + 1, calendarViewModel.CalendarMonths[i].Days[SecondAnalyzedDayIndex].EventsUsos[0].Start.Day);
        }

    }

    [Fact]
    public async Task UsosCalendar_WhenServerOnly_AssignsCorrectly()
    {
        //Arrange
        int serverEventFirstIndex = 1000;
        const int AnalyzedDayServerOffset = 10;
        var mockSeverConnectionManager = CalendarTestHelper.GetServerConnectionManagetMockForFetchingUsosCalendarEvents(FirstAnalyzedDayIndex + AnalyzedDayServerOffset,
            SecondAnalyzedDayIndex + AnalyzedDayServerOffset, serverEventFirstIndex);

        var usosCalendarService = new UsosCalendarService(mockUsosCalendarRepo.Object,
            mockSeverConnectionManager.Object,
            CalendarTestHelper.GetStudentProgrammeServiceMock().Object,
            new LocalStorageManagerMock());

        CalendarViewModel calendarViewModel = new(usosCalendarService,
            mockUsosCalendarRepo.Object,
            mockGoogleCalendarService.Object,
            mockGoogleCalendarRepo.Object,
            mockApplicationService);

        //Act
        await calendarViewModel.InitAsync();

        //Assert
        int firstDayWithExpectedEvent = FirstAnalyzedDayIndex + AnalyzedDayServerOffset + 1;
        int secondDayWithExpectedEvent = SecondAnalyzedDayIndex + AnalyzedDayServerOffset + 1;
        for (int i = 0; i < MonthsInTotal; i++)
        {
            Assert.Equal(1, calendarViewModel.CalendarMonths[i].Days[FirstAnalyzedDayIndex + AnalyzedDayServerOffset].EventsUsos.Count);
            Assert.Equal(1, calendarViewModel.CalendarMonths[i].Days[SecondAnalyzedDayIndex + AnalyzedDayServerOffset].EventsUsos.Count);

            Assert.Equal(firstDayWithExpectedEvent, calendarViewModel.CalendarMonths[i].Days[FirstAnalyzedDayIndex + AnalyzedDayServerOffset].EventsUsos[0].Start.Day);
            Assert.Equal(secondDayWithExpectedEvent, calendarViewModel.CalendarMonths[i].Days[SecondAnalyzedDayIndex + AnalyzedDayServerOffset].EventsUsos[0].Start.Day);
        }
    }

    [Fact]
    public async Task UsosCalendar_WhenLocalAndServerAndBothPresentInServerResponse_AssignsCorrectly()
    {
        //Arrange
        var localDatabaseManager = new LocalDatabaseManager(LocalDatabaseOptions.InMemory);
        var usosCalendarRepo = new UsosCalendarRepository(new Mock<ILocalNotificationsService>().Object, localDatabaseManager);
        usosCalendarRepo.SaveEvents(usosEventsLocal);

        int serverEventFirstIndex = 0;
        var mockSeverConnectionManager = CalendarTestHelper.GetServerConnectionManagetMockForFetchingUsosCalendarEvents
        (FirstAnalyzedDayIndex,
            SecondAnalyzedDayIndex,
            serverEventFirstIndex);

        var usosCalendarService = new UsosCalendarService(usosCalendarRepo,
            mockSeverConnectionManager.Object,
            CalendarTestHelper.GetStudentProgrammeServiceMock().Object,
            new LocalStorageManagerMock());

        CalendarViewModel calendarViewModel = new(usosCalendarService,
            usosCalendarRepo,
            mockGoogleCalendarService.Object,
            mockGoogleCalendarRepo.Object,
            mockApplicationService);

        //Act
        await calendarViewModel.InitAsync();

        //Assert
        for (int i = 0; i < MonthsInTotal; i++)
        {
            //local events are not present in response from server mock so they should have been removed
            Assert.Equal(1, calendarViewModel.CalendarMonths[i].Days[FirstAnalyzedDayIndex].EventsUsos.Count);
            Assert.Equal(1, calendarViewModel.CalendarMonths[i].Days[SecondAnalyzedDayIndex].EventsUsos.Count);

            int firstDayWithExpectedEvent = FirstAnalyzedDayIndex + 1;
            int secondDayWithExpectedEvent = SecondAnalyzedDayIndex + 1;
            Assert.Equal(firstDayWithExpectedEvent, calendarViewModel.CalendarMonths[i].Days[FirstAnalyzedDayIndex].EventsUsos[0].Start.Day);
            Assert.Equal(secondDayWithExpectedEvent, calendarViewModel.CalendarMonths[i].Days[SecondAnalyzedDayIndex].EventsUsos[0].Start.Day);

            //make sure that any other days don't have any events
            for (int j = 0; j < calendarViewModel.CalendarMonths[i].Days.Count; j++)
            {
                if (j == FirstAnalyzedDayIndex || j == SecondAnalyzedDayIndex)
                {
                    continue;
                }
                Assert.Empty(calendarViewModel.CalendarMonths[i].Days[j].EventsUsos);
            }
        }
    }

    [Fact]
    public async Task UsosCalendar_WhenLocalAndServerButLocalEventsNotPresentInServerResponse_AssignsCorrectly()
    {
        //Arrange
        var localDatabaseManager = new LocalDatabaseManager(LocalDatabaseOptions.InMemory);
        var usosCalendarRepo = new UsosCalendarRepository(new Mock<ILocalNotificationsService>().Object, localDatabaseManager);
        usosCalendarRepo.SaveEvents(usosEventsLocal);

        int serverEventFirstIndex = 1000;
        const int AnalyzedDayServerOffset = 10;
        var mockSeverConnectionManager = CalendarTestHelper.GetServerConnectionManagetMockForFetchingUsosCalendarEvents(FirstAnalyzedDayIndex + AnalyzedDayServerOffset,
            SecondAnalyzedDayIndex + AnalyzedDayServerOffset, serverEventFirstIndex);

        var usosCalendarService = new UsosCalendarService(usosCalendarRepo,
            mockSeverConnectionManager.Object,
            CalendarTestHelper.GetStudentProgrammeServiceMock().Object,
            new LocalStorageManagerMock());

        CalendarViewModel calendarViewModel = new(usosCalendarService,
            usosCalendarRepo,
            mockGoogleCalendarService.Object,
            mockGoogleCalendarRepo.Object,
            mockApplicationService);

        //Act
        await calendarViewModel.InitAsync();

        //Assert
        for (int i = 0; i < MonthsInTotal; i++)
        {
            //local events are not present in response from server mock so they should have been removed
            Assert.Empty(calendarViewModel.CalendarMonths[i].Days[FirstAnalyzedDayIndex].EventsUsos);
            Assert.Empty(calendarViewModel.CalendarMonths[i].Days[SecondAnalyzedDayIndex].EventsUsos);

            Assert.Equal(1, calendarViewModel.CalendarMonths[i].Days[FirstAnalyzedDayIndex + AnalyzedDayServerOffset].EventsUsos.Count);
            Assert.Equal(1, calendarViewModel.CalendarMonths[i].Days[SecondAnalyzedDayIndex + AnalyzedDayServerOffset].EventsUsos.Count);

            int firstDayWithExpectedEvent = FirstAnalyzedDayIndex + AnalyzedDayServerOffset + 1;
            int secondDayWithExpectedEvent = SecondAnalyzedDayIndex + AnalyzedDayServerOffset + 1;
            Assert.Equal(firstDayWithExpectedEvent, calendarViewModel.CalendarMonths[i].Days[FirstAnalyzedDayIndex + AnalyzedDayServerOffset].EventsUsos[0].Start.Day);
            Assert.Equal(secondDayWithExpectedEvent, calendarViewModel.CalendarMonths[i].Days[SecondAnalyzedDayIndex + AnalyzedDayServerOffset].EventsUsos[0].Start.Day);

            //make sure that any other days don't have any events
            for (int j = 0; j < calendarViewModel.CalendarMonths[i].Days.Count; j++)
            {
                if (j == FirstAnalyzedDayIndex + AnalyzedDayServerOffset || j == SecondAnalyzedDayIndex + AnalyzedDayServerOffset)
                {
                    continue;
                }
                Assert.Empty(calendarViewModel.CalendarMonths[i].Days[j].EventsUsos);
            }
        }
    }

    #endregion

}