using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using DCL.Interface;
using DCL.ProfanityFiltering;
using DCL.Social.Chat;
using DCL.Social.Chat.Mentions;
using SocialFeaturesAnalytics;
using UnityEngine;
using Channel = DCL.Chat.Channels.Channel;

namespace DCL.Chat.HUD
{
    public class PublicChatWindowController : IHUD
    {
        public IPublicChatWindowView View { get; private set; }

        public event Action OnBack;
        public event Action OnClosed;

        private readonly IChatController chatController;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly DataStore dataStore;
        private readonly IProfanityFilter profanityFilter;
        private readonly IMouseCatcher mouseCatcher;
        private readonly IChatMentionSuggestionProvider chatMentionSuggestionProvider;
        private readonly ISocialAnalytics socialAnalytics;
        private ChatHUDController chatHudController;
        private string channelId;
        private bool skipChatInputTrigger;
        private NearbyMembersHUDController nearbyMembersHUDController;

        private bool showOnlyOnlineMembersOnPublicChannels => !dataStore.featureFlags.flags.Get().IsFeatureEnabled("matrix_presence_disabled");
        private BaseDictionary<string, Player> nearbyPlayers => dataStore.player.otherPlayers;

        private bool isVisible;

        private BaseVariable<HashSet<string>> visibleTaskbarPanels => dataStore.HUDs.visibleTaskbarPanels;

        public PublicChatWindowController(IChatController chatController,
            IUserProfileBridge userProfileBridge,
            DataStore dataStore,
            IProfanityFilter profanityFilter,
            IMouseCatcher mouseCatcher,
            IChatMentionSuggestionProvider chatMentionSuggestionProvider,
            ISocialAnalytics socialAnalytics)
        {
            this.chatController = chatController;
            this.userProfileBridge = userProfileBridge;
            this.dataStore = dataStore;
            this.profanityFilter = profanityFilter;
            this.mouseCatcher = mouseCatcher;
            this.chatMentionSuggestionProvider = chatMentionSuggestionProvider;
            this.socialAnalytics = socialAnalytics;
        }

        public void Initialize(IPublicChatWindowView view, bool isVisible = true)
        {
            View = view;
            view.OnClose += HandleViewClosed;
            view.OnBack += HandleViewBacked;
            view.OnMuteChanged += MuteChannel;
            view.OnGoToCrowd += GoToCrowd;

            if (mouseCatcher != null)
                mouseCatcher.OnMouseLock += Hide;

            chatHudController = new ChatHUDController(dataStore,
                userProfileBridge,
                true,
                (name, count, ct) => chatMentionSuggestionProvider.GetNearbyProfilesStartingWith(name, count, ct),
                socialAnalytics,
                chatController,
                profanityFilter);
            // dont set any message's sorting strategy, just add them sequentally
            // comms cannot calculate a server timestamp for each message
            chatHudController.Initialize(view.ChatHUD);
            chatHudController.OnSendMessage += SendChatMessage;
            chatHudController.OnMessageSentBlockedBySpam += HandleMessageBlockedBySpam;

            chatController.OnAddMessage -= HandleMessageReceived;
            chatController.OnAddMessage += HandleMessageReceived;
            chatController.OnChannelUpdated -= HandleChannelUpdated;
            chatController.OnChannelUpdated += HandleChannelUpdated;

            dataStore.mentions.someoneMentionedFromContextMenu.OnChange += SomeoneMentionedFromContextMenu;
            nearbyPlayers.OnAdded += UpdateMembersCount;
            nearbyPlayers.OnRemoved += UpdateMembersCount;

            view.OnShowMembersList += ShowMembersList;
            view.OnHideMembersList += HideMembersList;
            nearbyMembersHUDController = new NearbyMembersHUDController(view.ChannelMembersHUD, dataStore.player, userProfileBridge);

            SetVisibility(isVisible);
            this.isVisible = isVisible;
        }

        public void Setup(string channelId)
        {
            if (string.IsNullOrEmpty(channelId) || channelId == this.channelId) return;
            this.channelId = channelId;

            var channel = chatController.GetAllocatedChannel(channelId);
            View.Configure(ToPublicChatModel(channel));

            chatHudController.ClearAllEntries();
        }

        public void Dispose()
        {
            View.OnClose -= HandleViewClosed;
            View.OnBack -= HandleViewBacked;
            View.OnMuteChanged -= MuteChannel;
            View.OnGoToCrowd -= GoToCrowd;

            if (chatController != null)
            {
                chatController.OnAddMessage -= HandleMessageReceived;
                chatController.OnChannelUpdated -= HandleChannelUpdated;
            }

            chatHudController.OnSendMessage -= SendChatMessage;
            chatHudController.OnMessageSentBlockedBySpam -= HandleMessageBlockedBySpam;
            chatHudController.Dispose();

            if (mouseCatcher != null)
                mouseCatcher.OnMouseLock -= Hide;

            dataStore.mentions.someoneMentionedFromContextMenu.OnChange -= SomeoneMentionedFromContextMenu;
            nearbyPlayers.OnAdded -= UpdateMembersCount;
            nearbyPlayers.OnRemoved -= UpdateMembersCount;

            View?.Dispose();
            nearbyMembersHUDController.Dispose();
        }

