using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DCL.Interface;
using DCL.Friends.WebApi;

public class WorldChatWindowController : IHUD
{
    private const string GENERAL_CHANNEL_ID = "nearby";
    private const int MAX_SEARCHED_CHANNELS = 100;
    private const int USER_DM_ENTRIES_TO_REQUEST_FOR_INITIAL_LOAD = 50;
    private const int USER_DM_ENTRIES_TO_REQUEST_FOR_SHOW_MORE = 20;
    private const int USER_DM_ENTRIES_TO_REQUEST_FOR_SEARCH = 20;

    private readonly IUserProfileBridge userProfileBridge;
    private readonly IFriendsController friendsController;
    private readonly IChatController chatController;

    private readonly Dictionary<string, PublicChatChannelModel> publicChannels =
        new Dictionary<string, PublicChatChannelModel>();

    private readonly Dictionary<string, UserProfile> recipientsFromPrivateChats = new Dictionary<string, UserProfile>();
    private readonly Dictionary<string, ChatMessage> lastPrivateMessages = new Dictionary<string, ChatMessage>();
    private int hiddenDMs;
    private string currentSearch = "";
    private bool areDMsRequestedByFirstTime;
    private bool isRequestingFriendsWithDMs;

    private IWorldChatWindowView view;
    private UserProfile ownUserProfile;

    public IWorldChatWindowView View => view;

    public event Action<string> OnOpenPrivateChat;
    public event Action<string> OnOpenPublicChannel;
    public event Action OnOpen;

    public WorldChatWindowController(
        IUserProfileBridge userProfileBridge,
        IFriendsController friendsController,
        IChatController chatController)
    {
        this.userProfileBridge = userProfileBridge;
        this.friendsController = friendsController;
        this.chatController = chatController;
    }

    public void Initialize(IWorldChatWindowView view)
    {
        this.view = view;
        view.Initialize(chatController);
        view.OnClose += HandleViewCloseRequest;
        view.OnOpenPrivateChat += OpenPrivateChat;
        view.OnOpenPublicChannel += OpenPublicChannel;
        view.OnSearchChannelRequested += SearchChannels;
        view.OnRequireMorePrivateChats += ShowMorePrivateChats;

        ownUserProfile = userProfileBridge.GetOwn();
        if (ownUserProfile != null)
            ownUserProfile.OnUpdate += OnUserProfileUpdate;

        // TODO: this data should come from the chat service when channels are implemented
        publicChannels[GENERAL_CHANNEL_ID] = new PublicChatChannelModel(GENERAL_CHANNEL_ID, "nearby",
            "Talk to the people around you. If you move far away from someone you will lose contact. All whispers will be displayed.");
        view.SetPublicChannel(publicChannels[GENERAL_CHANNEL_ID]);

        foreach (var value in chatController.GetAllocatedEntries())
            HandleMessageAdded(value);

        if (!friendsController.IsInitialized)
            if (ownUserProfile?.hasConnectedWeb3 ?? false)
                view.ShowPrivateChatsLoading();

        chatController.OnAddMessage += HandleMessageAdded;
        friendsController.OnAddFriendsWithDirectMessages += HandleFriendsWithDirectMessagesAdded;
        friendsController.OnUpdateUserStatus += HandleUserStatusChanged;
        friendsController.OnUpdateFriendship += HandleFriendshipUpdated;
        friendsController.OnInitialized += HandleFriendsControllerInitialization;
    }

    public void Dispose()
    {
        view.OnClose -= HandleViewCloseRequest;
        view.OnOpenPrivateChat -= OpenPrivateChat;
        view.OnOpenPublicChannel -= OpenPublicChannel;
        view.OnSearchChannelRequested -= SearchChannels;
        view.OnRequireMorePrivateChats -= ShowMorePrivateChats;
        view.Dispose();
        chatController.OnAddMessage -= HandleMessageAdded;
        friendsController.OnAddFriendsWithDirectMessages -= HandleFriendsWithDirectMessagesAdded;
        friendsController.OnUpdateUserStatus -= HandleUserStatusChanged;
        friendsController.OnUpdateFriendship -= HandleFriendshipUpdated;
        friendsController.OnInitialized -= HandleFriendsControllerInitialization;

        if (ownUserProfile != null)
            ownUserProfile.OnUpdate -= OnUserProfileUpdate;
    }

