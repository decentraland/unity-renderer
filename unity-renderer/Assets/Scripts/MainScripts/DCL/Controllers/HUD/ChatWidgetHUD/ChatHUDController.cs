using Cysharp.Threading.Tasks;
using DCL;
using DCL.Chat;
using DCL.Chat.HUD;
using DCL.Interface;
using DCL.ProfanityFiltering;
using DCL.Social.Chat.Mentions;
using DCL.Tasks;
using SocialFeaturesAnalytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

public class ChatHUDController : IHUD
{
    public const int MAX_CHAT_ENTRIES = 30;
    private const int TEMPORARILY_MUTE_MINUTES = 3;
    private const int MAX_CONTINUOUS_MESSAGES = 10;
    private const int MIN_MILLISECONDS_BETWEEN_MESSAGES = 1500;
    private const int MAX_HISTORY_ITERATION = 10;
    private const int MAX_MENTION_SUGGESTIONS = 5;

    public delegate UniTask<List<UserProfile>> GetSuggestedUserProfiles(string name, int maxCount, CancellationToken cancellationToken);

    private readonly DataStore dataStore;
    private readonly IUserProfileBridge userProfileBridge;
    private readonly bool detectWhisper;
    private readonly GetSuggestedUserProfiles getSuggestedUserProfiles;
    private readonly InputAction_Trigger closeMentionSuggestionsTrigger;
    private readonly IProfanityFilter profanityFilter;
    private readonly ISocialAnalytics socialAnalytics;
    private readonly Regex mentionRegex = new (@"(\B@\w+)|(\B@+)");
    private readonly Regex whisperRegex = new (@"(?i)^\/(whisper|w) (\S+)( *)(.*)");
    private readonly Dictionary<string, ulong> temporarilyMutedSenders = new ();
    private readonly List<ChatEntryModel> spamMessages = new ();
    private readonly List<string> lastMessagesSent = new ();
    private int currentHistoryIteration;
    private IChatHUDComponentView view;
    private CancellationTokenSource mentionSuggestionCancellationToken = new ();
    private int mentionLength;
    private int mentionFromIndex;
    private Dictionary<string, UserProfile> mentionSuggestedProfiles;

    private bool useLegacySorting => dataStore.featureFlags.flags.Get().IsFeatureEnabled("legacy_chat_sorting_enabled");
    private bool isMentionsEnabled => dataStore.featureFlags.flags.Get().IsFeatureEnabled("chat_mentions_enabled");

    public IComparer<ChatEntryModel> SortingStrategy
    {
        set
        {
            if (view != null)
                view.SortingStrategy = value;
        }
    }

    public event Action OnInputFieldSelected;
    public event Action<ChatMessage> OnSendMessage;
    public event Action<ChatMessage> OnMessageSentBlockedBySpam;

