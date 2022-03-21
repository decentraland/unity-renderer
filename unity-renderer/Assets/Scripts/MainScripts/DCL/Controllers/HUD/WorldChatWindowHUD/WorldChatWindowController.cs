using System;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Helpers;
using DCL.Interface;

public class WorldChatWindowController : IHUD
{
    private const string PLAYER_PREFS_LAST_READ_WORLD_CHAT_MESSAGES = "LastReadWorldChatMessages";

    private readonly DataStore dataStore;
    private readonly ChannelChatWindowController channelChatWindowController;
    private readonly PrivateChatWindowHUDController privateChatWindowController;
    private readonly LongVariable lastReadWorldChatMessages;
    private readonly IPlayerPrefs playerPrefs;
    private readonly IUserProfileBridge userProfileBridge;
    private Dictionary<string, UserProfile> recipientsFromPrivateChats = new Dictionary<string, UserProfile>();
    private IWorldChatWindowView view;
    private UserProfile ownUserProfile;

    private bool isSocialBarV1Enabled => dataStore.featureFlags.flags.Get().IsFeatureEnabled("social_bar_v1");

    public IWorldChatWindowView View => view;
    public bool IsInputFieldFocused { get; }
    public bool IsPreview { get; }

    public event Action<string> OnPressPrivateMessage;
    public event Action OnDeactivatePreview;
    public event Action OnOpen;

    public WorldChatWindowController(DataStore dataStore,
        ChannelChatWindowController channelChatWindowController,
        PrivateChatWindowHUDController privateChatWindowController,
        LongVariable lastReadWorldChatMessages,
        IPlayerPrefs playerPrefs,
        IUserProfileBridge userProfileBridge)
    {
        this.dataStore = dataStore;
        this.channelChatWindowController = channelChatWindowController;
        this.privateChatWindowController = privateChatWindowController;
        this.lastReadWorldChatMessages = lastReadWorldChatMessages;
        this.playerPrefs = playerPrefs;
        this.userProfileBridge = userProfileBridge;
    }

    public void Initialize(IChatController chatController,
        IMouseCatcher mouseCatcher,
        IWorldChatWindowView view)
    {
        channelChatWindowController.Initialize(chatController, mouseCatcher);
        privateChatWindowController.Initialize(chatController);
        view.Initialize(chatController);
        this.view = view;
        ownUserProfile = userProfileBridge.GetOwn();
        var privateChatsByRecipient = GetPrivateChatsByRecipient(chatController.GetEntries());
        recipientsFromPrivateChats = privateChatsByRecipient.Keys.ToDictionary(profile => profile.userId);
        ShowDirectChats(privateChatsByRecipient);
        chatController.OnAddMessage += HandleMessageAdded;
    }

    public void Dispose()
    {
        channelChatWindowController.Dispose();
        privateChatWindowController.Dispose();
    }

    public void SetVisibility(bool visible)
    {
        if (visible)
            view.Show();
        else
            view.Hide();
    }

    public void MarkWorldChatMessagesAsRead(long timestamp = 0)
    {
        long timeMark = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (timestamp != 0 && timestamp > timeMark)
            timeMark = timestamp;

        lastReadWorldChatMessages.Set(timeMark);
        SaveLatestReadWorldChatMessagesStatus();
    }

    public void DeactivatePreview()
    {
        if (!isSocialBarV1Enabled)
            channelChatWindowController.view.DeactivatePreview();
    }

    public void ActivatePreview()
    {
        if (!isSocialBarV1Enabled)
            channelChatWindowController.view.ActivatePreview();
    }

    public void OnPressReturn()
    {
        if (!isSocialBarV1Enabled)
            channelChatWindowController.OnPressReturn();
    }

    public void ResetInputField()
    {
        if (!isSocialBarV1Enabled)
            channelChatWindowController.view.ResetInputField();
    }

    private void ShowDirectChats(Dictionary<UserProfile, List<ChatMessage>> privateChatsByRecipient)
    {
        // TODO: throttle in case of hiccups
        foreach (var pair in privateChatsByRecipient)
        {
            var user = pair.Key;
            var chats = pair.Value;
            view.SetDirectRecipient(user,
                chats.OrderByDescending(message => message.timestamp).First());
        }
    }

    private void HandleMessageAdded(ChatMessage message)
    {
        if (message.messageType != ChatMessage.Type.PRIVATE) return;
        var profile = ExtractRecipient(message);
        if (recipientsFromPrivateChats.ContainsKey(profile.userId)) return;
        recipientsFromPrivateChats.Add(profile.userId, profile);
        view.SetDirectRecipient(profile, message);
    }

    private void SaveLatestReadWorldChatMessagesStatus()
    {
        playerPrefs.Set(PLAYER_PREFS_LAST_READ_WORLD_CHAT_MESSAGES,
            lastReadWorldChatMessages.Get().ToString());
        playerPrefs.Save();
    }

    private Dictionary<UserProfile, List<ChatMessage>> GetPrivateChatsByRecipient(IEnumerable<ChatMessage> messages)
    {
        var chatsByRecipient = new Dictionary<UserProfile, List<ChatMessage>>();

        foreach (var message in messages)
        {
            if (message.messageType != ChatMessage.Type.PRIVATE) continue;
            var profile = ExtractRecipient(message);
            if (!chatsByRecipient.ContainsKey(profile))
                chatsByRecipient.Add(profile, new List<ChatMessage>());
            var chats = chatsByRecipient[profile];
            chats.Add(message);
        }

        return chatsByRecipient;
    }

    private UserProfile ExtractRecipient(ChatMessage message) =>
        userProfileBridge.Get(message.sender != ownUserProfile.userId ? message.sender : message.recipient);
}