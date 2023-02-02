using Cysharp.Threading.Tasks;
using DCL.Tasks;
using SocialFeaturesAnalytics;
using System;
using System.Threading;
using UnityEngine;

namespace DCL.Social.Friends
{
    public class ReceivedFriendRequestHUDController
    {
        private const string PROCESS_REQUEST_ERROR_MESSAGE = "There was an error while trying to process your request. Please try again.";

        private readonly DataStore dataStore;
        private readonly IReceivedFriendRequestHUDView view;
        private readonly FriendRequestHUDController friendRequestHUDController;
        private readonly IFriendsController friendsController;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly StringVariable openPassportVariable;
        private readonly ISocialAnalytics socialAnalytics;

        private CancellationTokenSource friendOperationsCancellationToken = new ();
        private string friendRequestId;

        public ReceivedFriendRequestHUDController(DataStore dataStore,
            IReceivedFriendRequestHUDView view,
            FriendRequestHUDController friendRequestHUDController,
            IFriendsController friendsController,
            IUserProfileBridge userProfileBridge,
            StringVariable openPassportVariable,
            ISocialAnalytics socialAnalytics)
        {
            this.dataStore = dataStore;
            this.view = view;
            this.friendRequestHUDController = friendRequestHUDController;
            this.friendsController = friendsController;
            this.userProfileBridge = userProfileBridge;
            this.openPassportVariable = openPassportVariable;
            this.socialAnalytics = socialAnalytics;

            view.OnClose += Hide;
            view.OnOpenProfile += OpenProfile;
            view.OnRejectFriendRequest += Reject;
            view.OnConfirmFriendRequest += Confirm;
            view.Close();

            dataStore.HUDs.openReceivedFriendRequestDetail.OnChange += ShowOrHide;
        }

        public void Dispose()
        {
            friendOperationsCancellationToken.SafeCancelAndDispose();
            dataStore.HUDs.openReceivedFriendRequestDetail.OnChange -= ShowOrHide;
            friendRequestHUDController.Dispose();
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
            view.SetRecipientProfilePicture(ownProfile.face256SnapshotURL);
            view.SetSortingOrder(dataStore.HUDs.currentPassportSortingOrder.Get() + 1);
            view.Show();
        }

        private void Hide()
        {
            dataStore.HUDs.openReceivedFriendRequestDetail.Set(null, false);
            friendRequestHUDController.Hide();
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
            view.SetSortingOrder(dataStore.HUDs.currentPassportSortingOrder.Get() - 1);
        }

        private void Reject()
        {
            async UniTaskVoid RejectAsync(CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();

                view.SetState(ReceivedFriendRequestHUDModel.LayoutState.Pending);

                try
                {
                    FriendRequest request = await friendsController.RejectFriendshipAsync(friendRequestId, cancellationToken);

                    socialAnalytics.SendFriendRequestRejected(request.From, request.To, "modal", request.HasBodyMessage);

                    view.SetState(ReceivedFriendRequestHUDModel.LayoutState.RejectSuccess);
                    await friendRequestHUDController.HideWithDelay(cancellationToken: cancellationToken);
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    e.ReportFriendRequestErrorToAnalyticsByRequestId(friendRequestId, "modal", friendsController, socialAnalytics);
                    dataStore.notifications.DefaultErrorNotification.Set(PROCESS_REQUEST_ERROR_MESSAGE, true);
                    view.SetState(ReceivedFriendRequestHUDModel.LayoutState.Default);
                    throw;
                }
            }

            friendOperationsCancellationToken = friendOperationsCancellationToken.SafeRestart();
            RejectAsync(friendOperationsCancellationToken.Token).Forget();
        }

        private void Confirm()
        {
            async UniTaskVoid ConfirmAsync(CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();

                view.SetState(ReceivedFriendRequestHUDModel.LayoutState.Pending);

                try
                {
                    FriendRequest request = await friendsController.AcceptFriendshipAsync(friendRequestId, cancellationToken);

                    socialAnalytics.SendFriendRequestApproved(request.From, request.To, "modal", request.HasBodyMessage);

                    view.SetState(ReceivedFriendRequestHUDModel.LayoutState.ConfirmSuccess);
                    await friendRequestHUDController.HideWithDelay(cancellationToken: cancellationToken);
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    e.ReportFriendRequestErrorToAnalyticsByRequestId(friendRequestId, "modal", friendsController, socialAnalytics);
                    dataStore.notifications.DefaultErrorNotification.Set(PROCESS_REQUEST_ERROR_MESSAGE, true);
                    view.SetState(ReceivedFriendRequestHUDModel.LayoutState.Default);
                    throw;
                }
            }

            friendOperationsCancellationToken = friendOperationsCancellationToken.SafeRestart();
            ConfirmAsync(friendOperationsCancellationToken.Token).Forget();
        }
    }
}
