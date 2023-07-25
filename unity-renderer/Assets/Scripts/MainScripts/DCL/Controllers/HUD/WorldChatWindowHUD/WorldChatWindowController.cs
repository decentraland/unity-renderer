using Cysharp.Threading.Tasks;
using DCL.Chat;
using DCL.Chat.Channels;
using DCL.Interface;
using SocialFeaturesAnalytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DCL.Browser;
using DCl.Social.Friends;
using DCL.Social.Friends;
using UnityEngine;
using Channel = DCL.Chat.Channels.Channel;
using DCL.Tasks;

namespace DCL.Social.Chat
{
    public class WorldChatWindowController : IHUD
    {
        private const int DMS_PAGE_SIZE = 30;
        private const int USER_DM_ENTRIES_TO_REQUEST_FOR_SEARCH = 20;
        private const int CHANNELS_PAGE_SIZE = 10;
        private const int MINUTES_FOR_AUTOMATIC_CHANNELS_INFO_RELOADING = 1;

        private readonly IUserProfileBridge userProfileBridge;
        private readonly IFriendsController friendsController;
        private readonly IChatController chatController;
        private readonly DataStore dataStore;
        private readonly IMouseCatcher mouseCatcher;
        private readonly ISocialAnalytics socialAnalytics;
        private readonly IChannelsFeatureFlagService channelsFeatureFlagService;
        private readonly IBrowserBridge browserBridge;
        private readonly RendererState rendererState;
        private readonly DataStore_Mentions mentionsDataStore;
        private readonly IClipboard clipboard;
        private readonly Dictionary<string, PublicChatModel> publicChannels = new Dictionary<string, PublicChatModel>();
        private readonly Dictionary<string, ChatMessage> lastPrivateMessages = new Dictionary<string, ChatMessage>();
        private readonly HashSet<string> channelsClearedUnseenNotifications = new HashSet<string>();
        private HashSet<string> autoJoinChannelList => dataStore.HUDs.autoJoinChannelList.Get();
        private BaseVariable<HashSet<string>> visibleTaskbarPanels => dataStore.HUDs.visibleTaskbarPanels;
        private int hiddenDMs;
        private string currentSearch = "";
        private DateTime channelsRequestTimestamp;
        private bool areDMsRequestedByFirstTime;
        private int lastSkipForDMs;
        private CancellationTokenSource hideLoadingCancellationToken = new CancellationTokenSource();
        private IWorldChatWindowView view;
        private UserProfile ownUserProfile;
        private bool isRequestingDMs;
        private bool areUnseenMessagesRequestedByFirstTime;
        private string channelToJoinAtTheBeginning;
        private CancellationTokenSource hideChannelsLoadingCancellationToken = new CancellationTokenSource();
        private CancellationTokenSource hidePrivateChatsLoadingCancellationToken = new CancellationTokenSource();
        private CancellationTokenSource reloadingChannelsInfoCancellationToken = new CancellationTokenSource();
        private CancellationTokenSource showDMsCancellationToken = new ();
        private bool showOnlyOnlineMembersOnPublicChannels => !dataStore.featureFlags.flags.Get().IsFeatureEnabled("matrix_presence_disabled");

        private bool isVisible = true;
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
            IMouseCatcher mouseCatcher,
            ISocialAnalytics socialAnalytics,
            IChannelsFeatureFlagService channelsFeatureFlagService,
            IBrowserBridge browserBridge,
            RendererState rendererState,
            DataStore_Mentions mentionsDataStore,
            IClipboard clipboard)
        {
            this.userProfileBridge = userProfileBridge;
            this.friendsController = friendsController;
            this.chatController = chatController;
            this.dataStore = dataStore;
            this.mouseCatcher = mouseCatcher;
            this.socialAnalytics = socialAnalytics;
            this.channelsFeatureFlagService = channelsFeatureFlagService;
            this.browserBridge = browserBridge;
            this.rendererState = rendererState;
            this.mentionsDataStore = mentionsDataStore;
            this.clipboard = clipboard;
        }

