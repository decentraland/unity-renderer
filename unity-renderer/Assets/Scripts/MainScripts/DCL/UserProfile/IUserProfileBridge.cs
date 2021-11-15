public interface IUserProfileBridge
{
    void SaveUnverifiedName(string name);
    void SaveDescription(string description);
    UserProfile GetOwn();
}