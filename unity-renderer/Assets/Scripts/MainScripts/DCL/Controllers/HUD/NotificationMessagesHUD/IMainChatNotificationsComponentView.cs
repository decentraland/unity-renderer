using DCL;
using DCL.Interface;
using UnityEngine;
using System;

public interface IMainChatNotificationsComponentView
{
    event Action<bool> OnResetFade;

    Transform GetPanelTransform();
    ChatNotificationMessageComponentView AddNewChatNotification(ChatMessage message, string username = null, string profilePicture = null);
    void Show();
    void Hide();
    void ShowNotifications();
    void HideNotifications();
}
