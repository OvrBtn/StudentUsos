namespace StudentUsos.Features.UserInfo
{
    public class UserInfoRepository : IUserInfoRepository
    {
        ILocalDatabaseManager localDatabaseManager;
        public UserInfoRepository(ILocalDatabaseManager localDatabaseManager)
        {
            this.localDatabaseManager = localDatabaseManager;
        }

        public UserInfo? GetUserInfo()
        {
            return localDatabaseManager.GetAll<UserInfo>().FirstOrDefault();
        }

        public void SaveUserInfo(UserInfo userInfo)
        {
            localDatabaseManager.ClearTable<UserInfo>();
            localDatabaseManager.Insert(userInfo);
        }
    }
}
