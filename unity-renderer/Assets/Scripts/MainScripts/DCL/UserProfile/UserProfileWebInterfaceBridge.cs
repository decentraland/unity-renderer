using Cysharp.Threading.Tasks;
using DCL.Interface;
using System.Linq;
using System.Threading;

public class UserProfileWebInterfaceBridge : IUserProfileBridge
{
    public void SaveUnverifiedName(string name) => WebInterface.SendSaveUserUnverifiedName(name);

    public void SaveDescription(string description) => WebInterface.SendSaveUserDescription(description);

    public void RequestFullUserProfile(string userId) => WebInterface.SendRequestUserProfile(userId);

    public UniTask<UserProfile> RequestFullUserProfileAsync(string userId, CancellationToken cancellationToken) =>
        UserProfileController.i.RequestFullUserProfileAsync(userId, cancellationToken);

    public UserProfile GetOwn() => UserProfile.GetOwnUserProfile();

    public void AddUserProfileToCatalog(UserProfileModel userProfileModel)
    {
        UserProfileController.i.AddUserProfileToCatalog(userProfileModel);
    }

    public UserProfile Get(string userId)
    {
        if (userId == null) return null;
        return UserProfileController.userProfilesCatalog.Get(userId);
    }

    public UserProfile GetByName(string userNameOrId)
    {
        return UserProfileController.userProfilesCatalog.GetValues().FirstOrDefault(p => p.userName == userNameOrId);
    }

    public void SignUp() => WebInterface.RedirectToSignUp();
}
