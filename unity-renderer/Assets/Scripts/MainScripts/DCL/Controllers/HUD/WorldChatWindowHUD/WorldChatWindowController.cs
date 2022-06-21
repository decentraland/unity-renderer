using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DCL.Interface;

public class WorldChatWindowController : IHUD
{
    private const string GENERAL_CHANNEL_ID = "general";
    private const int MAX_SEARCHED_CHANNELS = 100;
    private const int USER_DM_ENTRIES_TO_REQUEST_FOR_INITIAL_LOAD = 50;
    private const int USER_DM_ENTRIES_TO_REQUEST_FOR_SHOW_MORE = 20;
    private const int USER_DM_ENTRIES_TO_REQUEST_FOR_SEARCH = 20;
    private const int USER_PRIVATE_MESSAGES_TO_REQUEST_FOR_INITIAL_LOAD = 30;
    private const int USER_PRIVATE_MESSAGES_TO_REQUEST_FOR_SHOW_MORE = 10;

    private readonly IUserProfileBridge userProfileBridge;
    private readonly IFriendsController friendsController;
    private readonly IChatController chatController;
    private readonly ILastReadMessagesService lastReadMessagesService;
    private readonly Dictionary<string, PublicChatChannelModel> publicChannels = new Dictionary<string, PublicChatChannelModel>();
    private readonly Dictionary<string, UserProfile> recipientsFromPrivateChats = new Dictionary<string, UserProfile>();
    private readonly Dictionary<string, ChatMessage> lastPrivateMessages = new Dictionary<string, ChatMessage>();
    private bool areDMsRequestedByFirstTime = false;
    private List<string> directMessagesAlreadyRequested = new List<string>();
    private bool isPendingToRequestMoreDMs = true;
    private int lastAmountOfFriendsWithDMsRequested = 0;
    private long olderDMTimestampRequested = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    private string currentSearch = "";

    private IWorldChatWindowView view;
    private UserProfile ownUserProfile;

    public IWorldChatWindowView View => view;

    public event Action<string> OnOpenPrivateChat;
    public event Action<string> OnOpenPublicChannel;
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
        view.OnUnfriend += HandleUnfriend;
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
        friendsController.OnInitialized += HandleFriendsControllerInitialization;
    }

    public void Dispose()
    {
        view.OnClose -= HandleViewCloseRequest;
        view.OnOpenPrivateChat -= OpenPrivateChat;
        view.OnOpenPublicChannel -= OpenPublicChannel;
        view.OnUnfriend -= HandleUnfriend;
        view.OnSearchChannelRequested -= SearchChannels;
        view.OnRequireMorePrivateChats -= ShowMorePrivateChats;
        view.Dispose();
        chatController.OnAddMessage -= HandleMessageAdded;
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
                    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        }
        else
            view.Hide();
    }

    private void HandleUnfriend(string friendId)
    {
        friendsController.RemoveFriend(friendId);
    }

    private void HandleFriendsControllerInitialization()
    {
        if (view.IsActive && !areDMsRequestedByFirstTime)
        {
            RequestFriendsWithDirectMessages(
                USER_DM_ENTRIES_TO_REQUEST_FOR_INITIAL_LOAD,
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }
        else
            view.HidePrivateChatsLoading();

        // show only private chats from friends. Change it whenever the catalyst supports to send pms to any user
        foreach (var userId in recipientsFromPrivateChats.Keys)
            if (!friendsController.IsFriend(userId))
                view.RemovePrivateChat(userId);
    }

    private void OpenPrivateChat(string userId)
    {
        if (!directMessagesAlreadyRequested.Contains(userId))
        {
            RequestPrivateMessages(
                userId,
                USER_PRIVATE_MESSAGES_TO_REQUEST_FOR_INITIAL_LOAD,
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }

        OnOpenPrivateChat?.Invoke(userId);
    }

    private void OpenPublicChannel(string channelId) => OnOpenPublicChannel?.Invoke(channelId);

    private void HandleViewCloseRequest() => SetVisibility(false);
    
    private void HandleUserStatusChanged(string userId, FriendsController.UserStatus status)
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
        if (string.IsNullOrEmpty(currentSearch))
            isPendingToRequestMoreDMs = usersWithDM.Count() >= lastAmountOfFriendsWithDMsRequested;

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

            if (string.IsNullOrEmpty(currentSearch) && usersWithDM[i].lastMessageTimestamp < olderDMTimestampRequested)
                olderDMTimestampRequested = usersWithDM[i].lastMessageTimestamp;

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
                .ToDictionary(model => model.userId, profile => CreatePrivateChatModel(lastPrivateMessages[profile.userId], profile));
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
        RequestFriendsWithDirectMessages(
            USER_DM_ENTRIES_TO_REQUEST_FOR_SHOW_MORE,
            olderDMTimestampRequested);
    }
    
    private void UpdateMoreChannelsToLoadHint()
    {
        if (!isPendingToRequestMoreDMs || !string.IsNullOrEmpty(currentSearch))
            View.HideMoreChatsToLoadHint();
        else
            View.ShowMoreChatsToLoadHint();
    }

    private void RequestFriendsWithDirectMessages(int limit, long fromTimestamp)
    {
        if (!areDMsRequestedByFirstTime)
        {
            view.ShowPrivateChatsLoading();
            view.HideMoreChatsToLoadHint();
        }
        else
            view.ShowMoreChatsLoading();

        friendsController.GetFriendsWithDirectMessages(limit, fromTimestamp);
        areDMsRequestedByFirstTime = true;
        lastAmountOfFriendsWithDMsRequested = limit;
    }

    private void RequestFriendsWithDirectMessagesFromSearch(string userNameOrId, int limit)
    {
        view.ShowSearchLoading();
        friendsController.GetFriendsWithDirectMessages(userNameOrId, limit);
    }

    private void RequestPrivateMessages(string userId, int limit, long fromTimestamp)
    {
        chatController.GetPrivateMessages(userId, limit, fromTimestamp);
        directMessagesAlreadyRequested.Add(userId);
    }
}