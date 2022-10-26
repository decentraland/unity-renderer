using System.Linq;
using DCL.Interface;

public class UserProfileWebInterfaceBridge : IUserProfileBridge
{
    public void SaveUnverifiedName(string name) => WebInterface.SendSaveUserUnverifiedName(name);

    public void SaveDescription(string description) => WebInterface.SendSaveUserDescription(description);

    public UserProfile GetOwn() => UserProfile.GetOwnUserProfile();
    
    public void AddUserProfileToCatalog(UserProfileModel userProfileModel)
    {
        UserProfileController.i.AddUserProfileToCatalog(userProfileModel);
    }

    public UserProfile Get(string userId)
    {
        return UserProfileController.userProfilesCatalog.Get(userId);
    }

    public UserProfile GetByName(string userNameOrId)
    {
        return UserProfileController.userProfilesCatalog.GetValues().FirstOrDefault(p => p.userName == userNameOrId);
    }

    public void SignUp() => WebInterface.RedirectToSignUp();
}