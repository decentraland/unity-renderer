using Cysharp.Threading.Tasks;
using System.Threading;

public interface IUserProfileBridge
{
    void SaveUnverifiedName(string name);
    void SaveDescription(string description);
    UniTask<UserProfile> RequestFullUserProfileAsync(string userId, CancellationToken cancellationToken = default);
    UserProfile GetOwn();
    UserProfile Get(string userId);
    UserProfile GetByName(string userNameOrId);
    void SignUp();
}