        public void SetVisibility(bool visible, bool focusInputField)
        {
            if (isVisible == visible)
                return;

            isVisible = visible;

            SetVisiblePanelList(visible);
            chatHudController.SetVisibility(visible);
            dataStore.HUDs.chatInputVisible.Set(visible);

            if (visible)
            {
                View.ChatHUD.ResetInputField();
                View.Show();
                MarkChannelMessagesAsRead();

                if (focusInputField)
                    chatHudController.FocusInputField();

                nearbyMembersHUDController.ClearSearch();
            }
            else
            {
                chatHudController.UnfocusInputField();
                View.Hide();
            }
        }

        public void SetVisibility(bool visible) =>
            SetVisibility(visible, false);

        private void SendChatMessage(ChatMessage message)
        {
            bool isValidMessage = !string.IsNullOrEmpty(message.body) && !string.IsNullOrWhiteSpace(message.body);
            bool isPrivateMessage = message.messageType == ChatMessage.Type.PRIVATE;

            if (isValidMessage)
            {
                chatHudController.ResetInputField();
                chatHudController.FocusInputField();
            }
            else
            {
                HandleViewClosed();
                SetVisibility(false);
                return;
            }

            if (isPrivateMessage)
                message.body = $"/w {message.recipient} {message.body}";

            chatController.Send(message);
        }

        private void SetVisiblePanelList(bool visible)
        {
            HashSet<string> newSet = visibleTaskbarPanels.Get();

            if (visible)
                newSet.Add("PublicChatChannel");
            else
                newSet.Remove("PublicChatChannel");

            visibleTaskbarPanels.Set(newSet, true);
        }

        private void MarkChannelMessagesAsRead() =>
            chatController.MarkChannelMessagesAsSeen(channelId);

        private void HandleViewClosed()
        {
            OnClosed?.Invoke();
        }

        private void HandleViewBacked()
        {
            OnBack?.Invoke();
        }

        private void HandleMessageReceived(ChatMessage[] messages)
        {
            var messageLogUpdated = false;

            var ownPlayerAlreadyMentioned = false;

            foreach (var message in messages)
            {
                if (!ownPlayerAlreadyMentioned)
                    ownPlayerAlreadyMentioned = CheckOwnPlayerMentionInNearBy(message);

                if (message.messageType != ChatMessage.Type.PUBLIC
                    && message.messageType != ChatMessage.Type.SYSTEM) continue;

                if (!string.IsNullOrEmpty(message.recipient)) continue;

                chatHudController.SetChatMessage(message, View.IsActive);
                messageLogUpdated = true;
            }

            if (View.IsActive && messageLogUpdated)
                MarkChannelMessagesAsRead();
        }

        private bool CheckOwnPlayerMentionInNearBy(ChatMessage message)
        {
            var ownUserProfile = userProfileBridge.GetOwn();

            if (message.sender == ownUserProfile.userId ||
                message.messageType != ChatMessage.Type.PUBLIC ||
                !string.IsNullOrEmpty(message.recipient) ||
                View.IsActive ||
                !MentionsUtils.IsUserMentionedInText(ownUserProfile.userName, message.body))
                return false;

            dataStore.mentions.ownPlayerMentionedInChannel.Set(ChatUtils.NEARBY_CHANNEL_ID, true);
            return true;
        }

        private void Hide() =>
            SetVisibility(false);

        private void HandleMessageBlockedBySpam(ChatMessage message)
        {
            chatHudController.SetChatMessage(new ChatEntryModel
            {
                timestamp = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                bodyText = "You sent too many messages in a short period of time. Please wait and try again later.",
                messageId = Guid.NewGuid().ToString(),
                messageType = ChatMessage.Type.SYSTEM,
                subType = ChatEntryModel.SubType.RECEIVED
            }).Forget();
        }

        private void MuteChannel(bool muted)
        {
            if (muted)
                chatController.MuteChannel(channelId);
            else
                chatController.UnmuteChannel(channelId);
        }

        private PublicChatModel ToPublicChatModel(Channel channel) =>
            new (channel.ChannelId,
                channel.Name,
                channel.Description,
                channel.Joined,
                nearbyPlayers.Count(),
                channel.Muted,
                showOnlyOnlineMembersOnPublicChannels);

        private void HandleChannelUpdated(Channel updatedChannel)
        {
            if (updatedChannel.ChannelId != channelId) return;
            View.Configure(ToPublicChatModel(updatedChannel));
        }

        private void SomeoneMentionedFromContextMenu(string mention, string _)
        {
            if (!View.IsActive)
                return;

            View.ChatHUD.AddTextIntoInputField(mention);
        }

        private void ShowMembersList() =>
            nearbyMembersHUDController.SetVisibility(true);

        private void HideMembersList() =>
            nearbyMembersHUDController.SetVisibility(false);

        private void GoToCrowd()
        {
            // Requested temporally by product team since the "go to crowd" approach was always redirecting to the casino.
            //Environment.i.world.teleportController.GoToCrowd();
            Environment.i.world.teleportController.Teleport(0, 0);
        }

        private void UpdateMembersCount(string userId, Player player) =>
            View.UpdateMembersCount(nearbyPlayers.Count());
    }
}
