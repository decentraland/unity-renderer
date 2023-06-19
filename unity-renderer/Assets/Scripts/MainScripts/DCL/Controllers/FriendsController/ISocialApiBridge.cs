using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace DCL.Social.Friends
{
    public interface ISocialApiBridge : IService
    {
        public event Action<FriendRequest> OnIncomingFriendRequestAdded;
        public event Action<FriendRequest> OnOutgoingFriendRequestAdded;
        event Action<string> OnFriendRequestAccepted;
        event Action<string> OnFriendRequestRejected;
        event Action<string> OnDeletedByFriend;
        event Action<string> OnFriendRequestCanceled;

        UniTask<AllFriendsInitializationMessage> GetInitializationInformationAsync(CancellationToken cancellationToken = default);

        UniTask RejectFriendshipAsync(string friendId, CancellationToken cancellationToken = default);

        UniTask CancelFriendshipAsync(string friendId, CancellationToken cancellationToken = default);

        UniTask AcceptFriendshipAsync(string friendId, CancellationToken cancellationToken = default);

        UniTask DeleteFriendshipAsync(string friendId, CancellationToken cancellationToken = default);

        UniTask<FriendRequest> RequestFriendshipAsync(string friendId, string messageBody, CancellationToken cancellationToken = default);
    }
}
