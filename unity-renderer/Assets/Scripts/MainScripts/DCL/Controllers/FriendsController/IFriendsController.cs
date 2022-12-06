using Cysharp.Threading.Tasks;
using DCl.Social.Friends;
using System;
using System.Collections.Generic;
using SocialFriendRequest = DCL.Social.Friends.FriendRequest;

namespace DCL.Social.Friends
{
    public interface IFriendsController
    {
        event Action OnInitialized;
        event Action<string, FriendshipAction> OnUpdateFriendship;
        event Action<string, UserStatus> OnUpdateUserStatus;
        event Action<string> OnFriendNotFound;
        event Action<List<FriendWithDirectMessages>> OnAddFriendsWithDirectMessages;
        event Action<int, int> OnTotalFriendRequestUpdated;
        event Action<int> OnTotalFriendsUpdated;
        event Action<SocialFriendRequest> OnAddFriendRequest;

        int AllocatedFriendCount { get; }
        bool IsInitialized { get; }
        int ReceivedRequestCount { get; }
        int TotalFriendCount { get; }
        int TotalFriendRequestCount { get; }
        int TotalReceivedFriendRequestCount { get; }
        int TotalSentFriendRequestCount { get; }
        int TotalFriendsWithDirectMessagesCount { get; }
        Dictionary<string, UserStatus> GetAllocatedFriends();
        UserStatus GetUserStatus(string userId);

        bool ContainsStatus(string friendId, FriendshipStatus status);
        UniTask<SocialFriendRequest> RequestFriendship(string friendUserId, string messageBody);
        UniTask<SocialFriendRequest> CancelRequestByUserId(string friendUserId);
        UniTask<SocialFriendRequest> CancelRequest(string friendRequestId);
        void AcceptFriendship(string friendUserId);
        void RejectFriendship(string friendUserId);
        bool IsFriend(string userId);
        void RemoveFriend(string friendId);
        void GetFriends(int limit, int skip);
        void GetFriends(string usernameOrId, int limit);
        void GetFriendRequests(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip); // TODO (NEW FRIEND REQUESTS): remove when we don't need to keep the retro-compatibility with the old version
        UniTask<List<SocialFriendRequest>> GetFriendRequestsAsync(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip);
        void GetFriendsWithDirectMessages(int limit, int skip);
        void GetFriendsWithDirectMessages(string userNameOrId, int limit);
        SocialFriendRequest GetAllocatedFriendRequest(string friendRequestId);
        SocialFriendRequest GetAllocatedFriendRequestByUser(string userId);
    }
}
