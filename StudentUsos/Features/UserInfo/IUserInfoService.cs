namespace StudentUsos.Features.UserInfo;

public interface IUserInfoService
{
    public Task<UserInfo?> GetUserInfoAsync();
}