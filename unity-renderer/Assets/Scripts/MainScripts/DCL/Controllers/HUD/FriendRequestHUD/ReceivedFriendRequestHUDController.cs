using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace DCL.Social.Friends
{
    public class ReceivedFriendRequestHUDController
    {
        private const int TIME_MS_BEFORE_SUCCESS_SCREEN_CLOSING = 3000;
        private const string PROCESS_REQUEST_ERROR_MESSAGE = "There was an error while trying to process your request. Please try again.";

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
            view.Close();

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

            var recipientProfile = userProfileBridge.Get(friendRequest.From);

            if (recipientProfile != null)
            {
                view.SetSenderName(recipientProfile.userName);
                view.SetSenderProfilePicture(recipientProfile.face256SnapshotURL);
            }
            else
                Debug.LogError($"Cannot display user profile {friendRequest.From}, is not allocated");

            var ownProfile = userProfileBridge.GetOwn();
            view.SetOwnProfilePicture(ownProfile.face256SnapshotURL);

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

            openPassportVariable.Set(friendRequest.From);
        }

        private void Reject() =>
            RejectAsync().Forget();

        private async UniTaskVoid RejectAsync(CancellationToken cancellationToken = default)
        {
            view.SetState(ReceivedFriendRequestHUDModel.LayoutState.Pending);

            try
            {
                await friendsController.RejectFriendshipAsync(friendRequestId)
                                       .Timeout(TimeSpan.FromSeconds(10));
                if (cancellationToken.IsCancellationRequested) return;

                // TODO FRIEND REQUESTS (#3807): send analytics

                view.SetState(ReceivedFriendRequestHUDModel.LayoutState.RejectSuccess);
                await UniTask.Delay(TIME_MS_BEFORE_SUCCESS_SCREEN_CLOSING, cancellationToken: cancellationToken);
                view.Close();
            }
            catch (Exception)
            {
                if (cancellationToken.IsCancellationRequested) return;
                // TODO FRIEND REQUESTS (#3807): track error to analytics
                dataStore.notifications.DefaultErrorNotification.Set(PROCESS_REQUEST_ERROR_MESSAGE, true);
                view.SetState(ReceivedFriendRequestHUDModel.LayoutState.Default);
                throw;
            }
        }

        private void Confirm() =>
            ConfirmAsync().Forget();

        private async UniTaskVoid ConfirmAsync(CancellationToken cancellationToken = default)
        {
            view.SetState(ReceivedFriendRequestHUDModel.LayoutState.Pending);

            try
            {
                await friendsController.AcceptFriendshipAsync(friendRequestId)
                                       .Timeout(TimeSpan.FromSeconds(10));
                if (cancellationToken.IsCancellationRequested) return;

                // TODO FRIEND REQUESTS (#3807): send analytics

                view.SetState(ReceivedFriendRequestHUDModel.LayoutState.ConfirmSuccess);
                await UniTask.Delay(TIME_MS_BEFORE_SUCCESS_SCREEN_CLOSING, cancellationToken: cancellationToken);
                view.Close();
            }
            catch (Exception)
            {
                if (cancellationToken.IsCancellationRequested) return;
                // TODO FRIEND REQUESTS (#3807): track error to analytics
                dataStore.notifications.DefaultErrorNotification.Set(PROCESS_REQUEST_ERROR_MESSAGE, true);
                view.SetState(ReceivedFriendRequestHUDModel.LayoutState.Default);
                throw;
            }
        }
    }
}
