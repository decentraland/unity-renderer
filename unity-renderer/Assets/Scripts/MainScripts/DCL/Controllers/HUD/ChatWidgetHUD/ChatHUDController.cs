using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Interface;

public class ChatHUDController : IDisposable
{
    public const int MAX_CHAT_ENTRIES = 30;
    private const int TEMPORARILY_MUTE_MINUTES = 10;
    private const int MAX_CONTINUOUS_MESSAGES = 6;
    private const int MIN_MILLISECONDS_BETWEEN_MESSAGES = 1500;
    private const int MAX_HISTORY_ITERATION = 10;

    public event Action OnInputFieldSelected;
    public event Action OnInputFieldDeselected;
    public event Action<ChatMessage> OnSendMessage;
    public event Action<string> OnMessageUpdated;

    private readonly DataStore dataStore;
    private readonly IUserProfileBridge userProfileBridge;
    private readonly bool detectWhisper;
    private readonly IProfanityFilter profanityFilter;
    private readonly Regex whisperRegex = new Regex(@"(?i)^\/(whisper|w) (\S+)( *)(.*)");
    private readonly Dictionary<string, ulong> temporarilyMutedSenders = new Dictionary<string, ulong>();
    private readonly List<ChatEntryModel> spamMessages = new List<ChatEntryModel>();
    private readonly List<string> lastMessagesSent = new List<string>();
    private int currentHistoryIteration;
    private IChatHUDComponentView view;

    public ChatHUDController(DataStore dataStore,
        IUserProfileBridge userProfileBridge,
        bool detectWhisper,
        IProfanityFilter profanityFilter = null)
    {
        this.dataStore = dataStore;
        this.userProfileBridge = userProfileBridge;
        this.detectWhisper = detectWhisper;
        this.profanityFilter = profanityFilter;
    }

    public bool IsInputSelected => view.IsInputFieldSelected;

    public void Initialize(IChatHUDComponentView view)
    {
        this.view = view;
        this.view.OnPreviousChatInHistory -= FillInputWithPreviousMessage;
        this.view.OnPreviousChatInHistory += FillInputWithPreviousMessage;
        this.view.OnNextChatInHistory -= FillInputWithNextMessage;
        this.view.OnNextChatInHistory += FillInputWithNextMessage;
        this.view.OnShowMenu -= ContextMenu_OnShowMenu;
        this.view.OnShowMenu += ContextMenu_OnShowMenu;
        this.view.OnInputFieldSelected -= HandleInputFieldSelected;
        this.view.OnInputFieldSelected += HandleInputFieldSelected;
        this.view.OnInputFieldDeselected -= HandleInputFieldDeselected;
        this.view.OnInputFieldDeselected += HandleInputFieldDeselected;
        this.view.OnSendMessage -= HandleSendMessage;
        this.view.OnSendMessage += HandleSendMessage;
        this.view.OnMessageUpdated -= HandleMessageUpdated;
        this.view.OnMessageUpdated += HandleMessageUpdated;
    }

    public void AddChatMessage(ChatMessage message, bool setScrollPositionToBottom = false, bool spamFiltering = true)
    {
        AddChatMessage(ChatMessageToChatEntry(message), setScrollPositionToBottom, spamFiltering).Forget();
    }

    public async UniTask AddChatMessage(ChatEntryModel chatEntryModel, bool setScrollPositionToBottom = false, bool spamFiltering = true)
    {
        if (IsSpamming(chatEntryModel.senderName) && spamFiltering) return;
        
        chatEntryModel.bodyText = ChatUtils.AddNoParse(chatEntryModel.bodyText);

        if (IsProfanityFilteringEnabled() && chatEntryModel.messageType != ChatMessage.Type.PRIVATE)
        {
            chatEntryModel.bodyText = await profanityFilter.Filter(chatEntryModel.bodyText);

            if (!string.IsNullOrEmpty(chatEntryModel.senderName))
                chatEntryModel.senderName = await profanityFilter.Filter(chatEntryModel.senderName);

            if (!string.IsNullOrEmpty(chatEntryModel.recipientName))
                chatEntryModel.recipientName = await profanityFilter.Filter(chatEntryModel.recipientName);
        }

        await UniTask.SwitchToMainThread();

        view.AddEntry(chatEntryModel, setScrollPositionToBottom);

        if (view.EntryCount > MAX_CHAT_ENTRIES)
            view.RemoveFirstEntry();
        
        if (string.IsNullOrEmpty(chatEntryModel.senderId)) return;

        if (spamFiltering)
            UpdateSpam(chatEntryModel);
    }

