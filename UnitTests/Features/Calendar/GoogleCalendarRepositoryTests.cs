using Moq;
using StudentUsos.Features.Calendar.Models;
using StudentUsos.Features.Calendar.Repositories;
using StudentUsos.Services.LocalDatabase;
using StudentUsos.Services.LocalNotifications;

namespace UnitTests.Features.Calendar
{
    [Category(Categories.Unit_SharedBusinessLogic)]
    public class GoogleCalendarRepositoryTests
    {
        [Fact]
        public async Task ScheduleEvents_WhenValidEvents_SchedulesNotifications()
        {
            //Arrange
            List<GoogleCalendarEvent> events = new();
            for (int i = 0; i < 3; i++)
            {
                events.Add(new()
                {
                    CalendarName = "calendar name",
                    Title = "title",
                    Description = "desc",
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

            var googleCalendarRepo = new GoogleCalendarRepository(localNotificationServiceMock.Object, new Mock<ILocalDatabaseManager>().Object);

            //Act
            int notifyBeforeDays = 2;
            TimeSpan time = new(16, 39, 0);
            await googleCalendarRepo.ScheduleNotificationsAsync(events, notifyBeforeDays, time);

            //Assert
            Assert.Equal(events.Count, scheduledNotifications.Count);
            for (int i = 0; i < events.Count; i++)
            {
                DateTime expected = new(DateOnly.FromDateTime(events[i].Start.AddDays(notifyBeforeDays * -1)), TimeOnly.FromTimeSpan(time));
                Assert.Equal(expected, scheduledNotifications[i].ScheduledDateTime);
                Assert.Equal(events[i].Title, scheduledNotifications[i].Title);
                Assert.Equal(i, events[i].NotificationId);
            }
        }

        [Fact]
        public async Task ScheduleEvents_WhenPastEvents_DoesNotScheduleNotifications()
        {
            //Arrange
            List<GoogleCalendarEvent> events = new();
            for (int i = 0; i < 3; i++)
            {
                events.Add(new()
                {
                    CalendarName = "calendar name",
                    Title = "title",
                    Description = "desc",
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

            var googleCalendarRepo = new GoogleCalendarRepository(localNotificationServiceMock.Object, new Mock<ILocalDatabaseManager>().Object);

            //Act
            int notifyBeforeDays = 2;
            TimeSpan time = new(16, 39, 0);
            await googleCalendarRepo.ScheduleNotificationsAsync(events, notifyBeforeDays, time);

            //Assert
            Assert.Empty(scheduledNotifications);
        }

        [Fact]
        public async Task SaveEventsFromServerAndHandleLocalNotifications_WhenNewAndOldEvents_SchedulesNotificationsAndInsertToDatabase()
        {
            //Arrange
            var localDatabaseManager = new LocalDatabaseManager(LocalDatabaseOptions.InMemory);

            string calendarName = "calendar";
            var googleCalendar = new GoogleCalendar()
            {
                Name = calendarName
            };
            localDatabaseManager.Insert(googleCalendar);

            string defaultTitle = "title";
            List<GoogleCalendarEvent> databaseEvents = new();
            for (int i = 0; i < 4; i++)
            {
                databaseEvents.Add(new()
                {
                    CalendarName = calendarName,
                    Title = defaultTitle,
                    Description = "desc",
                    Start = DateTime.Now.Date.AddDays(i + 5).AddHours(12),
                    End = DateTime.Now.Date.AddDays(i + 5).AddHours(14)
                });
            }
            localDatabaseManager.InsertAll(databaseEvents);

            List<GoogleCalendarEvent> newEvents = new();
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
            string newTitle = "new title";
            newEvents[0].Title = newTitle;
            newEvents[1].Start = DateTime.Now.Date.AddDays(10).AddHours(16);

            List<LocalNotification> scheduledNotifications = new();
            int notificationId = 0;
            var localNotificationServiceMock = new Mock<ILocalNotificationsService>();
            localNotificationServiceMock.Setup(x => x.ScheduleNotificationAsync(It.IsAny<LocalNotification>()))
                .Callback<LocalNotification>(x => scheduledNotifications.Add(x))
                .ReturnsAsync(() =>
                {
                    return notificationId++;
                });

            var googleCalendarRepository = new GoogleCalendarRepository(localNotificationServiceMock.Object, localDatabaseManager);

            //Act
            var results = await googleCalendarRepository.SaveEventsFromServerAndHandleLocalNotificationsAsync(newEvents);


            //Assert
            Assert.Equal(2, results.localExceptServer.Count);
            Assert.Equal(2, results.serverExceptLocal.Count);
            Assert.Equal(2, scheduledNotifications.Count);

            Assert.Equal(1, results.localExceptServer[0].Id);
            Assert.Equal(2, results.localExceptServer[1].Id);

            //ids 3 and 4 should have been first saved to database and then removed from it when they were not present in events from server
            Assert.Equal(5, results.serverExceptLocal[0].Id);
            Assert.Equal(6, results.serverExceptLocal[1].Id);

            Assert.Equal(2, scheduledNotifications.Count);
            Assert.Equal(newTitle, scheduledNotifications[0].Title);
            Assert.Equal(defaultTitle, scheduledNotifications[1].Title);

            var allFromLocal = localDatabaseManager.GetAll<GoogleCalendarEvent>();
            Assert.Equal(databaseEvents.Count, allFromLocal.Count);
        }
    }
}
