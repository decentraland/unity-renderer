using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCL.Interface;
using DCL;

public class ChatNotificationController : IHUD
{
    IChatController chatController;
    MainChatNotificationsComponentView view;
    TaskbarHUDController taskbarHUDController;
    private IUserProfileBridge userProfileBridge;
    internal BaseVariable<Transform> isInitialized => DataStore.i.HUDs.isNotificationPanelInitialized;

    private UserProfile ownUserProfile;

    public ChatNotificationController(IChatController chatController, IUserProfileBridge userProfileBridge, MainChatNotificationsComponentView view)
    {
        this.chatController = chatController;
        this.userProfileBridge = userProfileBridge;
        this.view = view;
        taskbarHUDController = HUDController.i.taskbarHud;
        ownUserProfile = userProfileBridge.GetOwn();
        chatController.OnAddMessage += HandleMessageAdded;
        view.OnClickedNotification += OpenNotificationChat;
    }

    private void HandleMessageAdded(ChatMessage message)
    {
        if (message.messageType != ChatMessage.Type.PRIVATE && message.messageType != ChatMessage.Type.PUBLIC) return;
        if (message.sender == ownUserProfile.userId) return;

        if (message.sender != null)
        {
            var profile = ExtractRecipient(message);
            if (profile == null) return;
            view.AddNewChatNotification(message, profile.userName, profile.face256SnapshotURL);
        }
        else
        {
            view.AddNewChatNotification(message);
        }
        isInitialized.Set(view.gameObject.transform);
    }

    private void OpenNotificationChat(string targetId)
    {
        if (targetId == null) return;

        if (targetId == "#nearby")
            HUDController.i.taskbarHud.OpenPublicChatChannel("#nearby", true);
        else
            HUDController.i.taskbarHud.OpenPrivateChat(targetId);
    }

    private UserProfile ExtractRecipient(ChatMessage message) =>
        userProfileBridge.Get(message.sender != ownUserProfile.userId ? message.sender : message.recipient);

    public void SetVisibility(bool visible)
    {
    }

    public void Dispose()
    {
        chatController.OnAddMessage -= HandleMessageAdded;
        view.OnClickedNotification -= OpenNotificationChat;
    }
}
