using Cysharp.Threading.Tasks;
using SocialFeaturesAnalytics;
using System;
using System.Threading;

namespace DCL.Social.Friends
{
    public class SendFriendRequestHUDController
    {
        private const string PROCESS_REQUEST_ERROR_MESSAGE = "There was an error while trying to process your request. Please try again.";
        private const int AUTOMATIC_CLOSE_DELAY = 2000;

        private readonly ISendFriendRequestHUDView view;
        private readonly DataStore dataStore;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly IFriendsController friendsController;
        private readonly ISocialAnalytics socialAnalytics;

        private string messageBody;
        private string recipientId;
        private CancellationTokenSource hideCancellationToken = new ();
        private CancellationTokenSource friendOperationsCancellationToken = new ();
        private bool sendInProgress;

        public SendFriendRequestHUDController(
            ISendFriendRequestHUDView view,
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

            dataStore.HUDs.sendFriendRequest.OnChange += ShowOrHide;

            view.OnMessageBodyChanged += OnMessageBodyChanged;
            view.OnSend += Send;
            view.OnCancel += Hide;

            view.Close();
        }

        public void Dispose()
        {
            friendOperationsCancellationToken.Cancel();
            friendOperationsCancellationToken.Dispose();
            dataStore.HUDs.sendFriendRequest.OnChange -= ShowOrHide;
            view.OnMessageBodyChanged -= OnMessageBodyChanged;
            view.OnSend -= Send;
            view.OnCancel -= Hide;
            view.Dispose();
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
            messageBody = "";
            recipientId = recipient;
            var userProfile = userProfileBridge.Get(recipient);
            if (userProfile == null) return;
            view.SetName(userProfile.userName);

            // must send the snapshot observer, otherwise the faceUrl is invalid and the texture never loads
            view.SetProfilePicture(userProfile.snapshotObserver);
            view.ClearInputField();
            view.Show();
        }

        private void Send()
        {
            friendOperationsCancellationToken.Cancel();
            friendOperationsCancellationToken = new CancellationTokenSource();
            SendAsync(friendOperationsCancellationToken.Token).Forget();
        }

        private async UniTaskVoid SendAsync(CancellationToken cancellationToken)
        {
            hideCancellationToken?.Cancel();
            hideCancellationToken = new CancellationTokenSource();

            view.ShowPendingToSend();

            try
            {
                sendInProgress = true;

                await friendsController.RequestFriendshipAsync(recipientId, messageBody, cancellationToken);

                socialAnalytics.SendFriendRequestSent(userProfileBridge.GetOwn().userId,
                    recipientId, messageBody.Length,
                    (PlayerActionSource)dataStore.HUDs.sendFriendRequestSource.Get());

                view.ShowSendSuccess();
                sendInProgress = false;

                await UniTask.Delay(AUTOMATIC_CLOSE_DELAY,
                    cancellationToken: hideCancellationToken.AddTo(cancellationToken).Token);

                if (!hideCancellationToken.IsCancellationRequested)
                    view.Close();
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                if (!sendInProgress)
                    return;

                socialAnalytics.SendFriendRequestError(userProfileBridge.GetOwn().userId, recipientId,
                    dataStore.HUDs.sendFriendRequestSource.Get().ToString(),
                    e is FriendshipException fe
                        ? fe.ErrorCode.ToString()
                        : FriendRequestErrorCodes.Unknown.ToString());

                view.Show();
                dataStore.notifications.DefaultErrorNotification.Set(PROCESS_REQUEST_ERROR_MESSAGE, true);
                throw;
            }
        }

        private void Hide()
        {
            hideCancellationToken?.Cancel();
            dataStore.HUDs.sendFriendRequest.Set(null, false);
            view.Close();
        }

        private void OnMessageBodyChanged(string body) =>
            messageBody = body;
    }
}
