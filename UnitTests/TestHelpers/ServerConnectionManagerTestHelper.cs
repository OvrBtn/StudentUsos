using Moq;
using StudentUsos.Services.ServerConnection;

namespace UnitTests.TestHelpers
{
    public static class ServerConnectionManagerTestHelper
    {
        public static void SetupGenericSendRequestToUsos(Mock<IServerConnectionManager> mock, string returns)
        {
            mock.Setup(mock => mock.SendRequestToUsosAsync(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(),
                It.IsAny<Action<string>>(),
                It.IsAny<int>()))
                .ReturnsAsync(returns);
        }
    }
}