        public void Initialize(IWorldChatWindowView view, bool isVisible = true)
        {
            this.view = view;
            view.Initialize(chatController, mentionsDataStore);

            if (mouseCatcher != null)
                mouseCatcher.OnMouseLock += HandleViewCloseRequest;

            view.OnClose += HandleViewCloseRequest;
            view.OnOpenPrivateChat += OpenPrivateChat;
            view.OnOpenPublicChat += OpenPublicChat;
            view.OnSearchChatRequested += SearchChats;
            view.OnRequireMorePrivateChats += ShowMorePrivateChats;
            view.OnSignUp += SignUp;
            view.OnRequireWalletReadme += OpenWalletReadme;

            ownUserProfile = userProfileBridge.GetOwn();
            if (ownUserProfile != null)
                ownUserProfile.OnUpdate += OnUserProfileUpdate;

            var channel = chatController.GetAllocatedChannel(ChatUtils.NEARBY_CHANNEL_ID);
            publicChannels[ChatUtils.NEARBY_CHANNEL_ID] = new PublicChatModel(ChatUtils.NEARBY_CHANNEL_ID, channel.Name,
                channel.Description,
                channel.Joined,
                channel.MemberCount,
                false,
                showOnlyOnlineMembersOnPublicChannels);
            view.SetPublicChat(publicChannels[ChatUtils.NEARBY_CHANNEL_ID]);

            if (!friendsController.IsInitialized)
                if (ownUserProfile?.hasConnectedWeb3 ?? false)
                    view.ShowPrivateChatsLoading();

            chatController.OnInitialized += HandleChatInitialization;
            chatController.OnAddMessage += HandleMessageAdded;
            friendsController.OnAddFriendsWithDirectMessages += HandleFriendsWithDirectMessagesAdded;
            friendsController.OnUpdateUserStatus += HandleUserStatusChanged;
            friendsController.OnUpdateFriendship += HandleFriendshipUpdated;
            friendsController.OnInitialized += HandleFriendsControllerInitialization;

            if (channelsFeatureFlagService.IsChannelsFeatureEnabled())
            {
                channelsFeatureFlagService.OnAllowedToCreateChannelsChanged += OnAllowedToCreateChannelsChanged;

                view.OnOpenChannelSearch += OpenChannelSearch;
                view.OnLeaveChannel += LeaveChannel;
                view.OnCreateChannel += OpenChannelCreationWindow;
                view.OnCopyChannelNameRequested += CopyChannelNameToClipboard;

                chatController.OnChannelUpdated += HandleChannelUpdated;
                chatController.OnChannelJoined += HandleChannelJoined;
                chatController.OnAutoChannelJoined += HandleAutoChannelJoined;
                chatController.OnJoinChannelError += HandleJoinChannelError;
                chatController.OnChannelLeaveError += HandleLeaveChannelError;
                chatController.OnChannelLeft += HandleChannelLeft;
                chatController.OnAskForJoinChannel += HandleAskForJoinChannel;
                dataStore.channels.channelToBeOpened.OnChange += HandleChannelOpened;

                view.ShowChannelsLoading();
                view.SetSearchAndCreateContainerActive(true);
            }
            else
            {
                view.HideChannelsLoading();
                view.SetSearchAndCreateContainerActive(false);
            }

            view.SetChannelsPromoteLabelVisible(channelsFeatureFlagService.IsPromoteChannelsToastEnabled());

            SetVisibility(isVisible);
            this.isVisible = isVisible;
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
            view.OnSignUp -= SignUp;
            view.OnRequireWalletReadme -= OpenWalletReadme;
            view.Dispose();
            chatController.OnInitialized -= HandleChatInitialization;
            chatController.OnAddMessage -= HandleMessageAdded;
            chatController.OnChannelUpdated -= HandleChannelUpdated;
            chatController.OnChannelJoined -= HandleChannelJoined;
            chatController.OnAutoChannelJoined -= HandleAutoChannelJoined;
            chatController.OnJoinChannelError -= HandleJoinChannelError;
            chatController.OnChannelLeaveError -= HandleLeaveChannelError;
            chatController.OnChannelLeft -= HandleChannelLeft;
            chatController.OnAskForJoinChannel -= HandleAskForJoinChannel;
            friendsController.OnAddFriendsWithDirectMessages -= HandleFriendsWithDirectMessagesAdded;
            friendsController.OnUpdateUserStatus -= HandleUserStatusChanged;
            friendsController.OnUpdateFriendship -= HandleFriendshipUpdated;
            friendsController.OnInitialized -= HandleFriendsControllerInitialization;
            dataStore.channels.channelToBeOpened.OnChange -= HandleChannelOpened;
            rendererState.OnChange -= HandleAskForJoinChannelAfterRendererState;

            if (ownUserProfile != null)
                ownUserProfile.OnUpdate -= OnUserProfileUpdate;

            hideChannelsLoadingCancellationToken?.SafeCancelAndDispose();
            reloadingChannelsInfoCancellationToken.SafeCancelAndDispose();
            showDMsCancellationToken.SafeCancelAndDispose();

            channelsFeatureFlagService.OnAllowedToCreateChannelsChanged -= OnAllowedToCreateChannelsChanged;
        }

