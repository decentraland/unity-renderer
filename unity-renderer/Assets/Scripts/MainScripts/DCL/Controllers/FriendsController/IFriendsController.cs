using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DCL.Social.Friends;

namespace DCl.Social.Friends
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
        UniTask<FriendRequest> RequestFriendship(string friendUserId, string messageBody);
        void CancelRequest(string friendUserId);
        void AcceptFriendship(string friendUserId);
        void RejectFriendship(string friendUserId);
        bool IsFriend(string userId);
        void RemoveFriend(string friendId);
        void GetFriends(int limit, int skip);
        void GetFriends(string usernameOrId, int limit);
        UniTask<List<FriendRequest>> GetFriendRequests(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip);
        void GetFriendsWithDirectMessages(int limit, int skip);
        void GetFriendsWithDirectMessages(string userNameOrId, int limit);
    }
}
