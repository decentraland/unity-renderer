using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Chat.HUD.Mentions;
using DCL.Interface;
using DCL.Social.Chat.Mentions;
using DCL.Social.Friends;
using SocialFeaturesAnalytics;
using UnityEngine;

public class PrivateChatWindowController : IHUD
{
    private const int USER_PRIVATE_MESSAGES_TO_REQUEST_FOR_INITIAL_LOAD = 30;
    private const float REQUEST_MESSAGES_TIME_OUT = 2;
    internal const int USER_PRIVATE_MESSAGES_TO_REQUEST_FOR_SHOW_MORE = 10;

    public IPrivateChatComponentView View { get; private set; }

    private readonly DataStore dataStore;
    private readonly IUserProfileBridge userProfileBridge;
    private readonly IChatController chatController;
    private readonly IFriendsController friendsController;
    private readonly ISocialAnalytics socialAnalytics;
    private readonly IMouseCatcher mouseCatcher;
    private readonly InputAction_Trigger toggleChatTrigger;
    private readonly IChatMentionSuggestionProvider chatMentionSuggestionProvider;
    private ChatHUDController chatHudController;
    private UserProfile conversationProfile;
    private bool skipChatInputTrigger;
    private float lastRequestTime;
    private CancellationTokenSource deactivateFadeOutCancellationToken = new CancellationTokenSource();
    private bool shouldRequestMessages;
    private ulong oldestTimestamp = ulong.MaxValue;
    private string oldestMessageId;
    private string conversationUserId;
    private BaseVariable<HashSet<string>> visibleTaskbarPanels => dataStore.HUDs.visibleTaskbarPanels;

    public event Action OnBack;
    public event Action OnClosed;

    public PrivateChatWindowController(DataStore dataStore,
        IUserProfileBridge userProfileBridge,
        IChatController chatController,
        IFriendsController friendsController,
        ISocialAnalytics socialAnalytics,
        IMouseCatcher mouseCatcher,
        InputAction_Trigger toggleChatTrigger,
        IChatMentionSuggestionProvider chatMentionSuggestionProvider)
    {
        this.dataStore = dataStore;
        this.userProfileBridge = userProfileBridge;
        this.chatController = chatController;
        this.friendsController = friendsController;
        this.socialAnalytics = socialAnalytics;
        this.mouseCatcher = mouseCatcher;
        this.toggleChatTrigger = toggleChatTrigger;
        this.chatMentionSuggestionProvider = chatMentionSuggestionProvider;
    }

    public void Initialize(IPrivateChatComponentView view = null)
    {
        view ??= PrivateChatWindowComponentView.Create();
        View = view;
        View.Initialize(friendsController, socialAnalytics);
        view.OnPressBack -= HandlePressBack;
        view.OnPressBack += HandlePressBack;
        view.OnClose -= Hide;
        view.OnClose += Hide;
        view.OnMinimize += MinimizeView;
        view.OnUnfriend += Unfriend;

        if (mouseCatcher != null)
            mouseCatcher.OnMouseLock += Hide;

        view.OnRequireMoreMessages += RequestOldConversations;

        chatHudController = new ChatHUDController(dataStore, userProfileBridge, false, chatMentionSuggestionProvider);
        chatHudController.Initialize(view.ChatHUD);
        chatHudController.OnInputFieldSelected += HandleInputFieldSelected;
        chatHudController.OnSendMessage += HandleSendChatMessage;
        chatHudController.OnMessageSentBlockedBySpam += HandleMessageBlockedBySpam;

        chatController.OnAddMessage -= HandleMessageReceived;
        chatController.OnAddMessage += HandleMessageReceived;

        toggleChatTrigger.OnTriggered += HandleChatInputTriggered;
    }

    public void Setup(string newConversationUserId)
    {
        if (string.IsNullOrEmpty(newConversationUserId) || newConversationUserId == conversationUserId)
            return;

        var newConversationUserProfile = userProfileBridge.Get(newConversationUserId);

        conversationUserId = newConversationUserId;
        conversationProfile = newConversationUserProfile;
        chatHudController.ClearAllEntries();
        shouldRequestMessages = true;
    }

