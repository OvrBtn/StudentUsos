using Moq;
using StudentUsos.Features.Calendar;
using StudentUsos.Features.Calendar.Repositories;
using StudentUsos.Features.Calendar.Services;
using StudentUsos.Features.StudentProgrammes.Services;
using StudentUsos.Services.ServerConnection;

namespace UnitTests.Features.Calendar;

[Category(Categories.Unit_SharedBusinessLogic)]
public class UsosCalendarServiceTests
{
    Mock<IServerConnectionManager> serverConnectionMock;
    Mock<IStudentProgrammeService> studentProgrammeServiceMock;
    LocalStorageManagerMock localStorageManagerMock = new();
    public UsosCalendarServiceTests()
    {
        serverConnectionMock = CalendarTestHelper.GetServerConnectionManagetMockForFetchingUsosCalendarEvents();
        studentProgrammeServiceMock = CalendarTestHelper.GetStudentProgrammeServiceMock();
    }

    [Fact]
    public async Task TryFetchingAvailableEventsAsync_EmptyLocalDatabase_ReturnsEvents()
    {
        //Arrange
        var usosCalendarRepoMock = new Mock<IUsosCalendarRepository>();

        var usosCalendarService = new UsosCalendarService(usosCalendarRepoMock.Object,
            serverConnectionMock.Object,
            studentProgrammeServiceMock.Object,
            localStorageManagerMock);

        //Act
        var result = await usosCalendarService.TryFetchingAvailableEventsAsync();

        //Assert
        Assert.Equal(CalendarSettings.MonthsToGetInTotal, result.Count);

        int primaryUpdateCount = 0, secondaryUpdateCount = 0;
        foreach (var item in result)
        {
            if (item.isPrimaryUpdate)
            {
                primaryUpdateCount++;
            }
            else
            {
                secondaryUpdateCount++;
            }
        }
        Assert.Equal(CalendarSettings.PrimaryUpdateMonths, primaryUpdateCount);
        Assert.Equal(CalendarSettings.SecondaryUpdateMonths, secondaryUpdateCount);

        DateTime date = new(DateTime.Now.Year, DateTime.Now.Month, 1);
        Assert.All(result, x =>
        {
            Assert.Equal(date, x.date);
            Assert.Equal(date, x.events[0].Start);
            date = date.AddMonths(1);
        });
    }

    [Fact]
    public async Task TryFetchingAvailableEventsAsync_WhenCanOnlyGetPrimaryEvents_ReturnsOnlyPrimaryEvents()
    {
        //Arrange
        var usosCalendarRepoMock = new Mock<IUsosCalendarRepository>();

        localStorageManagerMock.SetString(StudentUsos.Services.LocalStorage.LocalStorageKeys.LastSecondaryCalendarUpdate, DateTime.Now.ToString());

        var usosCalendarService = new UsosCalendarService(usosCalendarRepoMock.Object,
            serverConnectionMock.Object,
            studentProgrammeServiceMock.Object,
            localStorageManagerMock);

        //Act
        var result = await usosCalendarService.TryFetchingAvailableEventsAsync();

        //Assert
        Assert.Equal(CalendarSettings.PrimaryUpdateMonths, result.Count);

        int primaryUpdatesCount = 0, secondaryCount = 0;
        foreach (var item in result)
        {
            if (item.isPrimaryUpdate)
            {
                primaryUpdatesCount++;
            }
            else
            {
                secondaryCount++;
            }
        }
        Assert.Equal(CalendarSettings.PrimaryUpdateMonths, primaryUpdatesCount);
        Assert.Equal(0, secondaryCount);

        DateTime date = new(DateTime.Now.Year, DateTime.Now.Month, 1);
        Assert.All(result, x =>
        {
            Assert.Equal(date, x.date);
            Assert.Equal(date, x.events[0].Start);
            date = date.AddMonths(1);
        });
    }

    [Fact]
    public async Task TryFetchingAvailableEventsAsync_WhenCanOnlyGetSecondaryEvents_ReturnsOnlySecondary()
    {
        //Arrange
        var usosCalendarRepoMock = new Mock<IUsosCalendarRepository>();

        localStorageManagerMock.SetString(StudentUsos.Services.LocalStorage.LocalStorageKeys.LastPrimaryCalendarUpdate, DateTime.Now.ToString());

        var usosCalendarService = new UsosCalendarService(usosCalendarRepoMock.Object,
            serverConnectionMock.Object,
            studentProgrammeServiceMock.Object,
            localStorageManagerMock);

        //Act
        var result = await usosCalendarService.TryFetchingAvailableEventsAsync();

        //Assert
        Assert.Equal(CalendarSettings.SecondaryUpdateMonths, result.Count);

        int primaryUpdatesCount = 0, secondaryCount = 0;
        foreach (var item in result)
        {
            if (item.isPrimaryUpdate)
            {
                primaryUpdatesCount++;
            }
            else
            {
                secondaryCount++;
            }
        }
        Assert.Equal(0, primaryUpdatesCount);
        Assert.Equal(CalendarSettings.SecondaryUpdateMonths, secondaryCount);

        DateTime date = new(DateTime.Now.Year, DateTime.Now.Month, 1);
        date = date.AddMonths(CalendarSettings.PrimaryUpdateMonths);
        Assert.All(result, x =>
        {
            Assert.Equal(date, x.date);
            Assert.Equal(date, x.events[0].Start);
            date = date.AddMonths(1);
        });
    }
}