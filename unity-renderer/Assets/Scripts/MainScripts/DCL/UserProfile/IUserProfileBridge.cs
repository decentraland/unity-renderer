using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public interface IUserProfileBridge
{
    void SaveUnverifiedName(string name);
    void SaveDescription(string description);
    void RequestFullUserProfile(string userId);
    UniTask<UserProfile> RequestFullUserProfileAsync(string userId, CancellationToken cancellationToken = default);
    UserProfile GetOwn();
    UserProfile Get(string userId);
    UserProfile GetByName(string userNameOrId);
    void SignUp();
    void SendSaveAvatar(AvatarModel avatar, Texture2D face256Snapshot, Texture2D bodySnapshot, bool isSignUpFlow = false);
}
