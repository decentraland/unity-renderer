using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL.Friends.WebApi;
using DCL.Interface;
using DCL;
using Channel = DCL.Chat.Channels.Channel;

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
    private readonly DataStore dataStore;
    private readonly IMouseCatcher mouseCatcher;
    private readonly Dictionary<string, PublicChatModel> publicChannels = new Dictionary<string, PublicChatModel>();
    private readonly Dictionary<string, UserProfile> recipientsFromPrivateChats = new Dictionary<string, UserProfile>();
    private readonly Dictionary<string, ChatMessage> lastPrivateMessages = new Dictionary<string, ChatMessage>();
    internal BaseVariable<HashSet<string>> visibleTaskbarPanels => dataStore.HUDs.visibleTaskbarPanels;

    private int hiddenDMs;
    private string currentSearch = "";
    private DateTime channelsRequestTimestamp;
    private bool areDMsRequestedByFirstTime;
    private bool isRequestingFriendsWithDMs;
    private IWorldChatWindowView view;
    private UserProfile ownUserProfile;
    private bool isRequestingDMs;
    private bool areJoinedChannelsRequestedByFirstTime;
    private CancellationTokenSource hideChannelsLoadingCancellationToken = new CancellationTokenSource();

    public IWorldChatWindowView View => view;

    public event Action OnCloseView;
    public event Action<string> OnOpenPrivateChat;
    public event Action<string> OnOpenPublicChat;
    public event Action<string> OnOpenChannel;
    public event Action OnOpen;
    public event Action OnOpenChannelSearch;
    public event Action OnOpenChannelCreation;
    public event Action<string> OnOpenChannelLeave;

    public WorldChatWindowController(
        IUserProfileBridge userProfileBridge,
        IFriendsController friendsController,
        IChatController chatController,
        DataStore dataStore,
        IMouseCatcher mouseCatcher) 
    {
        this.userProfileBridge = userProfileBridge;
        this.friendsController = friendsController;
        this.chatController = chatController;
        this.dataStore = dataStore;
        this.mouseCatcher = mouseCatcher;
    }

    public void Initialize(IWorldChatWindowView view)
    {
        this.view = view;
        view.Initialize(chatController);

        if (mouseCatcher != null)
            mouseCatcher.OnMouseLock += HandleViewCloseRequest;

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
        friendsController.OnUpdateFriendship += HandleFriendshipUpdated;
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
        friendsController.OnUpdateFriendship -= HandleFriendshipUpdated;
        friendsController.OnInitialized -= HandleFriendsControllerInitialization;

        if (ownUserProfile != null)
            ownUserProfile.OnUpdate -= OnUserProfileUpdate;
        
        hideChannelsLoadingCancellationToken?.Cancel();
        hideChannelsLoadingCancellationToken?.Dispose();
    }

    public void SetVisibility(bool visible)
    {
        SetVisiblePanelList(visible);
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

    private void SetVisiblePanelList(bool visible)
    {
        HashSet<string> newSet = visibleTaskbarPanels.Get();
        if (visible)
            newSet.Add("WorldChatPanel");
        else
            newSet.Remove("WorldChatPanel");

        visibleTaskbarPanels.Set(newSet, true);
    }

    private void LeaveChannel(string channelId) => OnOpenChannelLeave?.Invoke(channelId);

    private void RequestJoinedChannels()
    {
        if ((DateTime.UtcNow - channelsRequestTimestamp).TotalSeconds < 3) return;
        
        // skip=0: we do not support pagination for channels, it is supposed that a user can have a limited amount of joined channels
        chatController.GetJoinedChannels(CHANNELS_PAGE_SIZE, 0);
        channelsRequestTimestamp = DateTime.UtcNow;

        areJoinedChannelsRequestedByFirstTime = true;
        
        hideChannelsLoadingCancellationToken?.Cancel();
        hideChannelsLoadingCancellationToken = new CancellationTokenSource();
        WaitThenHideChannelsLoading(hideChannelsLoadingCancellationToken.Token).Forget();
    }

    private async UniTask WaitThenHideChannelsLoading(CancellationToken cancellationToken)
    {
        await UniTask.Delay(3000, cancellationToken: cancellationToken);
        if (cancellationToken.IsCancellationRequested) return;
        view.HideChannelsLoading();
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

    private void OpenPublicChat(string channelId)
    {
        if (channelId == NEARBY_CHANNEL_ID)
            OnOpenPublicChat?.Invoke(channelId);
        else
            OnOpenChannel?.Invoke(channelId);
    }

    private void HandleViewCloseRequest()
    {
        OnCloseView?.Invoke();
        SetVisibility(false);
    }

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

        isRequestingDMs = false;
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
                .ToDictionary(model => model.userId,
                    profile => CreatePrivateChatModel(lastPrivateMessages[profile.userId], profile));
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

        if (hiddenDMs <= 0 || !string.IsNullOrEmpty(currentSearch))
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