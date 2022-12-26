using DCL.Social.Friends;
using System;
using UnityEngine;

namespace DCL.Chat.Notifications
{
    public interface ITopNotificationsComponentView
    {
        event Action<bool> OnResetFade;
        event Action<string> OnClickedFriendRequest;

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
