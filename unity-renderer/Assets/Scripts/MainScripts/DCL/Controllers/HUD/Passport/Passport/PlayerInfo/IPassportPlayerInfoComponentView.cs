using System;
using UnityEngine;
using SocialFeaturesAnalytics;

namespace DCL.Social.Passports
{
    public interface IPassportPlayerInfoComponentView
    {
        event Action OnAddFriend;
        event Action OnRemoveFriend;
        event Action OnCancelFriendRequest;
        event Action OnAcceptFriendRequest;
        event Action OnBlockUser;
        event Action OnUnblockUser;
        event Action OnReportUser;

        void SetName(string name);
        void SetWallet(string wallet);
        void SetPresence(PresenceStatus status);
        void SetGuestUser(bool isGuest);
        void SetIsBlocked(bool isBlocked);
        void SetHasBlockedOwnUser(bool hasBlocked);
        void SetFriendStatus(FriendshipStatus friendStatus);
        void InitializeJumpInButton(IFriendsController friendsController, string userId, ISocialAnalytics socialAnalytics);
    }
}