using System;
using DCL;
using DCL.Interface;
using UnityEngine;

public class ChatHUDController : IDisposable
{
    public const int MAX_CHAT_ENTRIES = 30;

    public event Action OnInputFieldSelected;
    public event Action<ChatMessage> OnSendMessage;
    public event Action<string> OnMessageUpdated;

    private readonly DataStore dataStore;
    private readonly IUserProfileBridge userProfileBridge;
    private readonly RegexProfanityFilter profanityFilter;
    private InputAction_Trigger closeWindowTrigger;
    private IChatHUDComponentView view;

    public ChatHUDController(DataStore dataStore,
        IUserProfileBridge userProfileBridge,
        RegexProfanityFilter profanityFilter = null)
    {
        this.dataStore = dataStore;
        this.userProfileBridge = userProfileBridge;
        this.profanityFilter = profanityFilter;
    }

    public void Initialize(IChatHUDComponentView view = null)
    {
        this.view = view ?? ChatHUDView.Create();

        this.view.OnShowMenu -= ContextMenu_OnShowMenu;
        this.view.OnShowMenu += ContextMenu_OnShowMenu;
        this.view.OnInputFieldSelected -= HandleInputFieldSelection;
        this.view.OnInputFieldSelected += HandleInputFieldSelection;
        this.view.OnSendMessage -= HandleSendMessage;
        this.view.OnSendMessage += HandleSendMessage;
        this.view.OnMessageUpdated -= HandleMessageUpdated;
        this.view.OnMessageUpdated += HandleMessageUpdated;

        closeWindowTrigger = Resources.Load<InputAction_Trigger>("CloseWindow");
        closeWindowTrigger.OnTriggered -= OnCloseButtonPressed;
        closeWindowTrigger.OnTriggered += OnCloseButtonPressed;
    }

    public void AddChatMessage(ChatMessage message, bool setScrollPositionToBottom = false)
    {
        AddChatMessage(ChatMessageToChatEntry(message), setScrollPositionToBottom);
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
        view.OnShowMenu -= ContextMenu_OnShowMenu;
        view.OnMessageUpdated -= HandleMessageUpdated;
        view.OnSendMessage -= HandleSendMessage;
        view.OnInputFieldSelected -= HandleInputFieldSelection;
        closeWindowTrigger.OnTriggered -= OnCloseButtonPressed;
        OnSendMessage = null;
        OnMessageUpdated = null;
        OnInputFieldSelected = null;
        view.Dispose();
    }

    public ChatEntry.Model ChatMessageToChatEntry(ChatMessage message)
    {
        ChatEntry.Model model = new ChatEntry.Model();

        var ownProfile = UserProfile.GetOwnUserProfile();

        model.messageType = message.messageType;
        model.bodyText = message.body;
        model.timestamp = message.timestamp;

        if (message.recipient != null)
        {
            var recipientProfile = userProfileBridge.Get(message.recipient);
            model.recipientName = recipientProfile != null ? recipientProfile.userName : message.recipient;
        }

        if (message.sender != null)
        {
            var senderProfile = userProfileBridge.Get(message.sender);
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

    public void ResetInputField(bool loseFocus = false) => view.ResetInputField(loseFocus);

    public void FocusInputField() => view.FocusInputField();

    public void SetInputFieldText(string setInputText) => view.SetInputFieldText(setInputText);

    private void ContextMenu_OnShowMenu() => view.OnMessageCancelHover();

    private void OnCloseButtonPressed(DCLAction_Trigger action) => view.Hide();

    private bool IsProfanityFilteringEnabled()
    {
        return dataStore.settings.profanityChatFilteringEnabled.Get()
               && profanityFilter != null;
    }

    private void HandleMessageUpdated(string obj) => OnMessageUpdated?.Invoke(obj);

    private void HandleSendMessage(ChatMessage obj) => OnSendMessage?.Invoke(obj);

    private void HandleInputFieldSelection() => OnInputFieldSelected?.Invoke();
}