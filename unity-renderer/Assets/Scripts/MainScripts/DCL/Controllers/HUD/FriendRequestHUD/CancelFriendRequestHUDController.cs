using Cysharp.Threading.Tasks;
using DCl.Social.Friends;
using SocialFeaturesAnalytics;
using System;
using System.Threading;
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

        private CancellationTokenSource cancellationToken = new ();
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

            dataStore.HUDs.openSentFriendRequestDetail.OnChange += ShowOrHide;
            view.OnCancel += Cancel;
            view.OnClose += Hide;
            view.OnOpenProfile += OpenProfile;
            view.Close();
        }

        public void Dispose()
        {
            dataStore.HUDs.openSentFriendRequestDetail.OnChange -= ShowOrHide;
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

            var recipientProfile = userProfileBridge.Get(friendRequest.To);

            if (recipientProfile != null)
            {
                view.SetRecipientName(recipientProfile.userName);
                view.SetRecipientProfilePicture(recipientProfile.snapshotObserver);
            }
            else
                Debug.LogError($"Cannot display user profile {friendRequest.To}, is not allocated");

            var ownProfile = userProfileBridge.GetOwn();
            view.SetSenderProfilePicture(ownProfile.snapshotObserver);

            view.Show();
        }

        private void Cancel()
        {
            cancellationToken?.Cancel();
            cancellationToken = new CancellationTokenSource();
            CancelAsync(cancellationToken.Token).Forget();
        }

        private async UniTask CancelAsync(CancellationToken cancellationToken = default)
        {
            view.ShowPendingToCancel();

            try
            {
                await friendsController.CancelRequest(friendRequestId)
                                       .Timeout(TimeSpan.FromSeconds(10));
                if (cancellationToken.IsCancellationRequested) return;

                // TODO: send analytics

                view.Close();
            }
            catch (Exception)
            {
                if (cancellationToken.IsCancellationRequested) return;
                // TODO: track error to analytics
                view.ShowCancelFailed();
                throw;
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
