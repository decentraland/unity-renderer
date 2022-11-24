using System;
using Cysharp.Threading.Tasks;
using DCl.Social.Friends;
using UnityEngine;

namespace DCL.Social.Friends
{
    public class SendFriendRequestHUDController
    {
        private readonly ISendFriendRequestHUDView view;
        private readonly DataStore dataStore;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly IFriendsController friendsController;
        
        private string messageBody;
        private string userId;

        public SendFriendRequestHUDController(
            ISendFriendRequestHUDView view,
            DataStore dataStore,
            IUserProfileBridge userProfileBridge,
            IFriendsController friendsController)
        {
            this.view = view;
            this.dataStore = dataStore;
            this.userProfileBridge = userProfileBridge;
            this.friendsController = friendsController;

            dataStore.HUDs.sendFriendRequest.OnChange += OpenOrClose;

            view.OnMessageBodyChanged += OnMessageBodyChanged;
            view.OnSend += Send;
            view.OnCancel += Close;
        }

        public void Dispose()
        {
            dataStore.HUDs.sendFriendRequest.OnChange -= OpenOrClose;
            view.OnMessageBodyChanged -= OnMessageBodyChanged;
            view.OnSend -= Send;
            view.OnCancel -= Close;
            view.Dispose();
        }

        private void OpenOrClose(string current, string previous)
        {
            if (string.IsNullOrEmpty(current))
                Close();
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
            view.Show();
        }

        private void Send() => SendAsync().Forget();

        private async UniTaskVoid SendAsync()
        {
            view.ShowPendingToSend();

            try
            {
                // TODO: track analytics
                await friendsController.RequestFriendship(userId, messageBody)
                    .Timeout(TimeSpan.FromSeconds(10));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                view.ShowSendFailed();
            }
            
            view.ShowSendSuccess();
        }
        
        private void Close()
        {
            dataStore.HUDs.sendFriendRequest.Set(null, false);
            view.Close();
        }
        
        private void OnMessageBodyChanged(string body) => messageBody = body;
    }
}