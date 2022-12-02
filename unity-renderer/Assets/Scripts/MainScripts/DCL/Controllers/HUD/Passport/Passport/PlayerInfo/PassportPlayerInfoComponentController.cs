using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCL;
using DCL.Interface;
using DCL.Helpers;
using SocialFeaturesAnalytics;
using DCl.Social.Friends;

namespace DCL.Social.Passports
{
    public class PassportPlayerInfoComponentController
    {
        private IPassportPlayerInfoComponentView view;
        private readonly DataStore dataStore;
        private readonly IProfanityFilter profanityFilter;
        private readonly IFriendsController friendsController;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly ISocialAnalytics socialAnalytics;

        private UserProfile ownUserProfile => userProfileBridge.GetOwn();
        private StringVariable currentPlayerId;
        private string name;

        public PassportPlayerInfoComponentController(
            StringVariable currentPlayerId,
            IPassportPlayerInfoComponentView view,
            DataStore dataStore,
            IProfanityFilter profanityFilter,
            IFriendsController friendsController,
            IUserProfileBridge userProfileBridge,
            ISocialAnalytics socialAnalytics)
        {
            this.currentPlayerId = currentPlayerId;
            this.view = view;
            this.dataStore = dataStore;
            this.profanityFilter = profanityFilter;
            this.friendsController = friendsController;
            this.userProfileBridge = userProfileBridge;
            this.socialAnalytics = socialAnalytics;

            view.OnAddFriend += AddPlayerAsFriend;
            view.OnRemoveFriend += RemoveFriend;
            view.OnCancelFriendRequest += CancelFriendRequest;
            view.OnAcceptFriendRequest += AcceptFriendRequest;
            view.OnBlockUser += BlockUser;
            view.OnUnblockUser += UnblockUser;
            view.OnReportUser += ReportUser;
        }

        public void UpdateWithUserProfile(UserProfile userProfile) => UpdateWithUserProfileAsync(userProfile);

        private async UniTask UpdateWithUserProfileAsync(UserProfile userProfile)
        {
            name = userProfile.name;
            string filteredName = await FilterName(userProfile);
            PlayerPassportModel playerPassportModel;

            if(userProfile.isGuest)
            {
                playerPassportModel = new PlayerPassportModel()
                {
                    name = filteredName,
                    isGuest = userProfile.isGuest,
                };
            }
            else
            {
                playerPassportModel = new PlayerPassportModel()
                {
                    name = filteredName,
                    userId = userProfile.userId,
                    presenceStatus = friendsController.GetUserStatus(userProfile.userId).presence,
                    isGuest = userProfile.isGuest,
                    isBlocked = ownUserProfile.IsBlocked(userProfile.userId),
                    hasBlocked = userProfile.IsBlocked(ownUserProfile.userId),
                    friendshipStatus = friendsController.GetUserStatus(userProfile.userId).friendshipStatus
                };
            }
            view.Configure(playerPassportModel);
        }

        private async UniTask<string> FilterName(UserProfile userProfile)
        {
            return IsProfanityFilteringEnabled()
                ? await profanityFilter.Filter(userProfile.userName)
                : userProfile.userName;
        }

        private bool IsProfanityFilteringEnabled()
        {
            return dataStore.settings.profanityChatFilteringEnabled.Get();
        }

        private void AddPlayerAsFriend()
        {
            UserProfile currentUserProfile = userProfileBridge.Get(currentPlayerId);

            friendsController.RequestFriendship(currentPlayerId);
            //socialAnalytics.SendFriendRequestSent(ownUserProfile.userId, currentPlayerId, 0, PlayerActionSource.Passport);
        }

        private void RemoveFriend()
        {
            friendsController.RemoveFriend(currentPlayerId);
        }

        private void CancelFriendRequest()
        {
            friendsController.CancelRequest(currentPlayerId);
            //socialAnalytics.SendFriendRequestCancelled(ownUserProfile.userId, currentPlayerId, PlayerActionSource.Passport);
        }

        private void AcceptFriendRequest()
        {
            friendsController.AcceptFriendship(currentPlayerId);
            //socialAnalytics.SendFriendRequestApproved(ownUserProfile.userId, currentPlayerId, PlayerActionSource.Passport);
        }

        private void BlockUser()
        {
            if (ownUserProfile.IsBlocked(currentPlayerId)) return;
            ownUserProfile.Block(currentPlayerId);
            view.SetIsBlocked(true);
            WebInterface.SendBlockPlayer(currentPlayerId);
            //socialAnalytics.SendPlayerBlocked(friendsController.IsFriend(currentUserProfile.userId), PlayerActionSource.Passport);
        }

        private void UnblockUser()
        {
            if (!ownUserProfile.IsBlocked(currentPlayerId)) return;
            ownUserProfile.Unblock(currentPlayerId);
            view.SetIsBlocked(false);
            WebInterface.SendUnblockPlayer(currentPlayerId);
            //socialAnalytics.SendPlayerUnblocked(friendsController.IsFriend(currentUserProfile.userId), PlayerActionSource.Passport);
        }

        private void ReportUser()
        {
            WebInterface.SendReportPlayer(currentPlayerId, name);
            //SocialAnalytics.SendPlayerReport(PlayerReportIssueType.None, 0, PlayerActionSource.Passport);
        }
    }
}