    public void SetVisibility(bool visible)
    {
        if (visible)
        {
            view.Show();
            OnOpen?.Invoke();

            if (friendsController.IsInitialized && !areDMsRequestedByFirstTime)
            {
                RequestFriendsWithDirectMessages(
                    USER_DM_ENTRIES_TO_REQUEST_FOR_INITIAL_LOAD,
                    0);
                RequestUnreadMessages();
            }
        }
        else
            view.Hide();
    }

    private void HandleFriendsControllerInitialization()
    {
        if (view.IsActive && !areDMsRequestedByFirstTime)
        {
            RequestFriendsWithDirectMessages(
                USER_DM_ENTRIES_TO_REQUEST_FOR_INITIAL_LOAD,
                0);
            RequestUnreadMessages();
        }
        else
            view.HidePrivateChatsLoading();
    }

    private void OpenPrivateChat(string userId)
    {
        OnOpenPrivateChat?.Invoke(userId);
    }

    private void OpenPublicChannel(string channelId) => OnOpenPublicChannel?.Invoke(channelId);

    private void HandleViewCloseRequest() => SetVisibility(false);

    private void HandleFriendshipUpdated(string userId, FriendshipAction friendship)
    {
        if (friendship == FriendshipAction.APPROVED)
        {
            var profile = userProfileBridge.Get(userId);
            if (profile == null) return;

            view.SetPrivateChat(new PrivateChatModel
            {
                user = profile,
                isBlocked = ownUserProfile.IsBlocked(userId),
                isOnline = friendsController.GetUserStatus(userId) is {presence: PresenceStatus.ONLINE},
                recentMessage = lastPrivateMessages.ContainsKey(userId) ? lastPrivateMessages[userId] : null
            });
        }
        else
        {
            // show only private chats from friends. Change it whenever the catalyst supports to send pms to any user
            view.RemovePrivateChat(userId);
        }
    }

