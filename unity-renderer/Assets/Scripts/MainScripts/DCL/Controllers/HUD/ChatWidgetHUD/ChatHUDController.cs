using System;
using System.Text.RegularExpressions;
using DCL;
using DCL.Interface;

public class ChatHUDController : IDisposable
{
    public const int MAX_CHAT_ENTRIES = 30;

    public event Action OnInputFieldSelected;
    public event Action<ChatMessage> OnSendMessage;
    public event Action<string> OnMessageUpdated;

    private readonly DataStore dataStore;
    private readonly IUserProfileBridge userProfileBridge;
    private readonly bool detectWhisper;
    private readonly RegexProfanityFilter profanityFilter;
    private readonly Regex whisperRegex = new Regex(@"(?i)^\/(whisper|w) (\S+)( *)(.*)");
    private IChatHUDComponentView view;

    public ChatHUDController(DataStore dataStore,
        IUserProfileBridge userProfileBridge,
        bool detectWhisper,
        RegexProfanityFilter profanityFilter = null)
    {
        this.dataStore = dataStore;
        this.userProfileBridge = userProfileBridge;
        this.detectWhisper = detectWhisper;
        this.profanityFilter = profanityFilter;
    }

    public void Initialize(IChatHUDComponentView view)
    {
        this.view = view;
        this.view.OnShowMenu -= ContextMenu_OnShowMenu;
        this.view.OnShowMenu += ContextMenu_OnShowMenu;
        this.view.OnInputFieldSelected -= HandleInputFieldSelection;
        this.view.OnInputFieldSelected += HandleInputFieldSelection;
        this.view.OnSendMessage -= HandleSendMessage;
        this.view.OnSendMessage += HandleSendMessage;
        this.view.OnMessageUpdated -= HandleMessageUpdated;
        this.view.OnMessageUpdated += HandleMessageUpdated;
    }

    public void AddChatMessage(ChatMessage message, bool setScrollPositionToBottom = false)
    {
        AddChatMessage(ChatMessageToChatEntry(message), setScrollPositionToBottom);
    }

    public void AddChatMessage(ChatEntryModel chatEntryModel, bool setScrollPositionToBottom = false)
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
        OnSendMessage = null;
        OnMessageUpdated = null;
        OnInputFieldSelected = null;
        view.Dispose();
    }

    public void ClearAllEntries() => view.ClearAllEntries();

    public void ResetInputField(bool loseFocus = false) => view.ResetInputField(loseFocus);

    public void FocusInputField() => view.FocusInputField();

    public void SetInputFieldText(string setInputText) => view.SetInputFieldText(setInputText);

    private ChatEntryModel ChatMessageToChatEntry(ChatMessage message)
    {
        var model = new ChatEntryModel();
        var ownProfile = userProfileBridge.GetOwn();

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

        if (message.messageType == ChatMessage.Type.PRIVATE)
        {
            if (message.recipient == ownProfile.userId)
            {
                model.subType = ChatEntryModel.SubType.RECEIVED;
                model.otherUserId = message.sender;
            }
            else if (message.sender == ownProfile.userId)
            {
                model.subType = ChatEntryModel.SubType.SENT;
                model.otherUserId = message.recipient;
            }
            else
            {
                model.subType = ChatEntryModel.SubType.NONE;
            }
        }
        else if (message.messageType == ChatMessage.Type.PUBLIC)
        {
            model.subType = message.sender == ownProfile.userId
                ? ChatEntryModel.SubType.SENT
                : ChatEntryModel.SubType.RECEIVED;
        }

        return model;
    }

    private void ContextMenu_OnShowMenu() => view.OnMessageCancelHover();

    private bool IsProfanityFilteringEnabled()
    {
        return dataStore.settings.profanityChatFilteringEnabled.Get()
               && profanityFilter != null;
    }

    private void HandleMessageUpdated(string obj) => OnMessageUpdated?.Invoke(obj);

    private void HandleSendMessage(ChatMessage message)
    {
        ApplyWhisperAttributes(message);
        OnSendMessage?.Invoke(message);
    }

    private void ApplyWhisperAttributes(ChatMessage message)
    {
        if (!detectWhisper) return;
        var body = message.body;
        if (string.IsNullOrWhiteSpace(body)) return;

        var match = whisperRegex.Match(body);
        if (!match.Success) return;

        message.messageType = ChatMessage.Type.PRIVATE;
        message.recipient = match.Groups[2].Value;
        message.body = match.Groups[4].Value;
    }

    private void HandleInputFieldSelection() => OnInputFieldSelected?.Invoke();
}