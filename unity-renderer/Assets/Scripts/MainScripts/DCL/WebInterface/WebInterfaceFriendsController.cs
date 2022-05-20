using System;
using System.Collections.Generic;

namespace DCL.Interface
{
    // TODO: remove this class when WebInterface assmebly removes the FriendController dependency
    public class WebInterfaceFriendsController : IFriendsController
    {
        private readonly IFriendsController friendsController;
        
        public event Action OnInitialized
        {
            add => friendsController.OnInitialized += value;
            remove => friendsController.OnInitialized -= value;
        }
        
        public event Action<string, FriendshipAction> OnUpdateFriendship
        {
            add => friendsController.OnUpdateFriendship += value;
            remove => friendsController.OnUpdateFriendship -= value;
        }
        
        public event Action<string, FriendsController.UserStatus> OnUpdateUserStatus
        {
            add => friendsController.OnUpdateUserStatus += value;
            remove => friendsController.OnUpdateUserStatus -= value;
        }
        
        public event Action<string> OnFriendNotFound
        {
            add => friendsController.OnFriendNotFound += value;
            remove => friendsController.OnFriendNotFound -= value;
        }
        
        public int friendCount => friendsController.friendCount;
        public bool isInitialized => friendsController.isInitialized;

        public WebInterfaceFriendsController(IFriendsController friendsController)
        {
            this.friendsController = friendsController;
        }

        public Dictionary<string, FriendsController.UserStatus> GetFriends() => friendsController.GetFriends();

        public FriendsController.UserStatus GetUserStatus(string userId) => friendsController.GetUserStatus(userId);

        public bool ContainsStatus(string friendId, FriendshipStatus status) =>
            friendsController.ContainsStatus(friendId, status);

        public void RequestFriendship(string friendUserId)
        {
            friendsController.RequestFriendship(friendUserId);
            WebInterface.UpdateFriendshipStatus(new FriendsController.FriendshipUpdateStatusMessage
            {
                userId = friendUserId,
                action = FriendshipAction.REQUESTED_TO
            });
        }

        public void CancelRequest(string friendUserId)
        {
            friendsController.CancelRequest(friendUserId);
            WebInterface.UpdateFriendshipStatus(new FriendsController.FriendshipUpdateStatusMessage
            {
                userId = friendUserId,
                action = FriendshipAction.CANCELLED
            });
        }

        public void AcceptFriendship(string friendUserId)
        {
            friendsController.AcceptFriendship(friendUserId);
            WebInterface.UpdateFriendshipStatus(new FriendsController.FriendshipUpdateStatusMessage
            {
                userId = friendUserId,
                action = FriendshipAction.APPROVED
            });
        }

        public void RejectFriendship(string friendUserId)
        {
            friendsController.RejectFriendship(friendUserId);
            WebInterface.UpdateFriendshipStatus(new FriendsController.FriendshipUpdateStatusMessage
            {
                userId = friendUserId,
                action = FriendshipAction.REJECTED
            });
        }

        public bool IsFriend(string userId) => friendsController.IsFriend(userId);

        public void RemoveFriend(string friendId)
        {
            friendsController.RemoveFriend(friendId);
            WebInterface.UpdateFriendshipStatus(new FriendsController.FriendshipUpdateStatusMessage
            {
                userId = friendId,
                action = FriendshipAction.DELETED
            });
        }
    }
}