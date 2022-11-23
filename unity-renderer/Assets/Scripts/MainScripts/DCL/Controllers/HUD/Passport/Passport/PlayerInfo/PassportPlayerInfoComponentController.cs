using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCL;
using DCL.Interface;
using DCL.Helpers;
using SocialFeaturesAnalytics;

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

        private StringVariable currentPlayerId;

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
            view.OnReportUser += ReportUser;
            view.OnUnfriendUser += UnfriendUser;
        }

        public void UpdateWithUserProfile(UserProfile userProfile) => UpdateWithUserProfileAsync(userProfile);

        private async UniTask UpdateWithUserProfileAsync(UserProfile userProfile)
        {
            string filteredName = await FilterName(userProfile);
            view.SetName(filteredName);
            view.SetGuestUser(userProfile.isGuest);
            if(!userProfile.isGuest)
            {
                view.SetWallet(userProfile.userId);
                view.SetPresence(friendsController.GetUserStatus(userProfile.userId).presence);
                view.SetFriendStatus(friendsController.GetUserStatus(userProfile.userId).friendshipStatus);
                view.InitializeJumpInButton(friendsController, userProfile.userId, socialAnalytics);
            }
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
            //if (ownUserProfile.IsBlocked(currentUserProfile.userId)) return;
            //ownUserProfile.Block(currentUserProfile.userId);
            //view.SetIsBlocked(true);
            WebInterface.SendBlockPlayer(currentPlayerId);
            //socialAnalytics.SendPlayerBlocked(friendsController.IsFriend(currentUserProfile.userId), PlayerActionSource.Passport);
        }

        private void ReportUser()
        {
            //WebInterface.SendReportPlayer(currentPlayerId, currentUserProfile?.name);
            //socialAnalytics.SendPlayerReport(PlayerReportIssueType.None, 0, PlayerActionSource.Passport);
        }

        private void UnfriendUser()
        {

        }
    }
}