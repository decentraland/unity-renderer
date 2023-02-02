using Cysharp.Threading.Tasks;
using System.Threading;

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
}
