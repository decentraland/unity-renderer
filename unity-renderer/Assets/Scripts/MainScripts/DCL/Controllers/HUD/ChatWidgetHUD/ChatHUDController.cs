using System;
using DCL;
using DCL.Interface;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

public class ChatHUDController : IDisposable
{
    public static int MAX_CHAT_ENTRIES { internal set; get; } = 30;

    public ChatHUDView view;

    public event UnityAction<string> OnPressPrivateMessage;

    private readonly DataStore dataStore;
    private readonly RegexProfanityFilter profanityFilter;
    private InputAction_Trigger closeWindowTrigger;

    public ChatHUDController(DataStore dataStore, RegexProfanityFilter profanityFilter = null)
    {
        this.dataStore = dataStore;
        this.profanityFilter = profanityFilter;
    }

    public void Initialize(ChatHUDView view = null, UnityAction<ChatMessage> onSendMessage = null)
    {
        this.view = view ?? ChatHUDView.Create();

        this.view.Initialize(this, onSendMessage);

        this.view.OnPressPrivateMessage -= View_OnPressPrivateMessage;
        this.view.OnPressPrivateMessage += View_OnPressPrivateMessage;

        if (this.view.contextMenu != null)
        {
            this.view.contextMenu.OnShowMenu -= ContextMenu_OnShowMenu;
            this.view.contextMenu.OnShowMenu += ContextMenu_OnShowMenu;
        }

        closeWindowTrigger = Resources.Load<InputAction_Trigger>("CloseWindow");
        closeWindowTrigger.OnTriggered -= OnCloseButtonPressed;
        closeWindowTrigger.OnTriggered += OnCloseButtonPressed;
    }

    void View_OnPressPrivateMessage(string friendUserId) { OnPressPrivateMessage?.Invoke(friendUserId); }

    private void ContextMenu_OnShowMenu() { view.OnMessageCancelHover(); }

    private void OnCloseButtonPressed(DCLAction_Trigger action)
    {
        if (view.contextMenu != null)
        {
            view.contextMenu.Hide();
            view.confirmationDialog.Hide();
        }
    }

    public void AddChatMessage(ChatEntry.Model chatEntryModel, bool setScrollPositionToBottom = false)
    {
        if (IsProfanityFilteringEnabled() && chatEntryModel.messageType != ChatMessage.Type.PRIVATE)
        {
            chatEntryModel.bodyText = profanityFilter.Filter(chatEntryModel.bodyText);
            if (!string.IsNullOrEmpty(chatEntryModel.senderName))
                chatEntryModel.senderName = profanityFilter.Filter(chatEntryModel.senderName);
            if (!string.IsNullOrEmpty(chatEntryModel.recipientName))
                chatEntryModel.recipientName = profanityFilter.Filter(chatEntryModel.recipientName);
        }
            
        view.AddEntry(chatEntryModel, setScrollPositionToBottom);

        if (view.entries.Count > MAX_CHAT_ENTRIES)
        {
            Object.Destroy(view.entries[0].gameObject);
            view.entries.Remove(view.entries[0]);
        }
    }

    public void Dispose()
    {
        view.OnPressPrivateMessage -= View_OnPressPrivateMessage;
        if (view.contextMenu != null)
        {
            view.contextMenu.OnShowMenu -= ContextMenu_OnShowMenu;
        }
        closeWindowTrigger.OnTriggered -= OnCloseButtonPressed;
        Object.Destroy(view.gameObject);
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
            model.senderId = message.sender;
        }

        if (model.messageType == ChatMessage.Type.PRIVATE)
        {
            if (message.recipient == ownProfile.userId)
            {
                model.subType = ChatEntry.Model.SubType.PRIVATE_FROM;
                model.otherUserId = message.sender;
            }
            else if (message.sender == ownProfile.userId)
            {
                model.subType = ChatEntry.Model.SubType.PRIVATE_TO;
                model.otherUserId = message.recipient;
            }
            else
            {
                model.subType = ChatEntry.Model.SubType.NONE;
            }
        }

        return model;
    }
    
    private bool IsProfanityFilteringEnabled()
    {
        return dataStore.settings.profanityChatFilteringEnabled.Get()
            && profanityFilter != null;
    }
}