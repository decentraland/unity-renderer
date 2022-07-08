using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCL.Interface;
using DCL;

public class ChatNotificationController : IHUD
{
    IChatController chatController;
    MainChatNotificationsComponentView mainChatNotificationView;
    private IUserProfileBridge userProfileBridge;
    internal BaseVariable<Transform> isInitialized => DataStore.i.HUDs.isNotificationPanelInitialized;
    internal BaseVariable<HashSet<string>> visibleTaskbarPanels => DataStore.i.HUDs.visibleTaskbarPanels;

    public event Action<string> OnOpenNotificationChat;

    private UserProfile ownUserProfile;

    public ChatNotificationController(IChatController chatController, IUserProfileBridge userProfileBridge)
    {
        this.chatController = chatController;
        this.userProfileBridge = userProfileBridge;
        mainChatNotificationView = MainChatNotificationsComponentView.Create();
        mainChatNotificationView.Initialize(this);
        ownUserProfile = userProfileBridge.GetOwn();
        chatController.OnAddMessage += HandleMessageAdded;
        mainChatNotificationView.OnClickedNotification += OpenNotificationChat;
        isInitialized.Set(mainChatNotificationView.gameObject.transform);
        visibleTaskbarPanels.OnChange += VisiblePanelsChanged;
    }

    private void VisiblePanelsChanged(HashSet<string> newList, HashSet<string> oldList)
    {
        SetVisibility(newList.Count == 0);
    }

    private void HandleMessageAdded(ChatMessage message)
    {
        if (message.messageType != ChatMessage.Type.PRIVATE && message.messageType != ChatMessage.Type.PUBLIC) return;
        if (message.sender == ownUserProfile.userId) return;

        if (message.sender != null)
        {
            var profile = ExtractRecipient(message);
            if (profile == null) return;
            mainChatNotificationView.AddNewChatNotification(message, profile.userName, profile.face256SnapshotURL);
        }
        else
        {
            mainChatNotificationView.AddNewChatNotification(message);
        }
    }

    private void OpenNotificationChat(string targetId)
    {
        if (targetId == null) return;

        OnOpenNotificationChat?.Invoke(targetId);
    }

    private UserProfile ExtractRecipient(ChatMessage message) =>
        userProfileBridge.Get(message.sender != ownUserProfile.userId ? message.sender : message.recipient);

    public void SetVisibility(bool visible)
    {
        if (visible)
            mainChatNotificationView.Show();
        else
            mainChatNotificationView.Hide();
    }

    public void Dispose()
    {
        chatController.OnAddMessage -= HandleMessageAdded;
        mainChatNotificationView.OnClickedNotification -= OpenNotificationChat;
    }
}
