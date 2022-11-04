using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL.Interface;
using SocialFeaturesAnalytics;
using UnityEngine;
using Channel = DCL.Chat.Channels.Channel;

namespace DCL.Chat.HUD
{
    public class ChatChannelHUDController : IHUD
    {
        private const int INITIAL_PAGE_SIZE = 30;
        private const int SHOW_MORE_PAGE_SIZE = 10;
        private const float REQUEST_MESSAGES_TIME_OUT = 2;

        public IChatChannelWindowView View { get; private set; }

        private readonly DataStore dataStore;
        private BaseVariable<HashSet<string>> visibleTaskbarPanels => dataStore.HUDs.visibleTaskbarPanels;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly IChatController chatController;
        private readonly IMouseCatcher mouseCatcher;
        private readonly InputAction_Trigger toggleChatTrigger;
        private readonly ISocialAnalytics socialAnalytics;
        private readonly IProfanityFilter profanityFilter;
        private ChatHUDController chatHudController;
        private ChannelMembersHUDController channelMembersHUDController;
        private CancellationTokenSource hideLoadingCancellationToken = new CancellationTokenSource();
        private bool skipChatInputTrigger;
        private float lastRequestTime;
        private string channelId;
        private Channel channel;
        private ChatMessage oldestMessage;

        public event Action OnPressBack;
        public event Action OnClosed;
        public event Action<string> OnOpenChannelLeave;

        public ChatChannelHUDController(DataStore dataStore,
            IUserProfileBridge userProfileBridge,
            IChatController chatController,
            IMouseCatcher mouseCatcher,
            InputAction_Trigger toggleChatTrigger,
            ISocialAnalytics socialAnalytics,
            IProfanityFilter profanityFilter)
        {
            this.dataStore = dataStore;
            this.userProfileBridge = userProfileBridge;
            this.chatController = chatController;
            this.mouseCatcher = mouseCatcher;
            this.toggleChatTrigger = toggleChatTrigger;
            this.socialAnalytics = socialAnalytics;
            this.profanityFilter = profanityFilter;
        }

        public void Initialize(IChatChannelWindowView view = null)
        {
            view ??= ChatChannelComponentView.Create();
            View = view;
            view.OnBack -= HandlePressBack;
            view.OnBack += HandlePressBack;
            view.OnClose -= Hide;
            view.OnClose += Hide;
            view.OnRequireMoreMessages += RequestOldConversations;
            view.OnLeaveChannel += LeaveChannel;
            view.OnShowMembersList += ShowMembersList;
            view.OnHideMembersList += HideMembersList;
            view.OnMuteChanged += MuteChannel;

            chatHudController = new ChatHUDController(dataStore, userProfileBridge, false, profanityFilter);
            chatHudController.Initialize(view.ChatHUD);
            chatHudController.OnSendMessage += HandleSendChatMessage;
            chatHudController.OnMessageSentBlockedBySpam += HandleMessageBlockedBySpam;

            if (mouseCatcher != null)
                mouseCatcher.OnMouseLock += Hide;

            toggleChatTrigger.OnTriggered += HandleChatInputTriggered;

            channelMembersHUDController = new ChannelMembersHUDController(view.ChannelMembersHUD, chatController, userProfileBridge);
        }

        public void Setup(string channelId)
        {
            channelMembersHUDController.SetChannelId(channelId);
            this.channelId = channelId;
            lastRequestTime = 0;

            channel = chatController.GetAllocatedChannel(channelId);
            View.Setup(ToPublicChatModel(channel));

            chatHudController.ClearAllEntries();
            oldestMessage = null;
        }

