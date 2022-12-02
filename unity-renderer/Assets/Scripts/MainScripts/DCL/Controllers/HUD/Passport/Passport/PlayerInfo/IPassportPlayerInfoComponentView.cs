using DCl.Social.Friends;
using System;
using SocialFeaturesAnalytics;
using DCL.Social.Friends;

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

        void Configure(PlayerPassportModel newModel);
        void SetIsBlocked(bool isBlocked);
    }
}
