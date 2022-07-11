using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL.Interface;
using SocialFeaturesAnalytics;
using UnityEngine;

namespace DCL.Chat.HUD
{
    public class ChatChannelHUDController : IHUD
    {
        private const int INITIAL_PAGE_SIZE = 30;
        private const int SHOW_MORE_PAGE_SIZE = 10;
        private const float REQUEST_PRIVATE_MESSAGES_TIME_OUT = 5;

        public IChatChannelWindowView View { get; private set; }

        private readonly DataStore dataStore;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly IChatController chatController;
        private readonly InputAction_Trigger closeWindowTrigger;
        private readonly IMouseCatcher mouseCatcher;
        private readonly InputAction_Trigger toggleChatTrigger;
        private readonly List<string> directMessagesAlreadyRequested = new List<string>();
        private readonly Dictionary<string, long> lastTimestampsRequested = new Dictionary<string, long>();
        private ChatHUDController chatHudController;
        private CancellationTokenSource deactivatePreviewCancellationToken = new CancellationTokenSource();
        private bool skipChatInputTrigger;
        private bool isRequestingOldMessages;
        private float lastRequestTime;

        internal string ChannelId { get; set; } = string.Empty;

        public event Action OnPressBack;
        public event Action OnClosed;
        public event Action<bool> OnPreviewModeChanged;

        public ChatChannelHUDController(DataStore dataStore,
            IUserProfileBridge userProfileBridge,
            IChatController chatController,
            InputAction_Trigger closeWindowTrigger,
            IMouseCatcher mouseCatcher,
            InputAction_Trigger toggleChatTrigger)
        {
            this.dataStore = dataStore;
            this.userProfileBridge = userProfileBridge;
            this.chatController = chatController;
            this.closeWindowTrigger = closeWindowTrigger;
            this.mouseCatcher = mouseCatcher;
            this.toggleChatTrigger = toggleChatTrigger;
        }

        public void Initialize(IChatChannelWindowView view = null)
        {
            view ??= ChatChannelComponentView.Create();
            View = view;
            view.OnBack -= HandlePressBack;
            view.OnBack += HandlePressBack;
            view.OnClose -= Hide;
            view.OnClose += Hide;
            view.OnFocused += HandleViewFocused;
            view.OnRequireMoreMessages += RequestOldConversations;

            closeWindowTrigger.OnTriggered -= HandleCloseInputTriggered;
            closeWindowTrigger.OnTriggered += HandleCloseInputTriggered;

            chatHudController = new ChatHUDController(dataStore, userProfileBridge, false);
            chatHudController.Initialize(view.ChatHUD);
            chatHudController.OnInputFieldSelected -= HandleInputFieldSelected;
            chatHudController.OnInputFieldSelected += HandleInputFieldSelected;
            chatHudController.OnInputFieldDeselected -= HandleInputFieldDeselected;
            chatHudController.OnInputFieldDeselected += HandleInputFieldDeselected;
            chatHudController.OnSendMessage += HandleSendChatMessage;

            chatController.OnAddMessage -= HandleMessageReceived;
            chatController.OnAddMessage += HandleMessageReceived;

            if (mouseCatcher != null)
                mouseCatcher.OnMouseLock += ActivatePreviewMode;

            toggleChatTrigger.OnTriggered += HandleChatInputTriggered;
        }

        public void Setup(string channelId)
        {
            if (string.IsNullOrEmpty(channelId) || channelId == ChannelId) return;

            ChannelId = channelId;

            var channel = chatController.GetAllocatedChannel(channelId);
            View.Setup(new PublicChatModel(channelId, channel.Name, channel.Description, channel.LastMessageTimestamp, channel.Joined, channel.MemberCount));

            ReloadAllChats().Forget();
        }

