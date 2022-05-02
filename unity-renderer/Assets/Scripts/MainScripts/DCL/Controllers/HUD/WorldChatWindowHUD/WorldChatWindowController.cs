using System;
using System.Collections.Generic;
using System.Linq;
using DCL.Interface;

public class WorldChatWindowController : IHUD
{
    private const string GENERAL_CHANNEL_ID = "general";
    
    private readonly IUserProfileBridge userProfileBridge;
    private readonly IFriendsController friendsController;
    private readonly IChatController chatController;
    private readonly ILastReadMessagesService lastReadMessagesService;

    private Dictionary<string, UserProfile> recipientsFromPrivateChats = new Dictionary<string, UserProfile>();
    private Dictionary<string, ChatMessage> lastPrivateMessages = new Dictionary<string, ChatMessage>();
    private IWorldChatWindowView view;
    private UserProfile ownUserProfile;

    public IWorldChatWindowView View => view;
    public bool IsInputFieldFocused { get; }
    public bool IsPreview { get; }

    public event Action<string> OnOpenPrivateChat;
    public event Action<string> OnOpenPublicChannel;
    public event Action OnDeactivatePreview;
    public event Action OnOpen;

    public WorldChatWindowController(
        IUserProfileBridge userProfileBridge,
        IFriendsController friendsController,
        IChatController chatController,
        ILastReadMessagesService lastReadMessagesService)
    {
        this.userProfileBridge = userProfileBridge;
        this.friendsController = friendsController;
        this.chatController = chatController;
        this.lastReadMessagesService = lastReadMessagesService;
    }

    public void Initialize(IWorldChatWindowView view)
    {
        this.view = view;
        view.Initialize(chatController, lastReadMessagesService);
        view.OnClose += HandleViewCloseRequest;
        view.OnOpenPrivateChat += OpenPrivateChat;
        view.OnOpenPublicChannel += OpenPublicChannel;
        ownUserProfile = userProfileBridge.GetOwn();
        // TODO: this data should come from the chat service when channels are implemented
        view.SetPublicChannel(new PublicChatChannelModel
        {
            channelId = GENERAL_CHANNEL_ID,
            name = "General"
        });
        var privateChatsByRecipient = GetLastPrivateChatByRecipient(chatController.GetEntries());
        lastPrivateMessages = privateChatsByRecipient.ToDictionary(pair => pair.Key.userId, pair => pair.Value);
        recipientsFromPrivateChats = privateChatsByRecipient.Keys.ToDictionary(profile => profile.userId);
        ShowPrivateChats(privateChatsByRecipient);
        if (privateChatsByRecipient.Count == 0)
            view.ShowPrivateChatsLoading();
        chatController.OnAddMessage += HandleMessageAdded;
        friendsController.OnUpdateUserStatus += HandleUserStatusChanged;
        friendsController.OnInitialized += HandleFriendsControllerInitialization;
    }

    public void Dispose()
    {
        view.OnClose -= HandleViewCloseRequest;
        view.OnOpenPrivateChat -= OpenPrivateChat;
        view.OnOpenPublicChannel -= OpenPublicChannel;
        view.Dispose();
        chatController.OnAddMessage -= HandleMessageAdded;
        friendsController.OnUpdateUserStatus -= HandleUserStatusChanged;
        friendsController.OnInitialized -= HandleFriendsControllerInitialization;
    }

    public void SetVisibility(bool visible)
    {
        if (visible)
            view.Show();
        else
            view.Hide();
    }

    public void OpenLastActiveChat()
    {
        if (lastPrivateMessages.Count == 0)
        {
            OpenPublicChannel(GENERAL_CHANNEL_ID);
            return;
        }

        var mostRecentMessagePair = lastPrivateMessages.OrderByDescending(pair => pair.Value.timestamp)
            .First();
        var userId = mostRecentMessagePair.Key;
        var message = mostRecentMessagePair.Value;
        var date = message.timestamp.ToDateTimeFromUnixTimestamp();
        if ((DateTime.UtcNow - date).TotalSeconds <= 60.0)
            OpenPrivateChat(userId);
        else
            OpenPublicChannel(GENERAL_CHANNEL_ID);
    }

    private void HandleFriendsControllerInitialization()
    {
        view.HidePrivateChatsLoading();
        
        // show only private chats from friends. Change it whenever the catalyst supports to send pms to any user
        foreach(var userId in recipientsFromPrivateChats.Keys)
            if (!friendsController.IsFriend(userId))
                view.RemovePrivateChat(userId);
    }

    private void OpenPrivateChat(string userId) => OnOpenPrivateChat?.Invoke(userId);

    private void OpenPublicChannel(string channelId) => OnOpenPublicChannel?.Invoke(channelId);

    private void HandleViewCloseRequest() => SetVisibility(false);
    
    private void HandleUserStatusChanged(string userId, FriendsController.UserStatus status)
    {
        if (!recipientsFromPrivateChats.ContainsKey(userId)) return;
        if (!lastPrivateMessages.ContainsKey(userId)) return;
        if (status.friendshipStatus == FriendshipStatus.FRIEND)
        {
            var profile = recipientsFromPrivateChats[userId];
            view.SetPrivateChat(new PrivateChatModel
            {
                user = profile,
                recentMessage = lastPrivateMessages[userId],
                isBlocked = ownUserProfile.IsBlocked(userId),
                isOnline = status.presence == PresenceStatus.ONLINE
            });
        }
        else if (status.friendshipStatus == FriendshipStatus.NOT_FRIEND)
        {
            // show only private chats from friends. Change it whenever the catalyst supports to send pms to any user
            view.RemovePrivateChat(userId);
        }
    }

    private void ShowPrivateChats(Dictionary<UserProfile, ChatMessage> privateChatsByRecipient)
    {
        // TODO: throttle in case of hiccups
        foreach (var pair in privateChatsByRecipient)
        {
            var user = pair.Key;
            var message = pair.Value;
            view.SetPrivateChat(new PrivateChatModel
            {
                user = user,
                recentMessage = message,
                isBlocked = ownUserProfile.IsBlocked(user.userId),
                isOnline = friendsController.GetUserStatus(user.userId).presence == PresenceStatus.ONLINE
            });
        }
    }

    private void HandleMessageAdded(ChatMessage message)
    {
        if (message.messageType != ChatMessage.Type.PRIVATE) return;
        var profile = ExtractRecipient(message);
        if (profile == null) return;
        if (friendsController.isInitialized && !friendsController.IsFriend(profile.userId)) return;
        lastPrivateMessages[profile.userId] = message;
        recipientsFromPrivateChats[profile.userId] = profile;
        view.SetPrivateChat(new PrivateChatModel
        {
            user = profile,
            recentMessage = message,
            isBlocked = ownUserProfile.IsBlocked(profile.userId),
            isOnline = friendsController.GetUserStatus(profile.userId).presence == PresenceStatus.ONLINE
        });
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