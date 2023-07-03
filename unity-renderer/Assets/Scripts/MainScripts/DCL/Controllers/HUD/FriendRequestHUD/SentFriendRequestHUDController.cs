using Cysharp.Threading.Tasks;
using DCL.Tasks;
using SocialFeaturesAnalytics;
using System;
using System.Threading;
using UnityEngine;

namespace DCL.Social.Friends
{
    public class SentFriendRequestHUDController
    {
        private const string PROCESS_REQUEST_ERROR_MESSAGE = "There was an error while trying to process your request. Please try again.";
        private const string OPEN_PASSPORT_SOURCE = "FriendRequest";

        private readonly ISentFriendRequestHUDView view;
        private readonly DataStore dataStore;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly IFriendsController friendsController;
        private readonly ISocialAnalytics socialAnalytics;
        private readonly BaseVariable<(string playerId, string source)> openPassportVariable;

        private CancellationTokenSource friendRequestOperationsCancellationToken = new ();
        private string friendRequestId;

        public SentFriendRequestHUDController(
            ISentFriendRequestHUDView view,
            DataStore dataStore,
            IUserProfileBridge userProfileBridge,
            IFriendsController friendsController,
            ISocialAnalytics socialAnalytics)
        {
            this.view = view;
            this.dataStore = dataStore;
            this.userProfileBridge = userProfileBridge;
            this.friendsController = friendsController;
            this.socialAnalytics = socialAnalytics;
            this.openPassportVariable = dataStore.HUDs.currentPlayerId;

            dataStore.HUDs.openSentFriendRequestDetail.OnChange += ShowOrHide;
            view.OnCancel += Cancel;
            view.OnClose += Hide;
            view.OnOpenProfile += OpenProfile;
            view.Close();
        }

        public void Dispose()
        {
            friendRequestOperationsCancellationToken.SafeCancelAndDispose();
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
            bool wasFound = friendsController.TryGetAllocatedFriendRequest(friendRequestId, out FriendRequest friendRequest);

            if (!wasFound)
            {
                Debug.LogError($"Cannot display friend request {friendRequestId}, is not allocated");
                return;
            }

            view.SetBodyMessage(friendRequest.MessageBody);
            view.SetTimestamp(friendRequest.Timestamp);

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

            view.SetSortingOrder(dataStore.HUDs.currentPassportSortingOrder.Get() + 1);
            view.Show();
        }

        private void Cancel()
        {
            friendRequestOperationsCancellationToken = friendRequestOperationsCancellationToken.SafeRestart();

            async UniTask CancelAsync(CancellationToken cancellationToken = default)
            {
                view.ShowPendingToCancel();

                try
                {
                    FriendRequest request = await friendsController.CancelRequestAsync(friendRequestId, cancellationToken);

                    socialAnalytics.SendFriendRequestCancelled(request.From, request.To, "modal", request.FriendRequestId);

                    view.Close();
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    e.ReportFriendRequestErrorToAnalyticsByRequestId(friendRequestId, "modal", friendsController, socialAnalytics);
                    view.Show();
                    dataStore.notifications.DefaultErrorNotification.Set(PROCESS_REQUEST_ERROR_MESSAGE, true);
                    throw;
                }
            }

            CancelAsync(friendRequestOperationsCancellationToken.Token).Forget();
        }

        private void Hide()
        {
            dataStore.HUDs.openSentFriendRequestDetail.Set(null, false);
            view.Close();
        }

        private void OpenProfile()
        {
            bool wasFound = friendsController.TryGetAllocatedFriendRequest(friendRequestId, out FriendRequest friendRequest);

            if (!wasFound)
            {
                Debug.LogError($"Cannot open passport {friendRequestId}, is not allocated");
                return;
            }

            openPassportVariable.Set((friendRequest.To, OPEN_PASSPORT_SOURCE));
            view.SetSortingOrder(dataStore.HUDs.currentPassportSortingOrder.Get() - 1);
        }
    }
}
