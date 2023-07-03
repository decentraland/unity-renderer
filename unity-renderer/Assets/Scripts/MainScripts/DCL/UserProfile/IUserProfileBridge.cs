using Cysharp.Threading.Tasks;
using DCL.UserProfiles;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public interface IUserProfileBridge
{
    UniTask<UserProfile> SaveVerifiedName(string name, CancellationToken cancellationToken);
    UniTask<UserProfile> SaveUnverifiedName(string name, CancellationToken cancellationToken);
    UniTask<UserProfile> SaveDescription(string description, CancellationToken cancellationToken);
    UniTask<UserProfile> SaveAdditionalInfo(AdditionalInfo additionalInfo, CancellationToken cancellationToken);
    void RequestFullUserProfile(string userId);
    void RequestOwnProfileUpdate();
    UniTask<UserProfile> RequestFullUserProfileAsync(string userId, CancellationToken cancellationToken = default);
    UserProfile GetOwn();
    UserProfile Get(string userId);
    UserProfile GetByName(string userName, bool caseSensitive = true);
    void SignUp();
    void SendSaveAvatar(AvatarModel avatar, Texture2D face256Snapshot, Texture2D bodySnapshot, bool isSignUpFlow = false);
    UniTask<UserProfile> SaveLinks(List<UserProfileModel.Link> links, CancellationToken cancellationToken);
    void LogOut();
}