    public void Dispose()
    {
        view.OnShowMenu -= ContextMenu_OnShowMenu;
        view.OnMessageUpdated -= HandleMessageUpdated;
        view.OnSendMessage -= HandleSendMessage;
        view.OnInputFieldSelected -= HandleInputFieldSelected;
        view.OnInputFieldDeselected -= HandleInputFieldDeselected;
        view.OnPreviousChatInHistory -= FillInputWithPreviousMessage;
        view.OnNextChatInHistory -= FillInputWithNextMessage;
        OnSendMessage = null;
        OnMessageUpdated = null;
        OnInputFieldSelected = null;
        view.Dispose();
    }

    public void ClearAllEntries() => view.ClearAllEntries();

    public void ResetInputField(bool loseFocus = false) => view.ResetInputField(loseFocus);

    public void FocusInputField() => view.FocusInputField();

    public void SetInputFieldText(string setInputText) => view.SetInputFieldText(setInputText);
    
    public void UnfocusInputField() => view.UnfocusInputField();

    public void ActivatePreview() => view.ActivatePreview();
    
    public void DeactivatePreview() => view.DeactivatePreview();

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
        var ownProfile = userProfileBridge.GetOwn();
        message.sender = ownProfile.userId;
        RegisterMessageHistory(message);
        currentHistoryIteration = 0;
        if (IsSpamming(message.sender)) return;
        if (IsSpamming(ownProfile.userName)) return;
        ApplyWhisperAttributes(message);
        OnSendMessage?.Invoke(message);
    }

    private void RegisterMessageHistory(ChatMessage message)
    {
        if (string.IsNullOrEmpty(message.body)) return;

        lastMessagesSent.RemoveAll(s => s.Equals(message.body));
        lastMessagesSent.Insert(0, message.body);

        if (lastMessagesSent.Count > MAX_HISTORY_ITERATION)
            lastMessagesSent.RemoveAt(lastMessagesSent.Count - 1);
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

    private void HandleInputFieldSelected()
    {
        currentHistoryIteration = 0;
        OnInputFieldSelected?.Invoke();
    }

    private void HandleInputFieldDeselected()
    {
        currentHistoryIteration = 0;
        OnInputFieldDeselected?.Invoke();
    }

    private bool IsSpamming(string senderName)
    {
        if (string.IsNullOrEmpty(senderName)) return false;

        var isSpamming = false;

        if (!temporarilyMutedSenders.ContainsKey(senderName)) return false;
        
        var muteTimestamp = CreateBaseDateTime().AddMilliseconds(temporarilyMutedSenders[senderName]).ToLocalTime();
        if ((DateTime.Now - muteTimestamp).Minutes < TEMPORARILY_MUTE_MINUTES)
            isSpamming = true;
        else
            temporarilyMutedSenders.Remove(senderName);

        return isSpamming;
    }
    
    private void UpdateSpam(ChatEntryModel model)
    {
        if (spamMessages.Count == 0)
        {
            spamMessages.Add(model);
        }
        else if (spamMessages[spamMessages.Count - 1].senderName == model.senderName)
        {
            if (MessagesSentTooFast(spamMessages[spamMessages.Count - 1].timestamp, model.timestamp))
            {
                spamMessages.Add(model);

                if (spamMessages.Count == MAX_CONTINUOUS_MESSAGES)
                {
                    temporarilyMutedSenders.Add(model.senderName, model.timestamp);
                    spamMessages.Clear();
                }
            }
            else
            {
                spamMessages.Clear();
            }
        }
        else
        {
            spamMessages.Clear();
        }
    }

    private bool MessagesSentTooFast(ulong oldMessageTimeStamp, ulong newMessageTimeStamp)
    {
        DateTime oldDateTime = CreateBaseDateTime().AddMilliseconds(oldMessageTimeStamp).ToLocalTime();
        DateTime newDateTime = CreateBaseDateTime().AddMilliseconds(newMessageTimeStamp).ToLocalTime();
        return (newDateTime - oldDateTime).TotalMilliseconds < MIN_MILLISECONDS_BETWEEN_MESSAGES;
    }

    private DateTime CreateBaseDateTime()
    {
        return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    }
    
    private void FillInputWithNextMessage()
    {
        if (lastMessagesSent.Count == 0) return;
        view.FocusInputField();
        view.SetInputFieldText(lastMessagesSent[currentHistoryIteration]);
        currentHistoryIteration = (currentHistoryIteration + 1) % lastMessagesSent.Count;
    }

    private void FillInputWithPreviousMessage()
    {
        if (lastMessagesSent.Count == 0)
        {
            view.ResetInputField();
            return;
        }
        
        currentHistoryIteration--;
        if (currentHistoryIteration < 0)
            currentHistoryIteration = lastMessagesSent.Count - 1;
        
        view.FocusInputField();
        view.SetInputFieldText(lastMessagesSent[currentHistoryIteration]);
    }
}
