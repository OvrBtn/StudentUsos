namespace StudentUsos.Features.UserInfo;

public interface IUserInfoRepository
{
    public UserInfo? GetUserInfo();
    public void SaveUserInfo(UserInfo userInfo);
}