        public void SetVisibility(bool visible)
        {
            if (isVisible == visible)
                return;

            isVisible = visible;
            SetVisiblePanelList(visible);

            if (visible)
            {
                view.Show();
                OnOpen?.Invoke();

                if (friendsController.IsInitialized && !areDMsRequestedByFirstTime)
                {
                    RequestFriendsWithDirectMessages();
                    RequestUnreadMessages();
                }

                if (channelsFeatureFlagService.IsChannelsFeatureEnabled())
                {
                    if (chatController.IsInitialized)
                    {
                        RequestJoinedChannels();
                        SetAutomaticChannelsInfoUpdatingActive(true);
                    }
                    else if (ownUserProfile.isGuest)
                    {
                        // TODO: channels are not allowed for guests. When we support it in the future, remove this call
                        view.HideChannelsLoading();
                    }

                    if (!areUnseenMessagesRequestedByFirstTime)
                        RequestUnreadChannelsMessages();
                }

                if (ownUserProfile?.isGuest ?? false)
                    view.ShowConnectWallet();
                else
                    view.HideConnectWallet();
            }
            else
            {
                view.DisableSearchMode();
                view.Hide();
                SetAutomaticChannelsInfoUpdatingActive(false);
            }
        }

        private void OpenChannelCreationWindow()
        {
            dataStore.channels.channelJoinedSource.Set(ChannelJoinedSource.ConversationList);
            OnOpenChannelCreation?.Invoke();
        }

        private void SetVisiblePanelList(bool visible)
        {
            HashSet<string> newSet = visibleTaskbarPanels.Get();
            if (visible)
                newSet.Add("WorldChatPanel");
            else
                newSet.Remove("WorldChatPanel");

            visibleTaskbarPanels.Set(newSet, true);
        }

        private void LeaveChannel(string channelId)
        {
            dataStore.channels.channelLeaveSource.Set(ChannelLeaveSource.ConversationList);
            OnOpenChannelLeave?.Invoke(channelId);
        }

