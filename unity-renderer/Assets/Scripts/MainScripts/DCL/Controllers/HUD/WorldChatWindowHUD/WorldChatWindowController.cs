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
    private readonly LongVariable lastReadWorldChatMessages;
    private readonly IPlayerPrefs playerPrefs;
    private readonly IUserProfileBridge userProfileBridge;
    private readonly IFriendsController friendsController;
    private readonly IChatController chatController;
    private readonly IMouseCatcher mouseCatcher;
    private Dictionary<string, UserProfile> recipientsFromPrivateChats = new Dictionary<string, UserProfile>();
    private Dictionary<string, ChatMessage> lastPrivateMessages = new Dictionary<string, ChatMessage>();
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
        LongVariable lastReadWorldChatMessages,
        IPlayerPrefs playerPrefs,
        IUserProfileBridge userProfileBridge,
        IFriendsController friendsController,
        IChatController chatController,
        IMouseCatcher mouseCatcher)
    {
        this.dataStore = dataStore;
        this.channelChatWindowController = channelChatWindowController;
        this.lastReadWorldChatMessages = lastReadWorldChatMessages;
        this.playerPrefs = playerPrefs;
        this.userProfileBridge = userProfileBridge;
        this.friendsController = friendsController;
        this.chatController = chatController;
        this.mouseCatcher = mouseCatcher;
    }

    public void Initialize(IWorldChatWindowView view)
    {
        channelChatWindowController.Initialize(chatController, mouseCatcher);
        this.view = view;
        view.Initialize(chatController);
        view.OnClose += HandleViewCloseRequest;
        view.OnOpenChat += OpenPrivateChat;
        ownUserProfile = userProfileBridge.GetOwn();
        var privateChatsByRecipient = GetLastPrivateChatByRecipient(chatController.GetEntries());
        lastPrivateMessages = privateChatsByRecipient.ToDictionary(pair => pair.Key.userId, pair => pair.Value);
        recipientsFromPrivateChats = privateChatsByRecipient.Keys.ToDictionary(profile => profile.userId);
        ShowPrivateChats(privateChatsByRecipient);
        view.ShowPrivateChatsLoading();
        chatController.OnAddMessage += HandleMessageAdded;
        friendsController.OnUpdateUserStatus += HandleUserStatusChanged;
        friendsController.OnInitialized += HandleFriendsControllerInitialization;
    }

    private void HandleFriendsControllerInitialization() => view.HidePrivateChatsLoading();

    public void Dispose()
    {
        view.OnClose -= HandleViewCloseRequest;
        view.OnOpenChat -= OpenPrivateChat;
        chatController.OnAddMessage -= HandleMessageAdded;
        friendsController.OnUpdateUserStatus -= HandleUserStatusChanged;
        friendsController.OnInitialized -= HandleFriendsControllerInitialization;
        channelChatWindowController.Dispose();
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

    private void OpenPrivateChat(string userId)
    {
        OnPressPrivateMessage?.Invoke(userId);
    }

    private void HandleViewCloseRequest() => SetVisibility(false);
    
    private void HandleUserStatusChanged(string userId, FriendsController.UserStatus status)
    {
        if (!recipientsFromPrivateChats.ContainsKey(userId)) return;
        if (!lastPrivateMessages.ContainsKey(userId)) return;
        var profile = recipientsFromPrivateChats[userId];
        view.SetPrivateRecipient(profile, lastPrivateMessages[userId], ownUserProfile.IsBlocked(userId), status.presence);
    }

    private void ShowPrivateChats(Dictionary<UserProfile, ChatMessage> privateChatsByRecipient)
    {
        // TODO: throttle in case of hiccups
        foreach (var pair in privateChatsByRecipient)
        {
            var user = pair.Key;
            var message = pair.Value;
            view.SetPrivateRecipient(user,
                message,
                ownUserProfile.IsBlocked(user.userId),
                friendsController.GetUserStatus(user.userId).presence);
        }
    }

    private void HandleMessageAdded(ChatMessage message)
    {
        if (message.messageType != ChatMessage.Type.PRIVATE) return;
        var profile = ExtractRecipient(message);
        if (recipientsFromPrivateChats.ContainsKey(profile.userId)) return;
        recipientsFromPrivateChats.Add(profile.userId, profile);
        view.SetPrivateRecipient(profile, message, ownUserProfile.IsBlocked(profile.userId),
            friendsController.GetUserStatus(profile.userId).presence);
    }

    private void SaveLatestReadWorldChatMessagesStatus()
    {
        playerPrefs.Set(PLAYER_PREFS_LAST_READ_WORLD_CHAT_MESSAGES,
            lastReadWorldChatMessages.Get().ToString());
        playerPrefs.Save();
    }

    private Dictionary<UserProfile, ChatMessage> GetLastPrivateChatByRecipient(IEnumerable<ChatMessage> messages)
    {
        var chatsByRecipient = new Dictionary<UserProfile, ChatMessage>();

        foreach (var message in messages)
        {
            if (message.messageType != ChatMessage.Type.PRIVATE) continue;
            var profile = ExtractRecipient(message);
            if (!chatsByRecipient.ContainsKey(profile))
                chatsByRecipient.Add(profile, message);
            else if (message.timestamp > chatsByRecipient[profile].timestamp)
                chatsByRecipient[profile] = message;
        }

        return chatsByRecipient;
    }

    private UserProfile ExtractRecipient(ChatMessage message) =>
        userProfileBridge.Get(message.sender != ownUserProfile.userId ? message.sender : message.recipient);
}