        public void SetVisibility(bool visible)
        {
            if (View.IsActive == visible) return;

            if (visible)
            {
                View?.SetLoadingMessagesActive(false);
                View?.SetOldMessagesLoadingActive(false);

                if (!string.IsNullOrEmpty(ChannelId))
                {
                    var channel = chatController.GetAllocatedChannel(ChannelId);
                    View.Setup(new PublicChatModel(ChannelId, channel.Name, channel.Description, channel.LastMessageTimestamp, channel.Joined, channel.MemberCount));

                    if (!directMessagesAlreadyRequested.Contains(ChannelId))
                    {
                        RequestMessages(
                            ChannelId,
                            INITIAL_PAGE_SIZE,
                            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                    }
                }

                View.Show();
                Focus();
            }
            else
            {
                chatHudController.UnfocusInputField();
                View.Hide();
            }
        }

        public void Focus()
        {
            chatHudController.FocusInputField();
            MarkUserChatMessagesAsRead();
        }

        public void Dispose()
        {
            if (chatHudController != null)
            {
                chatHudController.OnInputFieldSelected -= HandleInputFieldSelected;
                chatHudController.OnInputFieldDeselected -= HandleInputFieldDeselected;
            }

            if (chatController != null)
                chatController.OnAddMessage -= HandleMessageReceived;

            if (mouseCatcher != null)
                mouseCatcher.OnMouseLock -= ActivatePreviewMode;

            toggleChatTrigger.OnTriggered -= HandleChatInputTriggered;

            if (View != null)
            {
                View.OnBack -= HandlePressBack;
                View.OnClose -= Hide;
                View.OnFocused -= HandleViewFocused;
                View.OnRequireMoreMessages -= RequestOldConversations;
                View.Dispose();
            }
        }

        private async UniTaskVoid ReloadAllChats()
        {
            chatHudController.ClearAllEntries();

            const int entriesPerFrame = 10;
            var list = chatController.GetAllocatedEntries();
            if (list.Count == 0) return;

            for (var i = list.Count - 1; i >= 0; i--)
            {
                var message = list[i];
                if (i != 0 && i % entriesPerFrame == 0) await UniTask.NextFrame();
                if (!IsMessageFomCurrentChannel(message)) continue;
                chatHudController.AddChatMessage(message, spamFiltering: false, limitMaxEntries: false);
            }
        }

        private void HandleSendChatMessage(ChatMessage message)
        {
            message.messageType = ChatMessage.Type.PRIVATE;
            message.recipient = ChannelId;

            var isValidMessage = !string.IsNullOrEmpty(message.body)
                                 && !string.IsNullOrWhiteSpace(message.body)
                                 && !string.IsNullOrEmpty(message.recipient);

            if (isValidMessage)
            {
                chatHudController.ResetInputField();
                chatHudController.FocusInputField();
            }

            else
            {
                skipChatInputTrigger = true;
                chatHudController.ResetInputField(true);
                ActivatePreviewMode();
                return;
            }

            chatController.Send(message);
        }

        private void HandleCloseInputTriggered(DCLAction_Trigger action) => Hide();

        private void HandleMessageReceived(ChatMessage message)
        {
            if (!IsMessageFomCurrentChannel(message)) return;

            chatHudController.AddChatMessage(message, limitMaxEntries: false);

            if (View.IsActive)
            {
                // The messages from 'conversationUserId' are marked as read if his private chat window is currently open
                MarkUserChatMessagesAsRead();
            }

            isRequestingOldMessages = false;
            View?.SetLoadingMessagesActive(false);
            View?.SetOldMessagesLoadingActive(false);
        }

        private void Hide()
        {
            SetVisibility(false);
            OnClosed?.Invoke();
        }

        private void HandlePressBack() => OnPressBack?.Invoke();

        private bool IsMessageFomCurrentChannel(ChatMessage message) => message.sender == ChannelId || message.recipient == ChannelId;

        private void MarkUserChatMessagesAsRead() => chatController.MarkMessagesAsSeen(ChannelId);

        private void HandleInputFieldSelected()
        {
            deactivatePreviewCancellationToken.Cancel();
            deactivatePreviewCancellationToken = new CancellationTokenSource();
            DeactivatePreviewMode();
            // The messages from 'conversationUserId' are marked as read if the player clicks on the input field of the private chat
            MarkUserChatMessagesAsRead();
        }

        private void HandleInputFieldDeselected()
        {
            if (View.IsFocused) return;
            WaitThenActivatePreview(deactivatePreviewCancellationToken.Token).Forget();
        }

        private void HandleViewFocused(bool focused)
        {
            if (focused)
            {
                deactivatePreviewCancellationToken.Cancel();
                deactivatePreviewCancellationToken = new CancellationTokenSource();
                DeactivatePreviewMode();
            }
            else
            {
                if (chatHudController.IsInputSelected) return;
                WaitThenActivatePreview(deactivatePreviewCancellationToken.Token).Forget();
            }
        }

        private async UniTaskVoid WaitThenActivatePreview(CancellationToken cancellationToken)
        {
            await UniTask.Delay(3000, cancellationToken: cancellationToken);
            await UniTask.SwitchToMainThread(cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            ActivatePreviewMode();
        }

        public void DeactivatePreviewMode()
        {
            View.DeactivatePreview();
            chatHudController.DeactivatePreview();
            OnPreviewModeChanged?.Invoke(false);
        }

        public void ActivatePreviewMode()
        {
            View?.ActivatePreview();
            chatHudController?.ActivatePreview();
            OnPreviewModeChanged?.Invoke(true);
        }

        private void HandleChatInputTriggered(DCLAction_Trigger action)
        {
            // race condition patch caused by unfocusing input field from invalid message on SendChatMessage
            // chat input trigger is the same key as sending the chat message from the input field
            if (skipChatInputTrigger)
            {
                skipChatInputTrigger = false;
                return;
            }

            if (!View.IsActive) return;
            chatHudController.FocusInputField();
        }

        private void RequestMessages(string channelId, int limit, long fromTimestamp)
        {
            View?.SetLoadingMessagesActive(true);
            chatController.GetChannelMessages(channelId, limit, fromTimestamp);
            directMessagesAlreadyRequested.Add(channelId);
            WaitForRequestTimeOutThenHideLoadingFeedback().Forget();
        }

        private void RequestOldConversations()
        {
            if (isRequestingOldMessages) return;

            isRequestingOldMessages = true;
            View?.SetOldMessagesLoadingActive(true);

            if (!lastTimestampsRequested.ContainsKey(ChannelId))
                lastTimestampsRequested.Add(ChannelId, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

            var allocatedMessages = chatController.GetAllocatedEntriesByChannel(ChannelId);

            if (allocatedMessages.Count <= 0) return;
            var minTimestamp = (long) allocatedMessages.Min(x => x.timestamp);

            if (minTimestamp >= lastTimestampsRequested[ChannelId]) return;
            
            chatController.GetChannelMessages(
                ChannelId,
                SHOW_MORE_PAGE_SIZE,
                minTimestamp);

            lastTimestampsRequested[ChannelId] = minTimestamp;
            WaitForRequestTimeOutThenHideLoadingFeedback().Forget();
        }

        private async UniTaskVoid WaitForRequestTimeOutThenHideLoadingFeedback()
        {
            lastRequestTime = Time.realtimeSinceStartup;

            await UniTask.WaitUntil(() =>
                Time.realtimeSinceStartup - lastRequestTime > REQUEST_PRIVATE_MESSAGES_TIME_OUT);

            View?.SetLoadingMessagesActive(false);
            View?.SetOldMessagesLoadingActive(false);
        }
    }
}