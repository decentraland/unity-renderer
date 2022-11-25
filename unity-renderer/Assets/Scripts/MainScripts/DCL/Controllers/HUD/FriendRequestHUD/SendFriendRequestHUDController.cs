using System;
using Cysharp.Threading.Tasks;
using DCl.Social.Friends;
using SocialFeaturesAnalytics;
using UnityEngine;

namespace DCL.Social.Friends
{
    public class SendFriendRequestHUDController
    {
        private readonly ISendFriendRequestHUDView view;
        private readonly DataStore dataStore;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly IFriendsController friendsController;
        private readonly ISocialAnalytics socialAnalytics;

        private string messageBody;
        private string userId;

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
            userId = recipient;
            var userProfile = userProfileBridge.Get(recipient);
            if (userProfile == null) return;
            view.SetName(userProfile.userName);
            // must send the snapshot observer, otherwise the faceUrl is invalid and the texture never loads
            view.SetProfilePicture(userProfile.snapshotObserver);
            view.ClearInputField();
            view.Show();
        }

        private void Send() => SendAsync().Forget();

        private async UniTaskVoid SendAsync()
        {
            view.ShowPendingToSend();

            try
            {
                socialAnalytics.SendFriendRequestSent(userProfileBridge.GetOwn().userId, userId, messageBody.Length,
                    (PlayerActionSource) dataStore.HUDs.sendFriendRequestSource.Get());
                await friendsController.RequestFriendship(userId, messageBody)
                    .Timeout(TimeSpan.FromSeconds(10));
                view.ShowSendSuccess();
            }
            catch (Exception e)
            {
                // TODO: track error to analytics
                Debug.LogException(e);
                view.ShowSendFailed();
            }
        }

        private void Hide()
        {
            dataStore.HUDs.sendFriendRequest.Set(null, false);
            view.Close();
        }

        private void OnMessageBodyChanged(string body) => messageBody = body;
    }
}
