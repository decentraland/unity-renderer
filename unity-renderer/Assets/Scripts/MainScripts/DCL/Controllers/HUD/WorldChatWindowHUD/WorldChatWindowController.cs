using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DCL.Chat.Channels;
using DCL.Friends.WebApi;
using DCL.Interface;

public class WorldChatWindowController : IHUD
{
    private const string NEARBY_CHANNEL_ID = "nearby";
    private const int MAX_SEARCHED_CHANNELS = 100;
    private const int USER_DM_ENTRIES_TO_REQUEST_FOR_INITIAL_LOAD = 50;
    private const int USER_DM_ENTRIES_TO_REQUEST_FOR_SHOW_MORE = 20;
    private const int USER_DM_ENTRIES_TO_REQUEST_FOR_SEARCH = 20;
    private const int CHANNELS_PAGE_SIZE = 10;

    private readonly IUserProfileBridge userProfileBridge;
    private readonly IFriendsController friendsController;
    private readonly IChatController chatController;
    private readonly Dictionary<string, PublicChatModel> publicChannels = new Dictionary<string, PublicChatModel>();
    private readonly Dictionary<string, UserProfile> recipientsFromPrivateChats = new Dictionary<string, UserProfile>();
    private readonly Dictionary<string, ChatMessage> lastPrivateMessages = new Dictionary<string, ChatMessage>();
    private int hiddenDMs;
    private string currentSearch = "";
    private DateTime channelsRequestTimestamp;
    private bool areDMsRequestedByFirstTime;
    private bool isRequestingFriendsWithDMs;
    private IWorldChatWindowView view;
    private UserProfile ownUserProfile;
    internal bool isRequestingDMs;
    internal bool areJoinedChannelsRequestedByFirstTime;

    public IWorldChatWindowView View => view;

    public event Action<string> OnOpenPrivateChat;
    public event Action<string> OnOpenPublicChat;
    public event Action<string> OnOpenChannel;
    public event Action OnOpen;
    public event Action OnOpenChannelSearch;
    public event Action OnOpenChannelCreation;

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
        view.OnOpenPublicChat += OpenPublicChat;
        view.OnSearchChatRequested += SearchChats;
        view.OnRequireMorePrivateChats += ShowMorePrivateChats;
        view.OnOpenChannelSearch += OpenChannelSearch;
        view.OnLeaveChannel += LeaveChannel;
        view.OnCreateChannel += OpenChannelCreationWindow;
        
        ownUserProfile = userProfileBridge.GetOwn();
        if (ownUserProfile != null)
            ownUserProfile.OnUpdate += OnUserProfileUpdate;
        
        var channel = chatController.GetAllocatedChannel(NEARBY_CHANNEL_ID);
        publicChannels[NEARBY_CHANNEL_ID] = new PublicChatModel(NEARBY_CHANNEL_ID, channel.Name,
            channel.Description,
            channel.LastMessageTimestamp,
            channel.Joined,
            channel.MemberCount);
        view.SetPublicChat(publicChannels[NEARBY_CHANNEL_ID]);
        view.ShowChannelsLoading();

        foreach (var value in chatController.GetAllocatedEntries())
            HandleMessageAdded(value);
        
        if (!friendsController.IsInitialized)
            if (ownUserProfile?.hasConnectedWeb3 ?? false)
                view.ShowPrivateChatsLoading();
        
