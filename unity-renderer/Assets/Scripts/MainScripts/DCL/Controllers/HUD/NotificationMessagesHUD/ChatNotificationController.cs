using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCL.Interface;
using DCL;

public class ChatNotificationController : IHUD
{
    IChatController chatController;
    MainChatNotificationsComponentView view;
    private IUserProfileBridge userProfileBridge;
    internal BaseVariable<Transform> isInitialized => DataStore.i.HUDs.isNotificationPanelInitialized;

    private UserProfile ownUserProfile;

    public ChatNotificationController(IChatController chatController)
    {
        this.chatController = chatController;
    }

    public void Initialize(IUserProfileBridge userProfileBridge, MainChatNotificationsComponentView view)
    {
        this.userProfileBridge = userProfileBridge;
        this.view = view;
        ownUserProfile = userProfileBridge.GetOwn();
        chatController.OnAddMessage += HandleMessageAdded;
    }

    private void HandleMessageAdded(ChatMessage message)
    {
        if (message.messageType != ChatMessage.Type.PRIVATE && message.messageType != ChatMessage.Type.PUBLIC) return;
        if (message.sender == ownUserProfile.userId) return;

        if (message.sender != null)
        {
            var profile = ExtractRecipient(message);
            if (profile == null) return;
            message.sender = profile.userName;
        }
        view.AddNewChatNotification(message);
        isInitialized.Set(view.gameObject.transform);
    }

    private UserProfile ExtractRecipient(ChatMessage message) =>
        userProfileBridge.Get(message.sender != ownUserProfile.userId ? message.sender : message.recipient);

    public void SetVisibility(bool visible)
    {
    }

    public void Dispose()
    {
    }
}