        private void RequestJoinedChannels()
        {
            if ((DateTime.UtcNow - channelsRequestTimestamp).TotalSeconds < 3) return;

            // skip=0: we do not support pagination for channels, it is supposed that a user can have a limited amount of joined channels
            chatController.GetJoinedChannels(CHANNELS_PAGE_SIZE, 0);
            channelsRequestTimestamp = DateTime.UtcNow;

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

        private void HandleChatInitialization()
        {
            // we do request joined channels as soon as possible to be able to display messages correctly in the notification panel
            RequestJoinedChannels();
            ConnectToAutoJoinChannels();
        }

        private void ConnectToAutoJoinChannels()
        {
            AutomaticJoinChannelList joinChannelList = channelsFeatureFlagService.GetAutoJoinChannelsList();
            if (joinChannelList == null) return;
            if (joinChannelList.automaticJoinChannelList == null) return;

            foreach (var channel in joinChannelList.automaticJoinChannelList)
            {
                var channelId = channel.channelId;
                if (string.IsNullOrEmpty(channelId)) continue;
                autoJoinChannelList.Add(channelId);
                chatController.JoinOrCreateChannel(channelId);
                if (!channel.enableNotifications)
                    chatController.MuteChannel(channelId);
            }
        }

        private void HandleFriendsControllerInitialization()
        {
            if (view.IsActive && !areDMsRequestedByFirstTime)
            {
                RequestFriendsWithDirectMessages();
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
            if (channelId == ChatUtils.NEARBY_CHANNEL_ID)
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
            if (friendship != FriendshipAction.APPROVED)
            {
                // show only private chats from friends. Change it whenever the catalyst supports to send pms to any user
                view.RemovePrivateChat(userId);
            }
        }

        private void HandleUserStatusChanged(string userId, UserStatus status)
        {
            if (status.friendshipStatus != FriendshipStatus.FRIEND)
            {
                // show only private chats from friends. Change it whenever the catalyst supports to send pms to any user
                view.RemovePrivateChat(userId);
            }
            else
            {
                view.RefreshPrivateChatPresence(userId, status.presence == PresenceStatus.ONLINE);
            }
        }

        private void HandleMessageAdded(ChatMessage[] messages)
        {
            async UniTaskVoid AddMessages(ChatMessage[] messages, CancellationToken cancellationToken)
            {
                foreach (var message in messages)
                {
                    if (message.messageType != ChatMessage.Type.PRIVATE) continue;

                    var userId = ExtractRecipientId(message);

                    if (lastPrivateMessages.ContainsKey(userId))
                    {
                        if (message.timestamp > lastPrivateMessages[userId].timestamp)
                            lastPrivateMessages[userId] = message;
                    }
                    else
                        lastPrivateMessages[userId] = message;

                    try
                    {
                        // incoming friend request's message is added as a DM. This check filters it
                        if (await friendsController.GetFriendshipStatus(userId, cancellationToken) != FriendshipStatus.FRIEND) return;

                        var profile = userProfileBridge.Get(userId)
                                      ?? await userProfileBridge.RequestFullUserProfileAsync(userId, cancellationToken);

                        view.SetPrivateChat(CreatePrivateChatModel(message, profile));
                    }
                    catch (Exception e) when (e is not OperationCanceledException)
                    {
                        Debug.LogException(e);
                    }
                }
            }

            AddMessages(messages, showDMsCancellationToken.Token).Forget();
        }

        private void HandleFriendsWithDirectMessagesAdded(List<FriendWithDirectMessages> usersWithDM)
        {
            async UniTaskVoid HandleFriendsWithDirectMessagesAddedAsync(List<FriendWithDirectMessages> usersWithDM, CancellationToken cancellationToken)
            {
                for (var i = 0; i < usersWithDM.Count; i++)
                {
                    FriendWithDirectMessages dm = usersWithDM[i];

                    var lastMessage = new ChatMessage
                    {
                        messageType = ChatMessage.Type.PRIVATE,
                        body = dm.lastMessageBody,
                        timestamp = (ulong) dm.lastMessageTimestamp
                    };

                    string userId = dm.userId;

                    if (lastPrivateMessages.ContainsKey(userId))
                    {
                        if (lastMessage.timestamp > lastPrivateMessages[userId].timestamp)
                            lastPrivateMessages[userId] = lastMessage;
                    }
                    else
                        lastPrivateMessages[userId] = lastMessage;

                    var profile = userProfileBridge.Get(userId);

                    try { profile ??= await userProfileBridge.RequestFullUserProfileAsync(userId, cancellationToken); }
                    catch (Exception e) when (e is not OperationCanceledException)
                    {
                        var fallbackDM = new PrivateChatModel
                        {
                            recentMessage = lastMessage,
                            faceSnapshotUrl = "",
                            isBlocked = ownUserProfile.IsBlocked(userId),
                            isOnline = false,
                            userId = userId,
                            userName = userId,
                        };

                        view.SetPrivateChat(fallbackDM);

                        throw;
                    }

                    view.SetPrivateChat(CreatePrivateChatModel(lastMessage, profile));
                }

                UpdateMoreChannelsToLoadHint();
                view.HidePrivateChatsLoading();
                view.HideSearchLoading();

                isRequestingDMs = false;
            }

            HandleFriendsWithDirectMessagesAddedAsync(usersWithDM, showDMsCancellationToken.Token).Forget();
        }

        private PrivateChatModel CreatePrivateChatModel(ChatMessage recentMessage, UserProfile profile)
        {
            return new PrivateChatModel
            {
                userId = profile.userId,
                userName = profile.userName,
                faceSnapshotUrl = profile.face256SnapshotURL,
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

            if (profile.isGuest)
                view.ShowConnectWallet();
            else
                view.HideConnectWallet();
        }

        private void SearchChats(string search)
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

            var matchedChannels = publicChannels.Values
                                                .Where(model => model.name.ToLower().Contains(search.ToLower()));
            foreach (var channelMatch in matchedChannels)
                View.SetPublicChat(channelMatch);

            RequestFriendsWithDirectMessagesFromSearch(search, USER_DM_ENTRIES_TO_REQUEST_FOR_SEARCH);
        }

        private void ShowMorePrivateChats()
        {
            if (isRequestingDMs ||
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
            isRequestingDMs = true;

            if (!areDMsRequestedByFirstTime)
            {
                view.ShowPrivateChatsLoading();
                view.HideMoreChatsToLoadHint();
            }

            friendsController.GetFriendsWithDirectMessages(DMS_PAGE_SIZE, lastSkipForDMs);
            lastSkipForDMs += DMS_PAGE_SIZE;
            areDMsRequestedByFirstTime = true;

            hidePrivateChatsLoadingCancellationToken.Cancel();
            hidePrivateChatsLoadingCancellationToken = new CancellationTokenSource();
            HidePrivateChatsLoadingWhenTimeout(hidePrivateChatsLoadingCancellationToken.Token).Forget();
        }

        private async UniTaskVoid HidePrivateChatsLoadingWhenTimeout(CancellationToken cancellationToken)
        {
            await UniTask.Delay(3000, cancellationToken: cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            view.HidePrivateChatsLoading();
        }

        internal void RequestFriendsWithDirectMessagesFromSearch(string userNameOrId, int limit)
        {
            view.ShowSearchLoading();
            friendsController.GetFriendsWithDirectMessages(userNameOrId, limit);
            hideLoadingCancellationToken?.Cancel();
            hideLoadingCancellationToken = new CancellationTokenSource();
            HideSearchLoadingWhenTimeout(hideLoadingCancellationToken.Token).Forget();
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
            var model = new PublicChatModel(channelId, channel.Name, channel.Description, channel.Joined,
                channel.MemberCount, channel.Muted, showOnlyOnlineMembersOnPublicChannels);

            if (publicChannels.ContainsKey(channelId))
                publicChannels[channelId].CopyFrom(model);
            else
                publicChannels[channelId] = model;

            view.SetPublicChat(model);
            view.HideChannelsLoading();

            // we clear the unseen messages to avoid showing many of them while the user was offline
            // TODO: we should consider avoid clearing when the channel is private in the future
            ClearOfflineUnseenMessages(channelId);
        }

        private void ClearOfflineUnseenMessages(string channelId)
        {
            if (channelsClearedUnseenNotifications.Contains(channelId)) return;
            chatController.MarkChannelMessagesAsSeen(channelId);
            channelsClearedUnseenNotifications.Add(channelId);
        }

        private void HandleAutoChannelJoined(Channel channel) => ReportChannelJoinedToAnalytics(channel, "auto");

        private void HandleChannelJoined(Channel channel)
        {
            ReportChannelJoinedToAnalytics(channel, "manual");
            OpenPublicChat(channel.ChannelId);
        }

        private void ReportChannelJoinedToAnalytics(Channel channel, string method)
        {
            if (channel.MemberCount <= 1)
                socialAnalytics.SendEmptyChannelCreated(channel.Name, dataStore.channels.channelJoinedSource.Get());
            else
                socialAnalytics.SendPopulatedChannelJoined(channel.Name, dataStore.channels.channelJoinedSource.Get(), method);
        }

        private void HandleJoinChannelError(string channelId, ChannelErrorCode errorCode)
        {
            if (dataStore.channels.isCreationModalVisible.Get()) return;

            switch (errorCode)
            {
                case ChannelErrorCode.LimitExceeded:
                    dataStore.channels.currentChannelLimitReached.Set(channelId, true);
                    break;
                case ChannelErrorCode.Unknown:
                    dataStore.channels.joinChannelError.Set(channelId, true);
                    break;
            }
        }

        private void HandleLeaveChannelError(string channelId, ChannelErrorCode errorCode) =>
            dataStore.channels.leaveChannelError.Set(channelId, true);

        private void HandleChannelLeft(string channelId)
        {
            publicChannels.Remove(channelId);
            view.RemovePublicChat(channelId);
            var channel = chatController.GetAllocatedChannel(channelId);
            socialAnalytics.SendLeaveChannel(channel?.Name ?? channelId, dataStore.channels.channelLeaveSource.Get());
        }

        private void HandleChannelOpened(string channelId, string previousChannelId)
        {
            if (string.IsNullOrEmpty(channelId))
                return;

            OpenPublicChat(channelId);
        }

        private void HandleAskForJoinChannel(string channelName)
        {
            chatController.OnAskForJoinChannel -= HandleAskForJoinChannel;

            if (!channelsFeatureFlagService.IsAllowedToCreateChannels())
                return;

            if (rendererState.Get())
                dataStore.channels.currentJoinChannelModal.Set(channelName, true);
            else
            {
                channelToJoinAtTheBeginning = channelName;
                rendererState.OnChange += HandleAskForJoinChannelAfterRendererState;
            }
        }

        private void HandleAskForJoinChannelAfterRendererState(bool current, bool _)
        {
            if (!current)
                return;

            rendererState.OnChange -= HandleAskForJoinChannelAfterRendererState;
            dataStore.channels.currentJoinChannelModal.Set(channelToJoinAtTheBeginning, true);
        }

        private void RequestUnreadMessages() => chatController.GetUnseenMessagesByUser();

        private void RequestUnreadChannelsMessages()
        {
            chatController.GetUnseenMessagesByChannel();
            areUnseenMessagesRequestedByFirstTime = true;
        }

        private void OpenChannelSearch()
        {
            if (!ownUserProfile.isGuest)
                OnOpenChannelSearch?.Invoke();
            else
                dataStore.HUDs.connectWalletModalVisible.Set(true);
        }

        private async UniTask HideSearchLoadingWhenTimeout(CancellationToken cancellationToken)
        {
            await UniTask.Delay(3000, cancellationToken: cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            view.HideSearchLoading();
        }

        private void OnAllowedToCreateChannelsChanged(bool isAllowed) => view.SetCreateChannelButtonActive(isAllowed);

        private void SetAutomaticChannelsInfoUpdatingActive(bool isActive)
        {
            reloadingChannelsInfoCancellationToken.Cancel();

            if (isActive)
            {
                GetCurrentChannelsInfo();
                reloadingChannelsInfoCancellationToken = new CancellationTokenSource();
                ReloadChannelsInfoPeriodically(reloadingChannelsInfoCancellationToken.Token).Forget();
            }
        }

        private async UniTask ReloadChannelsInfoPeriodically(CancellationToken cancellationToken)
        {
            while (true)
            {
                await UniTask.Delay(MINUTES_FOR_AUTOMATIC_CHANNELS_INFO_RELOADING * 60 * 1000,
                    cancellationToken: cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    return;

                GetCurrentChannelsInfo();
            }
        }

        private void GetCurrentChannelsInfo()
        {
            chatController.GetChannelInfo(publicChannels
                                         .Select(x => x.Key)
                                         .Where(x => x != ChatUtils.NEARBY_CHANNEL_ID)
                                         .ToArray());
        }

        private void OpenWalletReadme() =>
            browserBridge.OpenUrl("https://docs.decentraland.org/player/blockchain-integration/get-a-wallet/");

        private void SignUp() => userProfileBridge.SignUp();

        private void CopyChannelNameToClipboard(string channelName)
        {
            clipboard.WriteText(channelName);
        }
    }
}
