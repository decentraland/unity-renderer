using Cysharp.Threading.Tasks;
using DCL.Interface;
using DCL.UserProfiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class UserProfileWebInterfaceBridge : IUserProfileBridge
{
    public UniTask<UserProfile> SaveVerifiedName(string name, CancellationToken cancellationToken) =>
        UserProfileController.i.SaveVerifiedName(name, cancellationToken);

    public UniTask<UserProfile> SaveUnverifiedName(string name, CancellationToken cancellationToken) =>
        UserProfileController.i.SaveUnverifiedName(name, cancellationToken);

    public UniTask<UserProfile> SaveDescription(string description, CancellationToken cancellationToken) =>
        UserProfileController.i.SaveDescription(description, cancellationToken);

    public UniTask<UserProfile> SaveAdditionalInfo(AdditionalInfo additionalInfo,
        CancellationToken cancellationToken) =>
        UserProfileController.i.SaveAdditionalInfo(additionalInfo, cancellationToken);

    public void RequestFullUserProfile(string userId) => WebInterface.SendRequestUserProfile(userId);

    public void RequestOwnProfileUpdate() =>
        WebInterface.RequestOwnProfileUpdate();

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

    public UniTask<UserProfile> SaveLinks(List<UserProfileModel.Link> links, CancellationToken cancellationToken) =>
        UserProfileController.i.SaveLinks(links, cancellationToken);

    public void LogOut() =>
        WebInterface.LogOut();
}
