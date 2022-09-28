using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL.Chat;
using DCL.Friends.WebApi;
using DCL.Interface;
using UnityEngine;

public class WorldChatWindowController : IHUD
{
    private const int DMS_PAGE_SIZE = 30;
    private const int USER_DM_ENTRIES_TO_REQUEST_FOR_SEARCH = 30;

    private readonly IUserProfileBridge userProfileBridge;
    private readonly IFriendsController friendsController;
    private readonly IChatController chatController;

    private readonly Dictionary<string, PublicChatChannelModel> publicChannels =
        new Dictionary<string, PublicChatChannelModel>();

    private readonly Dictionary<string, ChatMessage> lastPrivateMessages = new Dictionary<string, ChatMessage>();
    private int hiddenDMs;
    private string currentSearch = "";
    private bool areDMsRequestedByFirstTime;
    private bool isRequestingFriendsWithDMs;
    private int lastSkipForDMs;
    private CancellationTokenSource hideLoadingCancellationToken = new CancellationTokenSource();
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
        publicChannels[ChatUtils.NEARBY_CHANNEL_ID] = new PublicChatChannelModel(ChatUtils.NEARBY_CHANNEL_ID, ChatUtils.NEARBY_CHANNEL_ID,
            "Talk to the people around you. If you move far away from someone you will lose contact. All whispers will be displayed.");
        view.SetPublicChannel(publicChannels[ChatUtils.NEARBY_CHANNEL_ID]);

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
                RequestFriendsWithDirectMessages();
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
            RequestFriendsWithDirectMessages();
            lastSkipForDMs += DMS_PAGE_SIZE;
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
        if (friendship != FriendshipAction.APPROVED)
        {
            // show only private chats from friends. Change it whenever the catalyst supports to send pms to any user
            view.RemovePrivateChat(userId);
        }
    }

    private void HandleUserStatusChanged(string userId, UserStatus status)
    {
        if (!lastPrivateMessages.ContainsKey(userId)) return;

        if (status.friendshipStatus != FriendshipStatus.FRIEND)
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

        view.SetPrivateChat(CreatePrivateChatModel(message, profile));
    }

    private void HandleFriendsWithDirectMessagesAdded(List<FriendWithDirectMessages> usersWithDM)
    {
        for (var i = 0; i < usersWithDM.Count; i++)
        {
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

            view.SetPrivateChat(CreatePrivateChatModel(lastMessage, profile));
        }

        UpdateMoreChannelsToLoadHint();
        view.HidePrivateChatsLoading();
        view.HideSearchLoading();

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
            View.DisableSearchMode();
            UpdateMoreChannelsToLoadHint();
            return;
        }

        UpdateMoreChannelsToLoadHint();
        View.EnableSearchMode();
        RequestFriendsWithDirectMessagesFromSearch(search, USER_DM_ENTRIES_TO_REQUEST_FOR_SEARCH);
    }

    private void ShowMorePrivateChats()
    {
        if (isRequestingFriendsWithDMs ||
            hiddenDMs == 0 ||
            !string.IsNullOrEmpty(currentSearch))
            return;

        RequestFriendsWithDirectMessages();
    }

    private void UpdateMoreChannelsToLoadHint()
    {
        hiddenDMs = Mathf.Clamp(friendsController.TotalFriendsWithDirectMessagesCount - lastSkipForDMs,
            0,
            friendsController.TotalFriendsWithDirectMessagesCount);

        if (hiddenDMs <= 0 || !string.IsNullOrEmpty(currentSearch))
            View.HideMoreChatsToLoadHint();
        else
            View.ShowMoreChatsToLoadHint(hiddenDMs);
    }

    private void RequestFriendsWithDirectMessages()
    {
        isRequestingFriendsWithDMs = true;

        if (!areDMsRequestedByFirstTime)
        {
            view.ShowPrivateChatsLoading();
            view.HideMoreChatsToLoadHint();
        }

        friendsController.GetFriendsWithDirectMessages(DMS_PAGE_SIZE, lastSkipForDMs);
        lastSkipForDMs += DMS_PAGE_SIZE;
        areDMsRequestedByFirstTime = true;
    }

    internal void RequestFriendsWithDirectMessagesFromSearch(string userNameOrId, int limit)
    {
        view.ShowSearchLoading();
        friendsController.GetFriendsWithDirectMessages(userNameOrId, limit);
        hideLoadingCancellationToken?.Cancel();
        hideLoadingCancellationToken = new CancellationTokenSource();
        HideSearchLoadingWhenTimeout(hideLoadingCancellationToken.Token).Forget();
    }

    private void RequestUnreadMessages() => chatController.GetUnseenMessagesByUser();

    private async UniTask HideSearchLoadingWhenTimeout(CancellationToken cancellationToken)
    {
        await UniTask.Delay(3000, cancellationToken: cancellationToken);
        if (cancellationToken.IsCancellationRequested) return;
        view.HideSearchLoading();
    }
}