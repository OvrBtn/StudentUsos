using Moq;
using StudentUsos.Features.Calendar.Models;
using StudentUsos.Features.Calendar.Repositories;
using StudentUsos.Services.LocalDatabase;
using StudentUsos.Services.LocalNotifications;

namespace UnitTests.Features.Calendar
{
    [Category(Categories.Unit_SharedBusinessLogic)]
    public class UsosCalendarRepositoryTests
    {
        [Fact]
        public async Task ScheduleEvents_WhenValidEvents_SchedulesNotifications()
        {
            //Arrange
            List<UsosCalendarEvent> events = new();
            for (int i = 0; i < 3; i++)
            {
                events.Add(new()
                {
                    NameJson = "name",
                    TypeJson = "type",
                    Start = DateTime.Now.Date.AddDays(i + 5).AddHours(12),
                    End = DateTime.Now.Date.AddDays(i + 5).AddHours(14)
                });
            }

            List<LocalNotification> scheduledNotifications = new();
            int notificationId = 0;
            var localNotificationServiceMock = new Mock<ILocalNotificationsService>();
            localNotificationServiceMock.Setup(x => x.ScheduleNotificationAsync(It.IsAny<LocalNotification>()))
                .Callback<LocalNotification>(x => scheduledNotifications.Add(x))
                .ReturnsAsync(() =>
                {
                    return notificationId++;
                });

            var usosCalendarRepo = new UsosCalendarRepository(localNotificationServiceMock.Object, new Mock<ILocalDatabaseManager>().Object);

            //Act
            int notifyBeforeDays = 2;
            TimeSpan time = new(16, 39, 0);
            await usosCalendarRepo.ScheduleNotificationsAsync(events, notifyBeforeDays, time);

            //Assert
            Assert.Equal(events.Count, scheduledNotifications.Count);
            for (int i = 0; i < events.Count; i++)
            {
                DateTime expected = new(DateOnly.FromDateTime(events[i].Start.AddDays(notifyBeforeDays * -1)), TimeOnly.FromTimeSpan(time));
                Assert.Equal(expected, scheduledNotifications[i].ScheduledDateTime);
                Assert.Equal(events[i].Name, scheduledNotifications[i].Title);
                Assert.Equal(i, events[i].NotificationId);
            }
        }

        [Fact]
        public async Task ScheduleEvents_WhenPastEvents_DoesNotScheduleNotifications()
        {
            //Arrange
            List<UsosCalendarEvent> events = new();
            for (int i = 0; i < 3; i++)
            {
                events.Add(new()
                {
                    NameJson = "name",
                    TypeJson = "type",
                    Start = DateTime.Now.Date.AddDays(i - 5).AddHours(12),
                    End = DateTime.Now.Date.AddDays(i - 5).AddHours(14)
                });
            }

            List<LocalNotification> scheduledNotifications = new();
            int notificationId = 0;
            var localNotificationServiceMock = new Mock<ILocalNotificationsService>();
            localNotificationServiceMock.Setup(x => x.ScheduleNotificationAsync(It.IsAny<LocalNotification>()))
                .Callback<LocalNotification>(x => scheduledNotifications.Add(x))
                .ReturnsAsync(() =>
                {
                    return notificationId++;
                });

            var usosCalendarRepo = new UsosCalendarRepository(localNotificationServiceMock.Object, new Mock<ILocalDatabaseManager>().Object);

            //Act
            int notifyBeforeDays = 2;
            TimeSpan time = new(16, 39, 0);
            await usosCalendarRepo.ScheduleNotificationsAsync(events, notifyBeforeDays, time);

            //Assert
            Assert.Empty(scheduledNotifications);
        }

        [Fact]
        public async Task SaveEventsFromServerAndHandleLocalNotifications_WhenNewAndOldEvents_SchedulesNotificationsAndInsertToDatabase()
        {
            //Arrange
            var localDatabaseManager = new LocalDatabaseManager(LocalDatabaseOptions.InMemory);

            var nowDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 10).AddMonths(1);

            string defaultTitle = "{\"pl\":\"name\",\"en\":\"name\"}";
            List<UsosCalendarEvent> databaseEvents = new();
            for (int i = 0; i < 4; i++)
            {
                databaseEvents.Add(new()
                {
                    Id = i,
                    NameJson = defaultTitle,
                    TypeJson = "type",
                    isPrimaryUpdate = true,
                    Start = nowDate.Date.AddDays(i + 5).AddHours(12),
                    End = nowDate.Date.AddDays(i + 5).AddHours(14)
                });
            }
            localDatabaseManager.InsertAll(databaseEvents);

