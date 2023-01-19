using DCl.Social.Friends;
using System;
using SocialFeaturesAnalytics;
using DCL.Social.Friends;
using UIComponents.Scripts.Components;

namespace DCL.Social.Passports
{
    public interface IPassportPlayerInfoComponentView : IBaseComponentView<PlayerPassportModel>
    {
        event Action OnAddFriend;
        event Action OnRemoveFriend;
        event Action OnCancelFriendRequest;
        event Action OnAcceptFriendRequest;
        event Action OnBlockUser;
        event Action OnUnblockUser;
        event Action OnReportUser;
        event Action<string> OnWhisperUser;
        event Action OnJumpInUser;
        event Action<string> OnWalletCopy;
        event Action<string> OnUsernameCopy;

        void SetIsBlocked(bool isBlocked);
        void InitializeJumpInButton(IFriendsController friendsController, string userId, ISocialAnalytics socialAnalytics);
        void ResetCopyToast();
        void SetFriendStatus(FriendshipStatus status);
        void SetActionsActive(bool isActive);
    }
}
