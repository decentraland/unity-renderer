using DCL.Interface;

public class UserProfileWebInterfaceBridge : IUserProfileBridge
{
    public void SaveUnverifiedName(string name)
    {
        WebInterface.SendSaveUserUnverifiedName(name);
    }

    public void SaveDescription(string description)
    {
        WebInterface.SendSaveUserDescription(description);
    }
}