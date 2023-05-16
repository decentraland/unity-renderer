using Cysharp.Threading.Tasks;
using DCl.Social.Friends;
using System;
using System.Threading;

namespace DCL.Social.Friends
{
    public interface ISocialApiBridge : IService
    {
        public event Action<FriendRequest> OnIncomingFriendRequestAdded;
        public event Action<FriendRequest> OnOutgoingFriendRequestAdded;
        event Action<UserStatus> OnFriendAdded;
        event Action<string> OnFriendRemoved;
        event Action<FriendRequest> OnFriendRequestAdded;
        event Action<string> OnFriendRequestRemoved;
        event Action<AddFriendRequestsPayload> OnFriendRequestsAdded;
        event Action<FriendshipUpdateStatusMessage> OnFriendshipStatusUpdated;
        event Action<FriendRequestPayload> OnFriendRequestReceived;

        UniTask<FriendshipInitializationMessage> GetInitializationInformationAsync(CancellationToken cancellationToken = default);

        UniTask RejectFriendshipAsync(string friendRequestId, CancellationToken cancellationToken = default);

        UniTask<FriendRequest> RequestFriendshipAsync(string friendUserId, string messageBody, CancellationToken cancellationToken = default);
    }
}
