using System;
using UnityEngine;

namespace DCL.Chat.Notifications
{
    public interface ITopNotificationsComponentView
    {
        public delegate void ClickedNotificationDelegate(string friendRequestId, string userId, bool isAcceptedFromPeer);

        event Action<bool> OnResetFade;
        event Action<string> OnClickedChatMessage;
        event ClickedNotificationDelegate OnClickedFriendRequest;

        Transform GetPanelTransform();
        void AddNewChatNotification(PrivateChatMessageNotificationModel model);
        void AddNewChatNotification(PublicChannelMessageNotificationModel model);
        void AddNewFriendRequestNotification(FriendRequestNotificationModel model);
        void Show(bool instant = false);
        void Hide(bool instant = false);
        void ShowNotification();
        void HideNotification();
    }
}
