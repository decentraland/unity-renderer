using Cysharp.Threading.Tasks;
using DCL.Tasks;
using SocialFeaturesAnalytics;
using System;
using System.Threading;

namespace DCL.Social.Friends
{
    public class SendFriendRequestHUDController
    {
        private const string PROCESS_REQUEST_ERROR_MESSAGE = "There was an error while trying to process your request. Please try again.";

        private readonly ISendFriendRequestHUDView view;
        private readonly FriendRequestHUDController friendRequestHUDController;
        private readonly DataStore dataStore;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly IFriendsController friendsController;
        private readonly ISocialAnalytics socialAnalytics;

        private string messageBody;
        private string recipientId;
        private CancellationTokenSource friendOperationsCancellationToken = new ();
        private CancellationTokenSource showCancellationToken = new ();

        public SendFriendRequestHUDController(
            ISendFriendRequestHUDView view,
            FriendRequestHUDController friendRequestHUDController,
            DataStore dataStore,
            IUserProfileBridge userProfileBridge,
            IFriendsController friendsController,
            ISocialAnalytics socialAnalytics)
        {
            this.view = view;
            this.friendRequestHUDController = friendRequestHUDController;
            this.dataStore = dataStore;
            this.userProfileBridge = userProfileBridge;
            this.friendsController = friendsController;
            this.socialAnalytics = socialAnalytics;

            dataStore.HUDs.sendFriendRequest.OnChange += ShowOrHide;

            view.OnMessageBodyChanged += OnMessageBodyChanged;
            view.OnSend += Send;
            view.OnCancel += Hide;

            view.Close();
        }

        public void Dispose()
        {
            friendOperationsCancellationToken.SafeCancelAndDispose();
            showCancellationToken.SafeCancelAndDispose();
            dataStore.HUDs.sendFriendRequest.OnChange -= ShowOrHide;
            view.OnMessageBodyChanged -= OnMessageBodyChanged;
            view.OnSend -= Send;
            view.OnCancel -= Hide;
            view.Dispose();
            friendRequestHUDController.Dispose();
        }

        private void ShowOrHide(string current, string previous)
        {
            if (string.IsNullOrEmpty(current))
                Hide();
            else
                Show(current);
        }

        private void Show(string recipient)
        {
            async UniTaskVoid ShowAsync(string recipient, CancellationToken cancellationToken)
            {
                messageBody = "";
                recipientId = recipient;
                var userProfile = userProfileBridge.Get(recipient);

                try { userProfile ??= await userProfileBridge.RequestFullUserProfileAsync(recipient, cancellationToken); }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    view.SetName(recipient);
                    view.ClearInputField();
                    view.Show();
                    throw;
                }

                view.SetName(userProfile.userName);
                // must send the snapshot observer, otherwise the faceUrl is invalid and the texture never loads
                view.SetProfilePicture(userProfile.snapshotObserver);
                view.ClearInputField();
                view.Show();
            }

            showCancellationToken = showCancellationToken.SafeRestart();
            ShowAsync(recipient, showCancellationToken.Token).Forget();
        }

        private void Send()
        {
            friendOperationsCancellationToken = friendOperationsCancellationToken.SafeRestart();

            async UniTaskVoid SendAsync(CancellationToken cancellationToken)
            {
                view.ShowPendingToSend();

                try
                {
                    await friendsController.RequestFriendshipAsync(recipientId, messageBody, cancellationToken);

                    socialAnalytics.SendFriendRequestSent(userProfileBridge.GetOwn().userId,
                        recipientId, messageBody.Length,
                        (PlayerActionSource)dataStore.HUDs.sendFriendRequestSource.Get());

                    view.ShowSendSuccess();

                    await friendRequestHUDController.HideWithDelay(cancellationToken: cancellationToken);
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    e.ReportFriendRequestErrorToAnalyticsAsSender(recipientId, dataStore.HUDs.sendFriendRequestSource.Get().ToString(),
                        userProfileBridge, socialAnalytics);

                    view.Show();
                    dataStore.notifications.DefaultErrorNotification.Set(PROCESS_REQUEST_ERROR_MESSAGE, true);
                    throw;
                }
            }

            SendAsync(friendOperationsCancellationToken.Token)
               .Forget();
        }

        private void Hide()
        {
            showCancellationToken.SafeCancelAndDispose();
            dataStore.HUDs.sendFriendRequest.Set(null, false);
            friendRequestHUDController.Hide();
        }

        private void OnMessageBodyChanged(string body) =>
            messageBody = body;
    }
}
