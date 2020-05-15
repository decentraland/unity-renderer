
using DCL.Interface;
using System;
using UnityEngine.Events;
public class ChatHUDController : IDisposable
{
    public const int MAX_CHAT_ENTRIES = 100;

    public ChatHUDView view;

    public UnityAction<string> OnSendMessage;

    public void Initialize(ChatHUDView view = null, UnityAction<string> onSendMessage = null)
    {
        if (view == null)
        {
            this.view = ChatHUDView.Create();
        }
        else
        {
            this.view = view;
        }

        this.view.Initialize(this, onSendMessage);
    }

    public void AddChatMessage(ChatEntry.Model chatEntryModel)
    {
        view.AddEntry(chatEntryModel);

        if (view.entries.Count > MAX_CHAT_ENTRIES)
        {
            UnityEngine.Object.Destroy(view.entries[0].gameObject);
            view.entries.Remove(view.entries[0]);
        }
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(this.view.gameObject);
    }

    public static ChatEntry.Model ChatMessageToChatEntry(ChatMessage message)
    {
        ChatEntry.Model model = new ChatEntry.Model();

        var ownProfile = UserProfile.GetOwnUserProfile();

        model.messageType = message.messageType;
        model.bodyText = message.body;
        model.timestamp = message.timestamp;

        if (message.recipient != null)
        {
            var recipientProfile = UserProfileController.userProfilesCatalog.Get(message.recipient);
            model.recipientName = recipientProfile != null ? recipientProfile.userName : message.recipient;
        }

        if (message.sender != null)
        {
            var senderProfile = UserProfileController.userProfilesCatalog.Get(message.sender);
            model.senderName = senderProfile != null ? senderProfile.userName : message.sender;
        }

        if (model.messageType == ChatMessage.Type.PRIVATE)
        {
            if (message.recipient == ownProfile.userId)
            {
                model.subType = ChatEntry.Model.SubType.PRIVATE_FROM;
            }
            else if (message.sender == ownProfile.userId)
            {
                model.subType = ChatEntry.Model.SubType.PRIVATE_TO;
            }
            else
            {
                model.subType = ChatEntry.Model.SubType.NONE;
            }
        }

        return model;
    }
}
