using System;
using UnityEngine;

namespace DCL.Chat.Notifications
{
    public interface IMainChatNotificationsComponentView
    {
        public delegate void ClickedNotificationDelegate(string friendRequestId, string userId, bool isAcceptedFromPeer);

        event Action<bool> OnResetFade;
        event Action<bool> OnPanelFocus;
        event Action<string> OnClickedChatMessage;
        event ClickedNotificationDelegate OnClickedFriendRequest;

        Transform GetPanelTransform();
        void AddNewChatNotification(PrivateChatMessageNotificationModel model);
        void AddNewChatNotification(PublicChannelMessageNotificationModel model);
        void AddNewFriendRequestNotification(FriendRequestNotificationModel model);
        void Show(bool instant = false);
        void Hide(bool instant = false);
        void ShowNotifications();
        void HideNotifications();
        void ShowPanel();
        void HidePanel();
        int GetNotificationsCount();
    }
}