        chatController.OnAddMessage += HandleMessageAdded;
        chatController.OnChannelUpdated += HandleChannelUpdated;
        chatController.OnChannelJoined += HandleChannelJoined;
        chatController.OnJoinChannelError += HandleJoinChannelError;
        chatController.OnChannelLeft += HandleChannelLeft;
        friendsController.OnAddFriendsWithDirectMessages += HandleFriendsWithDirectMessagesAdded;
        friendsController.OnUpdateUserStatus += HandleUserStatusChanged;
        friendsController.OnInitialized += HandleFriendsControllerInitialization;
    }

    public void Dispose()
    {
        view.OnClose -= HandleViewCloseRequest;
        view.OnOpenPrivateChat -= OpenPrivateChat;
        view.OnOpenPublicChat -= OpenPublicChat;
        view.OnSearchChatRequested -= SearchChats;
        view.OnRequireMorePrivateChats -= ShowMorePrivateChats;
        view.OnOpenChannelSearch -= OpenChannelSearch;
        view.OnCreateChannel -= OpenChannelCreationWindow;
        view.Dispose();
        chatController.OnAddMessage -= HandleMessageAdded;
        chatController.OnChannelUpdated -= HandleChannelUpdated;
        chatController.OnChannelJoined -= HandleChannelJoined;
        chatController.OnJoinChannelError -= HandleJoinChannelError;
        chatController.OnChannelLeft -= HandleChannelLeft;
        friendsController.OnAddFriendsWithDirectMessages -= HandleFriendsWithDirectMessagesAdded;
        friendsController.OnUpdateUserStatus -= HandleUserStatusChanged;
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

            if (!areJoinedChannelsRequestedByFirstTime)
            {
                RequestJoinedChannels();
                RequestUnreadChannelsMessages();
            }
        }
        else
            view.Hide();
    }
    
    private void OpenChannelCreationWindow() => OnOpenChannelCreation?.Invoke();
    
    private void LeaveChannel(string channelId) => chatController.LeaveChannel(channelId);

    private void RequestJoinedChannels()
    {
        if ((DateTime.UtcNow - channelsRequestTimestamp).TotalSeconds < 3) return;
        
        // skip=0: we do not support pagination for channels, it is supposed that a user can have a limited amount of joined channels
        chatController.GetJoinedChannels(CHANNELS_PAGE_SIZE, 0);
        channelsRequestTimestamp = DateTime.UtcNow;

        areJoinedChannelsRequestedByFirstTime = true;
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

    private void OpenPrivateChat(string userId) { OnOpenPrivateChat?.Invoke(userId); }

    private void OpenPublicChat(string channelId)
    {
        if (channelId == NEARBY_CHANNEL_ID)
            OnOpenPublicChat?.Invoke(channelId);
        else
            OnOpenChannel?.Invoke(channelId);
    }

    private void HandleViewCloseRequest() => SetVisibility(false);
    
    private void HandleUserStatusChanged(string userId, UserStatus status)
    {
        if (!recipientsFromPrivateChats.ContainsKey(userId)) return;
        if (!lastPrivateMessages.ContainsKey(userId)) return;
        
        if (status.friendshipStatus == FriendshipStatus.FRIEND)
        {
            var profile = recipientsFromPrivateChats[userId];
            
            if (ShouldDisplayPrivateChat(profile.userId))
            {
                view.SetPrivateChat(new PrivateChatModel
                {
                    user = profile,
                    recentMessage = lastPrivateMessages[userId],
                    isBlocked = ownUserProfile.IsBlocked(userId),
                    isOnline = status.presence == PresenceStatus.ONLINE
                });
            }
        }
        else if (status.friendshipStatus == FriendshipStatus.NOT_FRIEND)
        {
            // show only private chats from friends. Change it whenever the catalyst supports to send pms to any user
            view.RemovePrivateChat(userId);
        }
    }

    private void HandleMessageAdded(ChatMessage message)
    {
        if (message.messageType != ChatMessage.Type.PRIVATE) return;
        var profile = ExtractRecipient(message);
        if (profile == null) return;
        
        if (friendsController.IsInitialized)
            if (!friendsController.IsFriend(profile.userId))
                return;

        if (lastPrivateMessages.ContainsKey(profile.userId))
        {
            if (message.timestamp > lastPrivateMessages[profile.userId].timestamp)
                lastPrivateMessages[profile.userId] = message;
        }
        else
            lastPrivateMessages[profile.userId] = message;
        
        recipientsFromPrivateChats[profile.userId] = profile;

        view.SetPrivateChat(CreatePrivateChatModel(message, profile));
    }

    private void HandleFriendsWithDirectMessagesAdded(List<FriendWithDirectMessages> usersWithDM)
    {
        for (int i = 0; i < usersWithDM.Count; i++)
        {
            if (recipientsFromPrivateChats.ContainsKey(usersWithDM[i].userId))
                continue;

            var profile = userProfileBridge.Get(usersWithDM[i].userId);
            if (profile == null) continue;

            ChatMessage lastMessage = new ChatMessage();
            lastMessage.messageType = ChatMessage.Type.PRIVATE;
            lastMessage.body = usersWithDM[i].lastMessageBody;
            lastMessage.timestamp = (ulong)usersWithDM[i].lastMessageTimestamp;

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

        isRequestingDMs = false;
    }

    private bool ShouldDisplayPrivateChat(string userId)
    {
        if (!friendsController.IsInitialized) return false;
        if (view.PrivateChannelsCount < USER_DM_ENTRIES_TO_REQUEST_FOR_INITIAL_LOAD) return true;
        if (view.ContainsPrivateChannel(userId)) return true;
        return false;
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

    private UserProfile ExtractRecipient(ChatMessage message) =>
        userProfileBridge.Get(message.sender != ownUserProfile.userId ? message.sender : message.recipient);

    private void OnUserProfileUpdate(UserProfile profile)
    {
        view.RefreshBlockedDirectMessages(profile.blocked);
        
        if (!profile.hasConnectedWeb3)
            view.HidePrivateChatsLoading();
    }
    
    private void SearchChats(string search)
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
                .ToDictionary(model => model.userId, profile => CreatePrivateChatModel(lastPrivateMessages[profile.userId], profile));
        }

        Dictionary<string, PublicChatModel> FilterPublicChannelsByName(string search)
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
        if (isRequestingDMs || 
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
        isRequestingDMs = true;

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

    private void HandleChannelUpdated(Channel channel)
    {
        if (!channel.Joined)
        {
            view.RemovePublicChat(channel.ChannelId);
            publicChannels.Remove(channel.ChannelId);
            return;
        }
        
        var channelId = channel.ChannelId;
        var model = new PublicChatModel(channelId, channel.Name, channel.Description, channel.LastMessageTimestamp, channel.Joined, channel.MemberCount);
        
        if (publicChannels.ContainsKey(channelId))
            publicChannels[channelId].CopyFrom(model);
        else
            publicChannels[channelId] = model;
        
        view.SetPublicChat(model);
        view.HideChannelsLoading();
    }

    private void HandleChannelJoined(Channel channel) => OpenPublicChat(channel.ChannelId);

    private void HandleJoinChannelError(string channelId, string message)
    {
    }

    private void HandleChannelLeft(string channelId)
    {
        publicChannels.Remove(channelId);
        view.RemovePublicChat(channelId);
    }
    
    private void RequestUnreadMessages() => chatController.GetUnseenMessagesByUser();

    private void RequestUnreadChannelsMessages() => chatController.GetUnseenMessagesByChannel();

    private void OpenChannelSearch() => OnOpenChannelSearch?.Invoke();
}