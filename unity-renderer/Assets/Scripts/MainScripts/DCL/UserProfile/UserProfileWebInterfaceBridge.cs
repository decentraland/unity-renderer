using Cysharp.Threading.Tasks;
using DCL.Interface;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;

public class UserProfileWebInterfaceBridge : IUserProfileBridge
{
    public void SaveUnverifiedName(string name) => WebInterface.SendSaveUserUnverifiedName(name);

    public void SaveDescription(string description) => WebInterface.SendSaveUserDescription(description);

    public void RequestFullUserProfile(string userId) => WebInterface.SendRequestUserProfile(userId);

    public UniTask<UserProfile> RequestFullUserProfileAsync(string userId, CancellationToken cancellationToken) =>
        UserProfileController.i.RequestFullUserProfileAsync(userId, cancellationToken);

    public UserProfile GetOwn() => UserProfile.GetOwnUserProfile();

    public UserProfile Get(string userId)
    {
        if (userId == null) return null;
        return UserProfileController.userProfilesCatalog.Get(userId);
    }

    public UserProfile GetByName(string userName, bool caseSensitive)
    {
        return UserProfileController.userProfilesCatalog.GetValues()
                                    .FirstOrDefault(p =>
                                     {
                                         if (caseSensitive)
                                             return p.userName == userName;

                                         return p.userName.Equals(userName, StringComparison.OrdinalIgnoreCase);
                                     });
    }

    public void SignUp() => WebInterface.RedirectToSignUp();

    public void SendSaveAvatar(AvatarModel avatar, Texture2D face256Snapshot, Texture2D bodySnapshot, bool isSignUpFlow = false) =>
        WebInterface.SendSaveAvatar(avatar, face256Snapshot, bodySnapshot, isSignUpFlow);
}
