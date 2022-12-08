using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace DCL.Social.Friends
{
    public class ReceivedFriendRequestHUDController
    {
        private readonly DataStore dataStore;
        private readonly IReceivedFriendRequestHUDView view;
        private readonly IFriendsController friendsController;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly StringVariable openPassportVariable;

        private string friendRequestId;

        public ReceivedFriendRequestHUDController(DataStore dataStore,
            IReceivedFriendRequestHUDView view,
            IFriendsController friendsController,
            IUserProfileBridge userProfileBridge,
            StringVariable openPassportVariable)
        {
            this.dataStore = dataStore;
            this.view = view;
            this.friendsController = friendsController;
            this.userProfileBridge = userProfileBridge;
            this.openPassportVariable = openPassportVariable;

            view.OnClose += Hide;
            view.OnOpenProfile += OpenProfile;
            view.OnRejectFriendRequest += Reject;
            view.OnConfirmFriendRequest += Confirm;

            dataStore.HUDs.openReceivedFriendRequestDetail.OnChange += ShowOrHide;
        }

        public void Dispose()
        {
            dataStore.HUDs.openReceivedFriendRequestDetail.OnChange -= ShowOrHide;
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

        private void Hide()
        {
            dataStore.HUDs.openReceivedFriendRequestDetail.Set(null, false);
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

        private void Reject() =>
            RejectAsync().Forget();

        private async UniTaskVoid RejectAsync(CancellationToken cancellationToken = default)
        {
            view.ShowPendingToReject();

            try
            {
                await friendsController.RejectFriendshipAsync(friendRequestId)
                                       .Timeout(TimeSpan.FromSeconds(10));
                if (cancellationToken.IsCancellationRequested) return;

                // TODO: send analytics

                view.Close();
            }
            catch (Exception)
            {
                if (cancellationToken.IsCancellationRequested) return;
                // TODO: track error to analytics
                view.ShowRejectFailed();
                throw;
            }
        }

        private void Confirm() =>
            ConfirmAsync().Forget();

        private async UniTaskVoid ConfirmAsync(CancellationToken cancellationToken = default)
        {
            view.ShowPendingToConfirm();

            try
            {
                await friendsController.AcceptFriendshipAsync(friendRequestId)
                                       .Timeout(TimeSpan.FromSeconds(10));
                if (cancellationToken.IsCancellationRequested) return;

                // TODO: send analytics

                view.Close();
            }
            catch (Exception)
            {
                if (cancellationToken.IsCancellationRequested) return;
                // TODO: track error to analytics
                view.ShowAcceptFailed();
                throw;
            }
        }
    }
}