    public ChatHUDController(DataStore dataStore,
        IUserProfileBridge userProfileBridge,
        bool detectWhisper,
        GetSuggestedUserProfiles getSuggestedUserProfiles,
        ISocialAnalytics socialAnalytics,
        IProfanityFilter profanityFilter = null)
    {
        this.dataStore = dataStore;
        this.userProfileBridge = userProfileBridge;
        this.detectWhisper = detectWhisper;
        this.getSuggestedUserProfiles = getSuggestedUserProfiles;
        this.socialAnalytics = socialAnalytics;
        this.profanityFilter = profanityFilter;
    }

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
        this.view.OnMentionSuggestionSelected -= HandleMentionSuggestionSelected;
        this.view.OnMentionSuggestionSelected += HandleMentionSuggestionSelected;
        this.view.OnOpenedContextMenu -= OpenedContextMenu;
        this.view.OnOpenedContextMenu += OpenedContextMenu;
        this.view.UseLegacySorting = useLegacySorting;
    }

    private void OpenedContextMenu()
    {
        socialAnalytics.SendClickedMention();
    }

    public void SetVisibility(bool visible)
    {
        if (!visible)
            HideMentionSuggestions();
    }

    public void AddChatMessage(ChatMessage message, bool setScrollPositionToBottom = false, bool spamFiltering = true, bool limitMaxEntries = true)
    {
        AddChatMessage(ChatMessageToChatEntry(message), setScrollPositionToBottom, spamFiltering, limitMaxEntries).Forget();
    }

    public async UniTask AddChatMessage(ChatEntryModel chatEntryModel, bool setScrollPositionToBottom = false, bool spamFiltering = true, bool limitMaxEntries = true)
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

        if (limitMaxEntries && view.EntryCount > MAX_CHAT_ENTRIES)
            view.RemoveOldestEntry();

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
        view.OnMentionSuggestionSelected -= HandleMentionSuggestionSelected;
        OnSendMessage = null;
        OnInputFieldSelected = null;
        view.Dispose();
        mentionSuggestionCancellationToken.SafeCancelAndDispose();
    }

    public void ClearAllEntries() =>
        view.ClearAllEntries();

    public void ResetInputField(bool loseFocus = false) =>
        view.ResetInputField(loseFocus);

    public void FocusInputField() =>
        view.FocusInputField();

    public void SetInputFieldText(string setInputText) =>
        view.SetInputFieldText(setInputText);

    public void UnfocusInputField() =>
        view.UnfocusInputField();

    private ChatEntryModel ChatMessageToChatEntry(ChatMessage message)
    {
        var model = new ChatEntryModel();
        var ownProfile = userProfileBridge.GetOwn();

        model.messageId = message.messageId;
        model.messageType = message.messageType;
        model.bodyText = message.body;
        model.timestamp = message.timestamp;
        model.isChannelMessage = message.isChannelMessage;

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
            model.subType = message.sender == ownProfile.userId
                ? ChatEntryModel.SubType.SENT
                : ChatEntryModel.SubType.RECEIVED;
        }
        else if (message.messageType == ChatMessage.Type.PUBLIC)
        {
            model.subType = message.sender == ownProfile.userId
                ? ChatEntryModel.SubType.SENT
                : ChatEntryModel.SubType.RECEIVED;
        }

        return model;
    }

    private void ContextMenu_OnShowMenu() =>
        view.OnMessageCancelHover();

    private bool IsProfanityFilteringEnabled() =>
        dataStore.settings.profanityChatFilteringEnabled.Get()
        && profanityFilter != null;

    private void HandleMessageUpdated(string message, int cursorPosition) =>
        UpdateMentions(message, cursorPosition);

    private void UpdateMentions(string message, int cursorPosition)
    {
        if (!isMentionsEnabled) return;

        if (string.IsNullOrEmpty(message))
        {
            HideMentionSuggestions();
            return;
        }

        async UniTaskVoid ShowMentionSuggestionsAsync(string name, CancellationToken cancellationToken)
        {
            try
            {
                List<UserProfile> suggestions = await getSuggestedUserProfiles.Invoke(name, MAX_MENTION_SUGGESTIONS, cancellationToken);

                mentionSuggestedProfiles = suggestions.ToDictionary(profile => profile.userId, profile => profile);

                if (suggestions.Count == 0)
                    HideMentionSuggestions();
                else
                {
                    view.ShowMentionSuggestions();
                    dataStore.mentions.isMentionSuggestionVisible.Set(true);

                    view.SetMentionSuggestions(suggestions.Select(profile => new ChatMentionSuggestionModel
                                                           {
                                                               userId = profile.userId,
                                                               userName = profile.userName,
                                                               imageUrl = profile.face256SnapshotURL,
                                                           })
                                                          .ToList());
                }
            }
            catch (Exception e) when (e is not OperationCanceledException) { HideMentionSuggestions(); }
        }

        int lastWrittenCharacterIndex = Math.Max(0, cursorPosition - 1);

        if (mentionFromIndex >= message.Length || message[lastWrittenCharacterIndex] == ' ')
            mentionFromIndex = cursorPosition;

        Match match = mentionRegex.Match(message, mentionFromIndex);

        if (match.Success)
        {
            mentionSuggestionCancellationToken = mentionSuggestionCancellationToken.SafeRestart();
            mentionFromIndex = match.Index;
            mentionLength = match.Length;
            string name = match.Value[1..];
            ShowMentionSuggestionsAsync(name, mentionSuggestionCancellationToken.Token).Forget();
        }
        else
        {
            mentionSuggestionCancellationToken.SafeCancelAndDispose();
            mentionFromIndex = lastWrittenCharacterIndex;
            HideMentionSuggestions();
        }
    }

    private void HandleSendMessage(ChatMessage message)
    {
        mentionFromIndex = 0;
        HideMentionSuggestions();

        var ownProfile = userProfileBridge.GetOwn();
        message.sender = ownProfile.userId;

        RegisterMessageHistory(message);
        currentHistoryIteration = 0;

        if (IsSpamming(message.sender) || (IsSpamming(ownProfile.userName) && !string.IsNullOrEmpty(message.body)))
        {
            OnMessageSentBlockedBySpam?.Invoke(message);
            return;
        }

        if (MentionsUtils.TextContainsMention(message.body))
            socialAnalytics.SendMessageWithMention();

        ApplyWhisperAttributes(message);

        if (message.body.ToLower().StartsWith("/join"))
        {
            if (!ownProfile.isGuest)
                dataStore.channels.channelJoinedSource.Set(ChannelJoinedSource.Command);
            else
            {
                dataStore.HUDs.connectWalletModalVisible.Set(true);
                return;
            }
        }

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

    private void HandleInputFieldDeselected() =>
        currentHistoryIteration = 0;

    private bool IsSpamming(string senderName)
    {
        if (string.IsNullOrEmpty(senderName)) return false;

        var isSpamming = false;

        if (!temporarilyMutedSenders.ContainsKey(senderName)) return false;

        var muteTimestamp = DateTimeOffset.FromUnixTimeMilliseconds((long)temporarilyMutedSenders[senderName]);

        if ((DateTimeOffset.UtcNow - muteTimestamp).Minutes < TEMPORARILY_MUTE_MINUTES)
            isSpamming = true;
        else
            temporarilyMutedSenders.Remove(senderName);

        return isSpamming;
    }

    private void UpdateSpam(ChatEntryModel model)
    {
        if (spamMessages.Count == 0)
            spamMessages.Add(model);
        else if (spamMessages[^1].senderName == model.senderName)
        {
            if (MessagesSentTooFast(spamMessages[^1].timestamp, model.timestamp))
            {
                spamMessages.Add(model);

                if (spamMessages.Count >= MAX_CONTINUOUS_MESSAGES)
                {
                    temporarilyMutedSenders.Add(model.senderName, model.timestamp);
                    spamMessages.Clear();
                }
            }
            else
                spamMessages.Clear();
        }
        else
            spamMessages.Clear();
    }

    private bool MessagesSentTooFast(ulong oldMessageTimeStamp, ulong newMessageTimeStamp)
    {
        var oldDateTime = DateTimeOffset.FromUnixTimeMilliseconds((long)oldMessageTimeStamp);
        var newDateTime = DateTimeOffset.FromUnixTimeMilliseconds((long)newMessageTimeStamp);
        return (newDateTime - oldDateTime).TotalMilliseconds < MIN_MILLISECONDS_BETWEEN_MESSAGES;
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

    private void HandleMentionSuggestionSelected(string userId)
    {
        view.AddMentionToInputField(mentionFromIndex, mentionLength, userId, mentionSuggestedProfiles[userId].userName);
        socialAnalytics.SendMentionCreated(MentionCreationSource.SuggestionList);
        HideMentionSuggestions();
    }

    private void HideMentionSuggestions()
    {
        view.HideMentionSuggestions();
        dataStore.mentions.isMentionSuggestionVisible.Set(false);
    }
}