        public void SetVisibility(bool visible)
        {
            SetVisiblePanelList(visible);
            
            if (visible)
            {
                ClearChatControllerListeners();
                
                chatController.OnAddMessage += HandleMessageReceived;
                chatController.OnChannelLeft += HandleChannelLeft;
                chatController.OnChannelUpdated += HandleChannelUpdated;
                
                if (channelMembersHUDController.IsVisible)
                    channelMembersHUDController.SetAutomaticReloadingActive(true);

                View?.SetLoadingMessagesActive(false);
                View?.SetOldMessagesLoadingActive(false);

                if (!string.IsNullOrEmpty(channelId))
                {
                    var channel = chatController.GetAllocatedChannel(channelId);
                    View.Setup(ToPublicChatModel(channel));

                    RequestMessages(
                        channelId,
                        INITIAL_PAGE_SIZE);
                }

                View.Show();
                Focus();
            }
            else
            {
                ClearChatControllerListeners();

                channelMembersHUDController.SetAutomaticReloadingActive(false);
                chatHudController.UnfocusInputField();
                OnClosed?.Invoke();
                View.Hide();
            }

            dataStore.channels.channelToBeOpenedFromLink.Set(null, notifyEvent: false);
        }

        public void Dispose()
        {
            ClearChatControllerListeners();

            if (mouseCatcher != null)
                mouseCatcher.OnMouseLock -= Hide;

            toggleChatTrigger.OnTriggered -= HandleChatInputTriggered;
            
            chatHudController.OnSendMessage -= HandleSendChatMessage;
            chatHudController.OnMessageSentBlockedBySpam -= HandleMessageBlockedBySpam;

            if (View != null)
            {
                View.OnBack -= HandlePressBack;
                View.OnClose -= Hide;
                View.OnRequireMoreMessages -= RequestOldConversations;
                View.OnLeaveChannel -= LeaveChannel;
                View.OnMuteChanged -= MuteChannel;
                View.Dispose();
            }

            hideLoadingCancellationToken.Dispose();
            channelMembersHUDController.Dispose();
        }

        private void HandleSendChatMessage(ChatMessage message)
        {
            message.messageType = ChatMessage.Type.PUBLIC;
            message.recipient = channelId;

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
                SetVisibility(false);
                return;
            }

            if (message.body.ToLower().Equals("/leave"))
            {
                LeaveChannelFromCommand();
                return;
            }

            chatController.Send(message);
            socialAnalytics.SendMessageSentToChannel(channel.Name, message.body.Length, "channel");
        }

        private void HandleMessageReceived(ChatMessage message)
        {
            if (!IsMessageFomCurrentChannel(message)) return;

            UpdateOldestMessage(message);

            message.isChannelMessage = true;
            // TODO: right now the channel history is disabled, but we must find a workaround to support history + max message limit allocation for performance reasons
            // one approach could be to increment the max amount of messages depending on how many pages you loaded from the history
            // for example: 1 page = 30 messages, 2 pages = 60 messages, and so on..
            chatHudController.AddChatMessage(message, limitMaxEntries: true);

            if (View.IsActive)
            {
                // The messages from 'channelId' are marked as read if the channel window is currently open
                MarkChannelMessagesAsRead();
            }

            View?.SetLoadingMessagesActive(false);
            View?.SetOldMessagesLoadingActive(false);
        }

        private void UpdateOldestMessage(ChatMessage message)
        {
            if (oldestMessage == null)
                oldestMessage = message;
            else if (message.timestamp < oldestMessage.timestamp)
                oldestMessage = message;
        }

        private void Hide()
        {
            SetVisibility(false);
            OnClosed?.Invoke();
        }

        private void HandlePressBack() => OnPressBack?.Invoke();

        private bool IsMessageFomCurrentChannel(ChatMessage message) =>
            message.sender == channelId || message.recipient == channelId || (View.IsActive && message.messageType == ChatMessage.Type.SYSTEM);

        private void MarkChannelMessagesAsRead() => chatController.MarkChannelMessagesAsSeen(channelId);

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

        private void RequestMessages(string channelId, int limit, string fromMessageId = null)
        {
            View?.SetLoadingMessagesActive(true);
            chatController.GetChannelMessages(channelId, limit, fromMessageId);
            hideLoadingCancellationToken.Cancel();
            hideLoadingCancellationToken = new CancellationTokenSource();
            WaitForRequestTimeOutThenHideLoadingFeedback(hideLoadingCancellationToken.Token).Forget();
        }

