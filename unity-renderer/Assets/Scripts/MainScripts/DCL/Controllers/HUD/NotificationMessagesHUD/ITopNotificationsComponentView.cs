using System;
using UnityEngine;

namespace DCL.Chat.Notifications
{
    public interface ITopNotificationsComponentView
    {
        event Action<bool> OnResetFade;
    
        Transform GetPanelTransform();
        void AddNewChatNotification(PrivateChatMessageNotificationModel model);
        void AddNewChatNotification(PublicChannelMessageNotificationModel model);
        void Show(bool instant = false);
        void Hide(bool instant = false);
        void ShowNotification();
        void HideNotification();
    }
}