using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL.Interface;
using DCL.Helpers;
using UnityEngine;
using System;
using Channel = DCL.Chat.Channels.Channel;

namespace DCL.Chat.Notifications
{
    public class ChatNotificationController : IHUD
    {
        private const int FADEOUT_DELAY = 8000;

        private readonly DataStore dataStore;
        private readonly IChatController chatController;
        private readonly IMainChatNotificationsComponentView mainChatNotificationView;
        private readonly ITopNotificationsComponentView topNotificationView;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly IProfanityFilter profanityFilter;
        private readonly TimeSpan maxNotificationInterval = new TimeSpan(0, 1, 0);
        private readonly HashSet<string> notificationEntries = new HashSet<string>();
        private BaseVariable<bool> shouldShowNotificationPanel => dataStore.HUDs.shouldShowNotificationPanel;
        private BaseVariable<Transform> notificationPanelTransform => dataStore.HUDs.notificationPanelTransform;
        private BaseVariable<Transform> topNotificationPanelTransform => dataStore.HUDs.topNotificationPanelTransform;
        private BaseVariable<HashSet<string>> visibleTaskbarPanels => dataStore.HUDs.visibleTaskbarPanels;
        private BaseVariable<string> openedChat => dataStore.HUDs.openedChat;
        private CancellationTokenSource fadeOutCT = new CancellationTokenSource();
        private UserProfile ownUserProfile;

        public ChatNotificationController(DataStore dataStore,
            IMainChatNotificationsComponentView mainChatNotificationView,
            ITopNotificationsComponentView topNotificationView,
            IChatController chatController,
            IUserProfileBridge userProfileBridge,
            IProfanityFilter profanityFilter)
        {
            this.dataStore = dataStore;
            this.chatController = chatController;
            this.userProfileBridge = userProfileBridge;
            this.profanityFilter = profanityFilter;
            this.mainChatNotificationView = mainChatNotificationView;
            this.topNotificationView = topNotificationView;
            mainChatNotificationView.OnResetFade += ResetFadeOut;
            topNotificationView.OnResetFade += ResetFadeOut;
            mainChatNotificationView.OnPanelFocus += TogglePanelBackground;
            chatController.OnAddMessage += HandleMessageAdded;
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
            visibleTaskbarPanels.OnChange -= VisiblePanelsChanged;
            mainChatNotificationView.OnResetFade -= ResetFadeOut;
            topNotificationView.OnResetFade -= ResetFadeOut;
        }

        private void VisiblePanelsChanged(HashSet<string> newList, HashSet<string> oldList)
        {
            SetVisibility(newList.Count == 0);
        }

        private void HandleMessageAdded(ChatMessage message)
        {
            if (message.messageType != ChatMessage.Type.PRIVATE &&
                message.messageType != ChatMessage.Type.PUBLIC) return;
            ownUserProfile ??= userProfileBridge.GetOwn();
            if (message.sender == ownUserProfile.userId) return;

            var span = Utils.UnixToDateTimeWithTime((ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) -
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

            AddNotification(message, channel).Forget();
        }

        private async UniTaskVoid AddNotification(ChatMessage message, Channel channel = null)
        {
            var peerId = ExtractPeerId(message);
            var peerProfile = userProfileBridge.Get(peerId);
            var peerName = peerProfile?.userName ?? peerId;
            var peerProfilePicture = peerProfile?.face256SnapshotURL;
            var body = message.body;

            switch (message.messageType)
            {
                case ChatMessage.Type.PRIVATE:
                    var privateModel = new PrivateChatMessageNotificationModel(message.messageId,
                        message.sender, body, message.timestamp, peerName, peerProfilePicture);

                    if (message.sender != openedChat.Get())
                    {
                        mainChatNotificationView.AddNewChatNotification(privateModel);
                        if (topNotificationPanelTransform.Get().gameObject.activeInHierarchy)
                            topNotificationView.AddNewChatNotification(privateModel);
                    }

                    break;
                case ChatMessage.Type.PUBLIC:
                    if (IsProfanityFilteringEnabled())
                    {
                        peerName = await profanityFilter.Filter(peerProfile?.userName ?? peerId);
                        body = await profanityFilter.Filter(message.body);
                    }
                    var publicModel = new PublicChannelMessageNotificationModel(message.messageId,
                        body, channel?.Name ?? message.recipient, channel?.ChannelId, message.timestamp,
                        peerName);

                    if (channel?.ChannelId != openedChat.Get())
                    {
                        mainChatNotificationView.AddNewChatNotification(publicModel);
                        if (topNotificationPanelTransform.Get().gameObject.activeInHierarchy)
                            topNotificationView.AddNewChatNotification(publicModel);
                    }

                    break;
            }
        }

        private void ResetFadeOut(bool fadeOutAfterDelay = false)
        {
            mainChatNotificationView.ShowNotifications();
            if (topNotificationPanelTransform.Get().gameObject.activeInHierarchy)
                topNotificationView.ShowNotification();

            fadeOutCT.Cancel();
            fadeOutCT = new CancellationTokenSource();

            if (fadeOutAfterDelay)
                WaitThenFadeOutNotifications(fadeOutCT.Token).Forget();
        }

        private void TogglePanelBackground(bool isInFocus)
        {
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

        private void ResetVisibility(bool current, bool previous) => SetVisibility(current);
        
        private bool IsProfanityFilteringEnabled() =>
            dataStore.settings.profanityChatFilteringEnabled.Get();
    }
}