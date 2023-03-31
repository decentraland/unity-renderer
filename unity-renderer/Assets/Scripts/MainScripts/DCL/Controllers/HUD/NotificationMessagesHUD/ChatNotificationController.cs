using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCL.Interface;
using DCL.ProfanityFiltering;
using DCL.SettingsCommon;
using DCL.Social.Chat.Mentions;
using DCL.Social.Friends;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using AudioSettings = DCL.SettingsCommon.AudioSettings;
using Channel = DCL.Chat.Channels.Channel;

namespace DCL.Chat.Notifications
{
    public class ChatNotificationController : IHUD
    {
        private const int FADEOUT_DELAY = 8000;
        private const string NEW_FRIEND_REQUESTS_FLAG = "new_friend_requests";

        private readonly DataStore dataStore;
        private readonly IChatController chatController;
        private readonly IFriendsController friendsController;
        private readonly IMainChatNotificationsComponentView mainChatNotificationView;
        private readonly ITopNotificationsComponentView topNotificationView;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly IProfanityFilter profanityFilter;
        private readonly ISettingsRepository<AudioSettings> audioSettings;
        private readonly TimeSpan maxNotificationInterval = new (0, 1, 0);
        private readonly HashSet<string> notificationEntries = new ();
        private readonly CancellationTokenSource addMessagesCancellationToken = new ();

        private BaseVariable<bool> shouldShowNotificationPanel => dataStore.HUDs.shouldShowNotificationPanel;
        private BaseVariable<Transform> notificationPanelTransform => dataStore.HUDs.notificationPanelTransform;
        private BaseVariable<Transform> topNotificationPanelTransform => dataStore.HUDs.topNotificationPanelTransform;
        private BaseVariable<HashSet<string>> visibleTaskbarPanels => dataStore.HUDs.visibleTaskbarPanels;
        private BaseVariable<string> openedChat => dataStore.HUDs.openChat;
        private CancellationTokenSource fadeOutCancellationToken = new ();
        private UserProfile internalOwnUserProfile;

        private UserProfile ownUserProfile
        {
            get
            {
                internalOwnUserProfile ??= userProfileBridge.GetOwn();
                return internalOwnUserProfile;
            }
        }

        private bool isNewFriendRequestsEnabled => dataStore.featureFlags.flags.Get().IsFeatureEnabled(NEW_FRIEND_REQUESTS_FLAG); // TODO (NEW FRIEND REQUESTS): remove when we don't need to keep the retro-compatibility with the old version

        public ChatNotificationController(DataStore dataStore,
            IMainChatNotificationsComponentView mainChatNotificationView,
            ITopNotificationsComponentView topNotificationView,
            IChatController chatController,
            IFriendsController friendsController,
            IUserProfileBridge userProfileBridge,
            IProfanityFilter profanityFilter,
            ISettingsRepository<AudioSettings> audioSettings)
        {
            this.dataStore = dataStore;
            this.chatController = chatController;
            this.friendsController = friendsController;
            this.userProfileBridge = userProfileBridge;
            this.profanityFilter = profanityFilter;
            this.audioSettings = audioSettings;
            this.mainChatNotificationView = mainChatNotificationView;
            this.topNotificationView = topNotificationView;
            mainChatNotificationView.OnResetFade += ResetFadeOut;
            topNotificationView.OnResetFade += ResetFadeOut;
            mainChatNotificationView.OnPanelFocus += TogglePanelBackground;
            mainChatNotificationView.OnClickedFriendRequest += HandleClickedFriendRequest;
            topNotificationView.OnClickedFriendRequest += HandleClickedFriendRequest;
            mainChatNotificationView.OnClickedChatMessage += OpenChat;
            topNotificationView.OnClickedChatMessage += OpenChat;
            chatController.OnAddMessage += HandleMessageAdded;
            friendsController.OnFriendRequestReceived += HandleFriendRequestReceived;
            friendsController.OnSentFriendRequestApproved += HandleSentFriendRequestApproved;
            notificationPanelTransform.Set(mainChatNotificationView.GetPanelTransform());
            topNotificationPanelTransform.Set(topNotificationView.GetPanelTransform());
            visibleTaskbarPanels.OnChange += VisiblePanelsChanged;
            shouldShowNotificationPanel.OnChange += ResetVisibility;
            ResetVisibility(shouldShowNotificationPanel.Get(), false);
        }

        public void SetVisibility(bool visible)
        {
            ResetFadeOut(visible);

            if (visible)
            {
                if (shouldShowNotificationPanel.Get())
                    mainChatNotificationView.Show();

                topNotificationView.Hide();
                mainChatNotificationView.ShowNotifications();
            }
            else
            {
                mainChatNotificationView.Hide();

                if (!visibleTaskbarPanels.Get().Contains("WorldChatPanel"))
                    topNotificationView.Show();
            }
        }

