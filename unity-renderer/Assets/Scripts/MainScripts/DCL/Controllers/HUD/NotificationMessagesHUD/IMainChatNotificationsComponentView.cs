using DCL;
using DCL.Interface;
using UnityEngine;
using System;

public interface IMainChatNotificationsComponentView
{
    event Action<bool> OnResetFade;

    Transform GetPanelTransform();
    ChatNotificationMessageComponentView AddNewChatNotification(ChatMessage message, string username = null, string profilePicture = null);
    void Show(bool instant = false);
    void Hide(bool instant = false);
    void ShowNotifications();
    void HideNotifications();
}
