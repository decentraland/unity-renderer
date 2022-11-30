using Cysharp.Threading.Tasks;
using DCl.Social.Friends;
using SocialFeaturesAnalytics;
using System;
using UnityEngine;

namespace DCL.Social.Friends
{
    public class CancelFriendRequestHUDController
    {
        private readonly ICancelFriendRequestHUDView view;
        private readonly DataStore dataStore;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly IFriendsController friendsController;
        private readonly ISocialAnalytics socialAnalytics;
        private readonly StringVariable openPassportVariable;

        private string friendRequestId;

        public CancelFriendRequestHUDController(
            ICancelFriendRequestHUDView view,
            DataStore dataStore,
            IUserProfileBridge userProfileBridge,
            IFriendsController friendsController,
            ISocialAnalytics socialAnalytics,
            StringVariable openPassportVariable)
        {
            this.view = view;
            this.dataStore = dataStore;
            this.userProfileBridge = userProfileBridge;
            this.friendsController = friendsController;
            this.socialAnalytics = socialAnalytics;
            this.openPassportVariable = openPassportVariable;

            dataStore.HUDs.cancelFriendRequest.OnChange += ShowOrHide;
            view.OnCancel += Cancel;
            view.OnClose += Hide;
            view.OnOpenProfile += OpenProfile;
            view.Close();
        }

        public void Dispose()
        {
            dataStore.HUDs.cancelFriendRequest.OnChange -= ShowOrHide;
            view.OnCancel -= Cancel;
            view.OnClose -= Hide;
            view.OnOpenProfile += OpenProfile;
            view.Dispose();
        }

        private void ShowOrHide(string current, string previous)
        {
            if (string.IsNullOrEmpty(current))
                Hide();
            else
                Show(current);
        }

        private void Show(string friendRequestId)
        {
            this.friendRequestId = friendRequestId;
            FriendRequest friendRequest = friendsController.GetAllocatedFriendRequest(this.friendRequestId);

            if (friendRequest == null)
            {
                Debug.LogError($"Cannot display friend request {friendRequestId}, is not allocated");
                return;
            }

            view.SetBodyMessage(friendRequest.MessageBody);
            view.SetTimestamp(DateTimeOffset.FromUnixTimeMilliseconds(friendRequest.Timestamp).DateTime);

            var userProfile = userProfileBridge.Get(friendRequest.To);

            if (userProfile != null)
            {
                view.SetName(userProfile.userName);
                view.SetProfilePicture(userProfile.snapshotObserver);
            }
            else
                Debug.LogError($"Cannot display user profile {friendRequest.To}, is not allocated");

            view.Show();
        }

        private void Cancel() =>
            CancelAsync().Forget();

        private async UniTaskVoid CancelAsync()
        {
            view.ShowPendingToCancel();

            try
            {
                await friendsController.CancelRequest(friendRequestId)
                                       .Timeout(TimeSpan.FromSeconds(10));

                // TODO: send analytics

                view.Close();
            }
            catch (Exception e)
            {
                // TODO: track error to analytics
                Debug.LogException(e);
                view.ShowCancelFailed();
            }
        }

        private void Hide()
        {
            dataStore.HUDs.sendFriendRequest.Set(null, false);
            view.Close();
        }

        private void OpenProfile()
        {
            FriendRequest friendRequest = friendsController.GetAllocatedFriendRequest(friendRequestId);

            if (friendRequest == null)
            {
                Debug.LogError($"Cannot open passport {friendRequestId}, is not allocated");
                return;
            }

            openPassportVariable.Set(friendRequest.To);
        }
    }
}