    public void SetVisibility(bool visible)
    {
        if (View.IsActive == visible)
            return;

        SetVisiblePanelList(visible);
        chatHudController.SetVisibility(visible);

        if (visible)
        {
            View?.SetLoadingMessagesActive(false);
            View?.SetOldMessagesLoadingActive(false);

            if (conversationProfile != null)
            {
                var userStatus = friendsController.GetUserStatus(conversationUserId);
                View.Setup(conversationProfile,
                    userStatus.presence == PresenceStatus.ONLINE,
                    userProfileBridge.GetOwn().IsBlocked(conversationUserId));

                if (shouldRequestMessages)
                {
                    ResetPagination();
                    RequestPrivateMessages(
                        conversationUserId,
                        USER_PRIVATE_MESSAGES_TO_REQUEST_FOR_INITIAL_LOAD,
                        null);

                    shouldRequestMessages = false;
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

    public void Dispose()
    {
        if (chatHudController != null)
        {
            chatHudController.OnInputFieldSelected -= HandleInputFieldSelected;
            chatHudController.OnSendMessage -= HandleSendChatMessage;
            chatHudController.OnMessageSentBlockedBySpam -= HandleMessageBlockedBySpam;
            chatHudController.Dispose();
        }

        if (chatController != null)
            chatController.OnAddMessage -= HandleMessageReceived;

        if (mouseCatcher != null)
            mouseCatcher.OnMouseLock -= Hide;

        toggleChatTrigger.OnTriggered -= HandleChatInputTriggered;

        if (View != null)
        {
            View.OnPressBack -= HandlePressBack;
            View.OnClose -= Hide;
            View.OnMinimize -= MinimizeView;
            View.OnUnfriend -= Unfriend;
            View.OnFocused -= HandleViewFocused;
            View.OnRequireMoreMessages -= RequestOldConversations;
            View.Dispose();
        }
    }

    private void HandleSendChatMessage(ChatMessage message)
    {
        if (string.IsNullOrEmpty(conversationProfile.userName))
            return;

        message.messageType = ChatMessage.Type.PRIVATE;
        message.recipient = conversationProfile.userName;

        bool isValidMessage = !string.IsNullOrEmpty(message.body)
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
            OnClosed?.Invoke();
            return;
        }

        // If Kernel allowed for private messages without the whisper param we could avoid this line
        message.body = $"/w {message.recipient} {message.body}";

        chatController.Send(message);
    }

    private void MinimizeView() => SetVisibility(false);

    private void HandleMessageReceived(ChatMessage[] messages)
    {
        var messageLogUpdated = false;

        var ownPlayerAlreadyMentioned = false;
        foreach (var message in messages)
        {
            if (!ownPlayerAlreadyMentioned)
                ownPlayerAlreadyMentioned = CheckOwnPlayerMentionInDMs(message);

            if (!IsMessageFomCurrentConversation(message)) continue;

            chatHudController.AddChatMessage(message, limitMaxEntries: false);

            if (message.timestamp < oldestTimestamp)
            {
                oldestTimestamp = message.timestamp;
                oldestMessageId = message.messageId;
            }

            View?.SetLoadingMessagesActive(false);
            View?.SetOldMessagesLoadingActive(false);

            messageLogUpdated = true;
        }

        deactivateFadeOutCancellationToken.Cancel();
        deactivateFadeOutCancellationToken = new CancellationTokenSource();

        if (View.IsActive && messageLogUpdated)
            MarkUserChatMessagesAsRead();
    }

    private bool CheckOwnPlayerMentionInDMs(ChatMessage message)
    {
        string ownUserId = userProfileBridge.GetOwn().userId;

        if (message.sender == ownUserId ||
            (message.sender == conversationUserId && View.IsActive) ||
            message.messageType != ChatMessage.Type.PRIVATE ||
            !MentionsUtils.IsUserMentionedInText(ownUserId, message.body))
            return false;

        dataStore.mentions.ownPlayerMentionedInDM.Set(message.sender, true);
        return true;
    }

    private void Hide()
    {
        SetVisibility(false);
        OnClosed?.Invoke();
    }

    private void Show()
    {
        SetVisibility(true);
    }

    private void HandlePressBack() => OnBack?.Invoke();

    private void Unfriend(string friendId)
    {
        dataStore.notifications.GenericConfirmation.Set(GenericConfirmationNotificationData.CreateUnFriendData(
            UserProfileController.userProfilesCatalog.Get(friendId)?.userName,
            () =>
            {
                friendsController.RemoveFriend(friendId);
                Hide();
            }), true);
    }

    private bool IsMessageFomCurrentConversation(ChatMessage message)
    {
        return message.messageType == ChatMessage.Type.PRIVATE &&
               (message.sender == conversationUserId || message.recipient == conversationUserId);
    }

    private void MarkUserChatMessagesAsRead() =>
        chatController.MarkMessagesAsSeen(conversationUserId);

    private void HandleInputFieldSelected()
    {
        Show();
        // The messages from 'conversationUserId' are marked as read if the player clicks on the input field of the private chat
        //MarkUserChatMessagesAsRead();
    }

    private void HandleViewFocused(bool focused)
    {
        if (focused)
        {
            deactivateFadeOutCancellationToken.Cancel();
            deactivateFadeOutCancellationToken = new CancellationTokenSource();
        }
    }

    private void SetVisiblePanelList(bool visible)
    {
        HashSet<string> newSet = visibleTaskbarPanels.Get();
        if (visible)
            newSet.Add("PrivateChatChannel");
        else
            newSet.Remove("PrivateChatChannel");

        visibleTaskbarPanels.Set(newSet, true);
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

        if (!View.IsActive)
            return;
        chatHudController.FocusInputField();
    }

    internal void RequestPrivateMessages(string userId, int limit, string fromMessageId)
    {
        View?.SetLoadingMessagesActive(true);
        chatController.GetPrivateMessages(userId, limit, fromMessageId);
        WaitForRequestTimeOutThenHideLoadingFeedback().Forget();
    }

    internal void RequestOldConversations()
    {
        if (IsLoadingMessages()) return;

        chatController.GetPrivateMessages(
            conversationUserId,
            USER_PRIVATE_MESSAGES_TO_REQUEST_FOR_SHOW_MORE,
            oldestMessageId);

        lastRequestTime = Time.realtimeSinceStartup;
        View?.SetOldMessagesLoadingActive(true);
        WaitForRequestTimeOutThenHideLoadingFeedback().Forget();
    }

    private bool IsLoadingMessages() =>
        Time.realtimeSinceStartup - lastRequestTime < REQUEST_MESSAGES_TIME_OUT;

    private async UniTaskVoid WaitForRequestTimeOutThenHideLoadingFeedback()
    {
        lastRequestTime = Time.realtimeSinceStartup;

        await UniTask.WaitUntil(() => Time.realtimeSinceStartup - lastRequestTime > REQUEST_MESSAGES_TIME_OUT);

        View?.SetLoadingMessagesActive(false);
        View?.SetOldMessagesLoadingActive(false);
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

    private void ResetPagination()
    {
        oldestTimestamp = long.MaxValue;
        oldestMessageId = null;
    }

    private void Focus()
    {
        chatHudController.FocusInputField();
        MarkUserChatMessagesAsRead();
    }
}
