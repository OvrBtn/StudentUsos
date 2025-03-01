using Moq;
using StudentUsos.Features.UserInfo;
using StudentUsos.Services.ServerConnection;


namespace UnitTests.Features.UserInfo
{
    [Category(Categories.Unit_SharedBusinessLogic)]
    public class UserInfoServiceTests
    {
        [Fact]
        public async Task GetUserInfoAsync_WhenValidResponse_ReturnsUserInfo()
        {
            //Arrange
            var json = MockDataHelper.LoadFile("UserInfo.json", "UserInfo");

            var mockServerConnectionManager = new Mock<IServerConnectionManager>();
            ServerConnectionManagerTestHelper.SetupGenericSendRequestToUsos(mockServerConnectionManager, json);

            UserInfoService service = new(mockServerConnectionManager.Object);

            //Act
            var userInfo = await service.GetUserInfoAsync();

            //Assert
            Assert.NotNull(userInfo);
            Assert.Equal("131000", userInfo.Id);
            Assert.Equal("160000", userInfo.StudentNumber);
            Assert.Equal("John", userInfo.FirstName);
            Assert.Equal("Doe", userInfo.LastName);
            Assert.Equal("JD", userInfo.Initials);

        }
    }
}
