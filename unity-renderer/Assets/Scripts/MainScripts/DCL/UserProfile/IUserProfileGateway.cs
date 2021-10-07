public interface IUserProfileGateway
{
    void SaveUnverifiedName(string name);
    void SaveDescription(string description);
}