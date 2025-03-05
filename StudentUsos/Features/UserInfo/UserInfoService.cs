using StudentUsos.Services.ServerConnection;
using System.Text.Json;

namespace StudentUsos.Features.UserInfo
{
    public class UserInfoService : IUserInfoService
    {
        IServerConnectionManager serverConnectionManager;
        public UserInfoService(IServerConnectionManager serverConnectionManager)
        {
            this.serverConnectionManager = serverConnectionManager;
        }

        public async Task<UserInfo?> GetUserInfoAsync()
        {
            var webRequestArguments = new Dictionary<string, string> { { "fields", "id|first_name|last_name|student_number|library_card_id" } };
            var userData = await serverConnectionManager.SendRequestToUsosAsync("services/users/user", webRequestArguments);
            if (userData is null)
            {
                return null;
            }
            var userInfo = JsonSerializer.Deserialize(userData, UserInfoJsonContext.Default.UserInfo);
            return userInfo;
        }
    }
}
