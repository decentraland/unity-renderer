using Cysharp.Threading.Tasks;
using SocialFeaturesAnalytics;
using System;
using System.Threading;
using UnityEngine;

namespace DCL.Social.Friends
{
    public class ReceivedFriendRequestHUDController
    {
        private const int TIME_MS_BEFORE_SUCCESS_SCREEN_CLOSING = 3000;
        private const string PROCESS_REQUEST_ERROR_MESSAGE = "There was an error while trying to process your request. Please try again.";
        private const int FRIEND_REQUEST_OPERATION_TIMEOUT = 10;

        private readonly DataStore dataStore;
        private readonly IReceivedFriendRequestHUDView view;
        private readonly IFriendsController friendsController;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly StringVariable openPassportVariable;
        private readonly ISocialAnalytics socialAnalytics;

        private string friendRequestId;

        public ReceivedFriendRequestHUDController(DataStore dataStore,
            IReceivedFriendRequestHUDView view,
            IFriendsController friendsController,
            IUserProfileBridge userProfileBridge,
            StringVariable openPassportVariable,
            ISocialAnalytics socialAnalytics)
        {
            this.dataStore = dataStore;
            this.view = view;
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
            Hide();
        }

        private void Reject() =>
            RejectAsync().Forget();

        private async UniTaskVoid RejectAsync(CancellationToken cancellationToken = default)
        {
            view.SetState(ReceivedFriendRequestHUDModel.LayoutState.Pending);

            try
            {
                FriendRequest request = await friendsController.RejectFriendshipAsync(friendRequestId)
                                                                     .Timeout(TimeSpan.FromSeconds(FRIEND_REQUEST_OPERATION_TIMEOUT));

                if (cancellationToken.IsCancellationRequested) return;

                socialAnalytics.SendFriendRequestRejected(request.From, request.To, "modal", request.HasBodyMessage);

                view.SetState(ReceivedFriendRequestHUDModel.LayoutState.RejectSuccess);
                await UniTask.Delay(TIME_MS_BEFORE_SUCCESS_SCREEN_CLOSING, cancellationToken: cancellationToken);
                view.Close();
            }
            catch (Exception e)
            {
                await UniTask.SwitchToMainThread(cancellationToken);
                if (cancellationToken.IsCancellationRequested) return;

                FriendRequest request = friendsController.GetAllocatedFriendRequest(friendRequestId);
                socialAnalytics.SendFriendRequestError(request?.From, request?.To,
                    "modal",
                    e is FriendshipException fe
                        ? fe.ErrorCode.ToString()
                        : FriendRequestErrorCodes.Unknown.ToString());
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
                FriendRequest request = await friendsController.AcceptFriendshipAsync(friendRequestId)
                                                                     .Timeout(TimeSpan.FromSeconds(FRIEND_REQUEST_OPERATION_TIMEOUT));

                if (cancellationToken.IsCancellationRequested) return;

                socialAnalytics.SendFriendRequestApproved(request.From, request.To, "modal", request.HasBodyMessage);

                view.SetState(ReceivedFriendRequestHUDModel.LayoutState.ConfirmSuccess);
                await UniTask.Delay(TIME_MS_BEFORE_SUCCESS_SCREEN_CLOSING, cancellationToken: cancellationToken);
                view.Close();
            }
            catch (Exception e)
            {
                await UniTask.SwitchToMainThread(cancellationToken);
                if (cancellationToken.IsCancellationRequested) return;
                FriendRequest request = friendsController.GetAllocatedFriendRequest(friendRequestId);
                socialAnalytics.SendFriendRequestError(request?.From, request?.To,
                    "modal",
                    e is FriendshipException fe
                        ? fe.ErrorCode.ToString()
                        : FriendRequestErrorCodes.Unknown.ToString());
                dataStore.notifications.DefaultErrorNotification.Set(PROCESS_REQUEST_ERROR_MESSAGE, true);
                view.SetState(ReceivedFriendRequestHUDModel.LayoutState.Default);
                throw;
            }
        }
    }
}
