public interface IUserProfileBridge
{
    void SaveUnverifiedName(string name);
    void SaveDescription(string description);
    UserProfile GetOwn();
    void AddUserProfileToCatalog(UserProfileModel userProfileModel);
    UserProfile Get(string userId);
}