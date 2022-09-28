using DCL;
using DCL.Interface;
using UnityEngine;
using System;

public interface ITopNotificationsComponentView
{
    event Action<bool> OnResetFade;
    
    Transform GetPanelTransform();
    void AddNewChatNotification(ChatMessage message, string username = null, string profilePicture = null);
    void Show(bool instant = false);
    void Hide(bool instant = false);
    void ShowNotification();
    void HideNotification();
}
