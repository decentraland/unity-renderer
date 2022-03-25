using System;
using DCL;
using DCL.Interface;
using UnityEngine;
using UnityEngine.Events;

public class ChatHUDController : IDisposable
{
    public const int MAX_CHAT_ENTRIES = 30;

    public IChatHUDComponentView view;

    public event UnityAction<string> OnPressPrivateMessage;
    public event Action OnInputFieldSelected;

    private readonly DataStore dataStore;
    private readonly RegexProfanityFilter profanityFilter;
    private InputAction_Trigger closeWindowTrigger;

    public ChatHUDController(DataStore dataStore, RegexProfanityFilter profanityFilter = null)
    {
        this.dataStore = dataStore;
        this.profanityFilter = profanityFilter;
    }

    public void Initialize(IChatHUDComponentView view = null, Action<ChatMessage> onSendMessage = null)
    {
        this.view = view ?? ChatHUDView.Create();
        view.OnSendMessage += onSendMessage;

        this.view.OnPressPrivateMessage -= View_OnPressPrivateMessage;
        this.view.OnPressPrivateMessage += View_OnPressPrivateMessage;
        this.view.OnShowMenu -= ContextMenu_OnShowMenu;
        this.view.OnShowMenu += ContextMenu_OnShowMenu;
        this.view.OnInputFieldSelected -= OnInputFieldSelected;
        this.view.OnInputFieldSelected += OnInputFieldSelected;

        closeWindowTrigger = Resources.Load<InputAction_Trigger>("CloseWindow");
        closeWindowTrigger.OnTriggered -= OnCloseButtonPressed;
        closeWindowTrigger.OnTriggered += OnCloseButtonPressed;
    }

    public void AddChatMessage(ChatEntry.Model chatEntryModel, bool setScrollPositionToBottom = false)
    {
        chatEntryModel.bodyText = ChatUtils.AddNoParse(chatEntryModel.bodyText);

        if (IsProfanityFilteringEnabled() && chatEntryModel.messageType != ChatMessage.Type.PRIVATE)
        {
            chatEntryModel.bodyText = profanityFilter.Filter(chatEntryModel.bodyText);
            if (!string.IsNullOrEmpty(chatEntryModel.senderName))
                chatEntryModel.senderName = profanityFilter.Filter(chatEntryModel.senderName);
            if (!string.IsNullOrEmpty(chatEntryModel.recipientName))
                chatEntryModel.recipientName = profanityFilter.Filter(chatEntryModel.recipientName);
        }
            
        view.AddEntry(chatEntryModel, setScrollPositionToBottom);

        if (view.EntryCount > MAX_CHAT_ENTRIES)
            view.RemoveFirstEntry();
    }

    public void Dispose()
    {
        view.OnPressPrivateMessage -= View_OnPressPrivateMessage;
        view.OnShowMenu -= ContextMenu_OnShowMenu;
        closeWindowTrigger.OnTriggered -= OnCloseButtonPressed;
        view.Dispose();
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
    
    public void ClearAllEntries() => view.ClearAllEntries();

    public void ResetInputField() => view.ResetInputField();
    
    public void FocusInputField() => view.FocusInputField();
    
    private void View_OnPressPrivateMessage(string friendUserId) => OnPressPrivateMessage?.Invoke(friendUserId);

    private void ContextMenu_OnShowMenu() => view.OnMessageCancelHover();

    private void OnCloseButtonPressed(DCLAction_Trigger action) => view.Hide();
    
    private bool IsProfanityFilteringEnabled()
    {
        return dataStore.settings.profanityChatFilteringEnabled.Get()
            && profanityFilter != null;
    }
}