            List<UsosCalendarEvent> newEvents = new();
            for (int i = 0; i < 4; i++)
            {
                if (i < 2)
                {
                    newEvents.Add(GeneralTestHelper.DeepCopyReflection(databaseEvents[i]));
                }
                else
                {
                    newEvents.Add(databaseEvents[i]);
                }
            }
            string newTitle = "{\"pl\":\"new name\",\"en\":\"new name\"}";
            newEvents[0].NameJson = newTitle;
            newEvents[0].Id = 5;
            newEvents[1].Start = nowDate.Date.AddDays(10).AddHours(16);
            newEvents[1].Id = 6;

            List<LocalNotification> scheduledNotifications = new();
            int notificationId = 0;
            var localNotificationServiceMock = new Mock<ILocalNotificationsService>();
            localNotificationServiceMock.Setup(x => x.ScheduleNotificationAsync(It.IsAny<LocalNotification>()))
                .Callback<LocalNotification>(x => scheduledNotifications.Add(x))
                .ReturnsAsync(() =>
                {
                    return notificationId++;
                });

            var usosCalendarRepository = new UsosCalendarRepository(localNotificationServiceMock.Object, localDatabaseManager);

            //Act
            var results = await usosCalendarRepository.SaveEventsFromServerAndHandleLocalNotificationsAsync(nowDate.Year, nowDate.Month, newEvents, true);


            //Assert
            Assert.Equal(2, results.localExceptServer.Count);
            Assert.Equal(2, results.serverExceptLocal.Count);
            Assert.Equal(2, scheduledNotifications.Count);

            Assert.Equal(0, results.localExceptServer[0].Id);
            Assert.Equal(1, results.localExceptServer[1].Id);

            //ids 2 and 3 should have been first saved to database and then removed from it when they were not present in events from server
            Assert.Equal(5, results.serverExceptLocal[0].Id);
            Assert.Equal(6, results.serverExceptLocal[1].Id);

            Assert.Equal(2, scheduledNotifications.Count);
            Assert.Equal(newEvents[0].Name, scheduledNotifications[0].Title);
            Assert.Equal(databaseEvents[0].Name, scheduledNotifications[1].Title);

            var allFromLocal = localDatabaseManager.GetAll<UsosCalendarEvent>();
            Assert.Equal(databaseEvents.Count, allFromLocal.Count);
        }

        [Fact]
        public async Task SaveEventsFromServerAndHandleLocalNotifications_WhenLongEvents_DoesNotScheduleDuplicates()
        {
            //Arrange
            var nowDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 10).AddMonths(1);
            List<UsosCalendarEvent> serverEvents = new();
            const int EventsCount = 5;
            for (int i = 0; i < EventsCount; i++)
            {
                serverEvents.Add(new()
                {
                    Id = i,
                    NameJson = "{\"pl\":\"name" + i + "\",\"en\":\"name" + i + "\"}",
                    TypeJson = "{\"pl\":\"type\",\"en\":\"type\"}",
                    isPrimaryUpdate = true,
                    Start = nowDate.Date.AddDays(i + 5),
                    End = nowDate.Date.AddDays(i + 5).AddMonths(i)
                });
            }

            var localDatabaseManagerMock = new Mock<ILocalDatabaseManager>();
            localDatabaseManagerMock.Setup(x => x.GetAll(It.IsAny<Func<UsosCalendarEvent, bool>>()))
                .Returns(new List<UsosCalendarEvent>());

            List<LocalNotification> scheduledNotifications = new();
            int notificationId = 0;
            var localNotificationServiceMock = new Mock<ILocalNotificationsService>();
            localNotificationServiceMock.Setup(x => x.ScheduleNotificationAsync(It.IsAny<LocalNotification>()))
                .Callback<LocalNotification>(x => scheduledNotifications.Add(x))
                .ReturnsAsync(() =>
                {
                    return notificationId++;
                });

            var usosCalendarRepository = new UsosCalendarRepository(localNotificationServiceMock.Object, localDatabaseManagerMock.Object);

            //Act
            var results = await usosCalendarRepository.SaveEventsFromServerAndHandleLocalNotificationsAsync(nowDate.Year, nowDate.Month, serverEvents, true);

            //Assert
            Assert.Equal(EventsCount, scheduledNotifications.Count);
        }
    }
}