        public void Dispose()
        {
            chatController.OnAddMessage -= HandleMessageAdded;
            friendsController.OnFriendRequestReceived -= HandleFriendRequestReceived;
            friendsController.OnSentFriendRequestApproved -= HandleSentFriendRequestApproved;
            visibleTaskbarPanels.OnChange -= VisiblePanelsChanged;
            mainChatNotificationView.OnResetFade -= ResetFadeOut;
            topNotificationView.OnResetFade -= ResetFadeOut;
            mainChatNotificationView.OnClickedChatMessage -= OpenChat;
            topNotificationView.OnClickedChatMessage -= OpenChat;
            addMessagesCancellationToken.Cancel();
            addMessagesCancellationToken.Dispose();
        }

        private void VisiblePanelsChanged(HashSet<string> newList, HashSet<string> oldList)
        {
            SetVisibility(newList.Count == 0);
        }

        private void HandleMessageAdded(ChatMessage[] messages)
        {
            foreach (var message in messages)
            {
                if (message.messageType != ChatMessage.Type.PRIVATE &&
                    message.messageType != ChatMessage.Type.PUBLIC) return;

                var span = Utils.UnixToDateTimeWithTime((ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) -
                           Utils.UnixToDateTimeWithTime(message.timestamp);

                if (span >= maxNotificationInterval) return;

                var channel = chatController.GetAllocatedChannel(
                    string.IsNullOrEmpty(message.recipient) && message.messageType == ChatMessage.Type.PUBLIC
                        ? "nearby"
                        : message.recipient);

                if (channel?.Muted ?? false) return;

                // TODO: entries may have an inconsistent state. We should update the entry with new data
                if (notificationEntries.Contains(message.messageId)) return;
                notificationEntries.Add(message.messageId);

                AddNotificationAsync(message, channel, addMessagesCancellationToken.Token).Forget();
            }
        }

        private async UniTaskVoid AddNotificationAsync(ChatMessage message, Channel channel = null, CancellationToken cancellationToken = default)
        {
            string body = message.body;
            string openedChatId = openedChat.Get();
            bool isOwnPlayerMentioned = MentionsUtils.IsUserMentionedInText(ownUserProfile.userName, body);

            if (message.messageType == ChatMessage.Type.PRIVATE)
            {
                string peerId = ExtractPeerId(message);
                UserProfile peerProfile = userProfileBridge.Get(peerId);
                bool isMyMessage = message.sender == ownUserProfile.userId;
                UserProfile senderProfile = isMyMessage ? ownUserProfile : userProfileBridge.Get(message.sender);
                string peerName = peerProfile?.userName ?? peerId;
                string peerProfilePicture = peerProfile?.face256SnapshotURL;
                string senderName = senderProfile?.userName ?? message.sender;

                var privateModel = new PrivateChatMessageNotificationModel(
                    message.messageId,
                    isMyMessage ? peerId : message.sender,
                    body,
                    message.timestamp,
                    senderName,
                    peerName,
                    isMyMessage,
                    isOwnPlayerMentioned,
                    peerProfilePicture);

                mainChatNotificationView.AddNewChatNotification(privateModel);

                if (message.sender != openedChatId && message.recipient != openedChatId)
                    if (topNotificationPanelTransform.Get().gameObject.activeInHierarchy)
                        topNotificationView.AddNewChatNotification(privateModel);
            }
            else if (message.messageType == ChatMessage.Type.PUBLIC)
            {
                bool isMyMessage = message.sender == ownUserProfile.userId;
                UserProfile senderProfile = isMyMessage ? ownUserProfile : userProfileBridge.Get(message.sender);
                string senderName = senderProfile?.userName ?? message.sender;
                bool shouldPlayMentionSfx = isOwnPlayerMentioned;

                if (isOwnPlayerMentioned)
                {
                    AudioSettings.ChatNotificationType chatNotificationSfxType = audioSettings.Data.chatNotificationType;

                    shouldPlayMentionSfx = chatNotificationSfxType is AudioSettings.ChatNotificationType.All
                        or AudioSettings.ChatNotificationType.MentionsOnly;
                }

                if (IsProfanityFilteringEnabled())
                {
                    senderName = await profanityFilter.Filter(senderName, cancellationToken);
                    body = await profanityFilter.Filter(message.body, cancellationToken);
                }

                var publicModel = new PublicChannelMessageNotificationModel(
                    message.messageId,
                    body,
                    channel?.Name ?? message.recipient,
                    channel?.ChannelId,
                    message.timestamp,
                    isMyMessage,
                    senderName,
                    isOwnPlayerMentioned,
                    shouldPlayMentionSfx);

                mainChatNotificationView.AddNewChatNotification(publicModel);

                if ((string.IsNullOrEmpty(message.recipient) && openedChatId != ChatUtils.NEARBY_CHANNEL_ID)
                    || (!string.IsNullOrEmpty(message.recipient) && openedChatId != message.recipient))
                    if (topNotificationPanelTransform.Get().gameObject.activeInHierarchy)
                        topNotificationView.AddNewChatNotification(publicModel);
            }
        }

        private void HandleFriendRequestReceived(FriendRequest friendRequest)
        {
            if (!isNewFriendRequestsEnabled) return;

            if (friendRequest.From == ownUserProfile.userId ||
                friendRequest.To != ownUserProfile.userId)
                return;

            var friendRequestProfile = userProfileBridge.Get(friendRequest.From);
            var friendRequestName = friendRequestProfile?.userName ?? friendRequest.From;

            FriendRequestNotificationModel friendRequestNotificationModel = new FriendRequestNotificationModel(
                friendRequest.FriendRequestId,
                friendRequest.From,
                friendRequestName,
                "Friend Request received",
                "wants to be your friend.",
                (ulong)friendRequest.Timestamp,
                false);

            mainChatNotificationView.AddNewFriendRequestNotification(friendRequestNotificationModel);

            if (topNotificationPanelTransform.Get().gameObject.activeInHierarchy)
                topNotificationView.AddNewFriendRequestNotification(friendRequestNotificationModel);
        }

        private void HandleSentFriendRequestApproved(FriendRequest friendRequest)
        {
            if (!isNewFriendRequestsEnabled) return;

            string recipientUserId = friendRequest.To;
            var friendRequestProfile = userProfileBridge.Get(recipientUserId);

            FriendRequestNotificationModel friendRequestNotificationModel = new FriendRequestNotificationModel(
                friendRequest.FriendRequestId,
                recipientUserId,
                friendRequestProfile.userName,
                "Friend Request accepted",
                "and you are friends now!",
                (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                true);

            mainChatNotificationView.AddNewFriendRequestNotification(friendRequestNotificationModel);

            if (topNotificationPanelTransform.Get().gameObject.activeInHierarchy)
                topNotificationView.AddNewFriendRequestNotification(friendRequestNotificationModel);
        }

        private void ResetFadeOut(bool fadeOutAfterDelay = false)
        {
            mainChatNotificationView.ShowNotifications();

            if (topNotificationPanelTransform.Get().gameObject.activeInHierarchy)
                topNotificationView.ShowNotification();

            fadeOutCancellationToken.Cancel();
            fadeOutCancellationToken = new CancellationTokenSource();

            if (fadeOutAfterDelay)
                WaitThenFadeOutNotifications(fadeOutCancellationToken.Token).Forget();
        }

        private void TogglePanelBackground(bool isInFocus)
        {
            if (mainChatNotificationView.GetNotificationsCount() == 0)
                return;

            if (isInFocus)
                mainChatNotificationView.ShowPanel();
            else
                mainChatNotificationView.HidePanel();
        }

        private async UniTaskVoid WaitThenFadeOutNotifications(CancellationToken cancellationToken)
        {
            await UniTask.Delay(FADEOUT_DELAY, cancellationToken: cancellationToken);
            await UniTask.SwitchToMainThread(cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                return;

            mainChatNotificationView.HideNotifications();

            if (topNotificationPanelTransform.Get() != null &&
                topNotificationPanelTransform.Get().gameObject.activeInHierarchy)
                topNotificationView.HideNotification();
        }

        private string ExtractPeerId(ChatMessage message) =>
            message.sender != ownUserProfile.userId ? message.sender : message.recipient;

        private void ResetVisibility(bool current, bool previous) =>
            SetVisibility(current);

        private bool IsProfanityFilteringEnabled() =>
            dataStore.settings.profanityChatFilteringEnabled.Get();

        private void HandleClickedFriendRequest(string friendRequestId, string userId, bool isAcceptedFromPeer)
        {
            if (string.IsNullOrEmpty(friendRequestId)) return;

            FriendRequest request = friendsController.GetAllocatedFriendRequest(friendRequestId);
            bool isFriend = friendsController.IsFriend(userId);

            if (request != null && !isFriend && !isAcceptedFromPeer)
                dataStore.HUDs.openReceivedFriendRequestDetail.Set(friendRequestId, true);
            else if (isFriend)
                OpenChat(userId);
        }

        private void OpenChat(string chatId) =>
            dataStore.HUDs.openChat.Set(chatId, true);
    }
}