        private void RequestOldConversations()
        {
            if (IsLoadingMessages()) return;

            View?.SetOldMessagesLoadingActive(true);
            lastRequestTime = Time.realtimeSinceStartup;

            chatController.GetChannelMessages(
                channelId,
                SHOW_MORE_PAGE_SIZE,
                oldestMessage?.messageId);

            hideLoadingCancellationToken.Cancel();
            hideLoadingCancellationToken = new CancellationTokenSource();
            WaitForRequestTimeOutThenHideLoadingFeedback(hideLoadingCancellationToken.Token).Forget();
        }

        private bool IsLoadingMessages() =>
            Time.realtimeSinceStartup - lastRequestTime < REQUEST_MESSAGES_TIME_OUT;

        private async UniTaskVoid WaitForRequestTimeOutThenHideLoadingFeedback(CancellationToken cancellationToken)
        {
            lastRequestTime = Time.realtimeSinceStartup;

            await UniTask.WaitUntil(() =>
                    Time.realtimeSinceStartup - lastRequestTime > REQUEST_MESSAGES_TIME_OUT,
                cancellationToken: cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;

            View?.SetLoadingMessagesActive(false);
            View?.SetOldMessagesLoadingActive(false);
        }

        private void LeaveChannel()
        {
            dataStore.channels.channelLeaveSource.Set(ChannelLeaveSource.Chat);
            OnOpenChannelLeave?.Invoke(channelId);
        }

        private void LeaveChannelFromCommand()
        {
            dataStore.channels.channelLeaveSource.Set(ChannelLeaveSource.Command);
            chatController.LeaveChannel(channelId);
        }

        private void HandleChannelLeft(string channelId)
        {
            if (channelId != this.channelId) return;
            OnPressBack?.Invoke();
        }

        private void HandleChannelUpdated(Channel updatedChannel)
        {
            if (updatedChannel.ChannelId != channelId)
                return;

            View.Setup(ToPublicChatModel(updatedChannel));
            channelMembersHUDController.SetMembersCount(updatedChannel.MemberCount);
        }

        private void ShowMembersList() => channelMembersHUDController.SetVisibility(true);

        private void HideMembersList() => channelMembersHUDController.SetVisibility(false);

        private void MuteChannel(bool muted)
        {
            if (muted)
                chatController.MuteChannel(channelId);
            else
                chatController.UnmuteChannel(channelId);
        }

        private void SetVisiblePanelList(bool visible)
        {
            var newSet = visibleTaskbarPanels.Get();

            if (visible)
                newSet.Add("ChatChannel");
            else
                newSet.Remove("ChatChannel");

            visibleTaskbarPanels.Set(newSet, true);
        }

        private PublicChatModel ToPublicChatModel(Channel channel)
        {
            return new PublicChatModel(channelId, channel.Name, channel.Description,
                channel.Joined, channel.MemberCount, channel.Muted);
        }
        
        private void ClearChatControllerListeners()
        {
            if (chatController == null) return;
            chatController.OnAddMessage -= HandleMessageReceived;
            chatController.OnChannelLeft -= HandleChannelLeft;
            chatController.OnChannelUpdated -= HandleChannelUpdated;
        }

        private void Focus()
        {
            chatHudController.FocusInputField();
            MarkChannelMessagesAsRead();
        }
        
        private void HandleMessageBlockedBySpam(ChatMessage message)
        {
            chatHudController.AddChatMessage(new ChatEntryModel
            {
                timestamp = (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                bodyText = "You sent too many messages in a short period of time. Please wait and try again later.",
                messageId = Guid.NewGuid().ToString(),
                messageType = ChatMessage.Type.SYSTEM,
                subType = ChatEntryModel.SubType.RECEIVED
            });
        }
    }
}