    private void HandleUserStatusChanged(string userId, UserStatus status)
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
        else
        {
            // show only private chats from friends. Change it whenever the catalyst supports to send pms to any user
            view.RemovePrivateChat(userId);
        }
    }

    private void HandleMessageAdded(ChatMessage message)
    {
        if (message.messageType != ChatMessage.Type.PRIVATE) return;
        
        var userId = ExtractRecipientId(message);
        
        if (lastPrivateMessages.ContainsKey(userId))
        {
            if (message.timestamp > lastPrivateMessages[userId].timestamp)
                lastPrivateMessages[userId] = message;
        }
        else
            lastPrivateMessages[userId] = message;

        var profile = userProfileBridge.Get(userId);
        if (profile == null) return;

        recipientsFromPrivateChats[profile.userId] = profile;

        view.SetPrivateChat(CreatePrivateChatModel(message, profile));
    }

    private void HandleFriendsWithDirectMessagesAdded(List<FriendWithDirectMessages> usersWithDM)
    {
        for (var i = 0; i < usersWithDM.Count; i++)
        {
            if (recipientsFromPrivateChats.ContainsKey(usersWithDM[i].userId)) continue;

            var profile = userProfileBridge.Get(usersWithDM[i].userId);
            if (profile == null) continue;

            var lastMessage = new ChatMessage
            {
                messageType = ChatMessage.Type.PRIVATE,
                body = usersWithDM[i].lastMessageBody,
                timestamp = (ulong) usersWithDM[i].lastMessageTimestamp
            };

            if (lastPrivateMessages.ContainsKey(profile.userId))
            {
                if (lastMessage.timestamp > lastPrivateMessages[profile.userId].timestamp)
                    lastPrivateMessages[profile.userId] = lastMessage;
            }
            else
                lastPrivateMessages[profile.userId] = lastMessage;

            recipientsFromPrivateChats[profile.userId] = profile;

            view.SetPrivateChat(CreatePrivateChatModel(lastMessage, profile));
        }

        UpdateMoreChannelsToLoadHint();
        view.HidePrivateChatsLoading();
        view.HideMoreChatsLoading();
        view.HideSearchLoading();

        if (!string.IsNullOrEmpty(currentSearch))
            SearchChannelsLocally(currentSearch);

        isRequestingFriendsWithDMs = false;
    }

    private PrivateChatModel CreatePrivateChatModel(ChatMessage recentMessage, UserProfile profile)
    {
        return new PrivateChatModel
        {
            user = profile,
            recentMessage = recentMessage,
            isBlocked = ownUserProfile.IsBlocked(profile.userId),
            isOnline = friendsController.GetUserStatus(profile.userId).presence == PresenceStatus.ONLINE
        };
    }

    private string ExtractRecipientId(ChatMessage message)
    {
        return message.sender != ownUserProfile.userId ? message.sender : message.recipient;
    }

    private void OnUserProfileUpdate(UserProfile profile)
    {
        view.RefreshBlockedDirectMessages(profile.blocked);

        if (!profile.hasConnectedWeb3)
            view.HidePrivateChatsLoading();
    }

    private void SearchChannels(string search)
    {
        currentSearch = search;

        if (string.IsNullOrEmpty(search))
        {
            View.ClearFilter();
            UpdateMoreChannelsToLoadHint();
            return;
        }

        UpdateMoreChannelsToLoadHint();
        RequestFriendsWithDirectMessagesFromSearch(search, USER_DM_ENTRIES_TO_REQUEST_FOR_SEARCH);
        SearchChannelsLocally(search);
    }

    private void SearchChannelsLocally(string search)
    {
        Dictionary<string, PrivateChatModel> FilterPrivateChannelsByUserName(string search)
        {
            var regex = new Regex(search, RegexOptions.IgnoreCase);

            return recipientsFromPrivateChats.Values.Where(profile =>
                    !string.IsNullOrEmpty(profile.userName) && regex.IsMatch(profile.userName))
                .Take(MAX_SEARCHED_CHANNELS)
                .ToDictionary(model => model.userId,
                    profile => CreatePrivateChatModel(lastPrivateMessages[profile.userId], profile));
        }

        Dictionary<string, PublicChatChannelModel> FilterPublicChannelsByName(string search)
        {
            var regex = new Regex(search, RegexOptions.IgnoreCase);

            return publicChannels.Values
                .Where(model => !string.IsNullOrEmpty(model.name) && regex.IsMatch(model.name))
                .Take(MAX_SEARCHED_CHANNELS)
                .ToDictionary(model => model.channelId, model => model);
        }

        View.Filter(FilterPrivateChannelsByUserName(search), FilterPublicChannelsByName(search));
    }

    private void ShowMorePrivateChats()
    {
        if (isRequestingFriendsWithDMs ||
            hiddenDMs == 0 ||
            !string.IsNullOrEmpty(currentSearch))
            return;

        RequestFriendsWithDirectMessages(
            USER_DM_ENTRIES_TO_REQUEST_FOR_SHOW_MORE,
            view.PrivateChannelsCount);
    }

    internal void UpdateMoreChannelsToLoadHint()
    {
        hiddenDMs = friendsController.TotalFriendsWithDirectMessagesCount - view.PrivateChannelsCount;

        if (hiddenDMs == 0 || !string.IsNullOrEmpty(currentSearch))
            View.HideMoreChatsToLoadHint();
        else
            View.ShowMoreChatsToLoadHint(hiddenDMs);
    }

    private void RequestFriendsWithDirectMessages(int limit, int skip)
    {
        isRequestingFriendsWithDMs = true;

        if (!areDMsRequestedByFirstTime)
        {
            view.ShowPrivateChatsLoading();
            view.HideMoreChatsToLoadHint();
        }
        else
            view.ShowMoreChatsLoading();

        friendsController.GetFriendsWithDirectMessages(limit, skip);
        areDMsRequestedByFirstTime = true;
    }

    internal void RequestFriendsWithDirectMessagesFromSearch(string userNameOrId, int limit)
    {
        view.ShowSearchLoading();
        friendsController.GetFriendsWithDirectMessages(userNameOrId, limit);
    }

    private void RequestUnreadMessages() => chatController.GetUnseenMessagesByUser();
}