using Cysharp.Threading.Tasks;
using DCL;
using DCL.Interface;
using SocialFeaturesAnalytics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class PrivateChatWindowController : IHUD
{
    private const int USER_PRIVATE_MESSAGES_TO_REQUEST_FOR_INITIAL_LOAD = 30;
    private const float REQUEST_MESSAGES_TIME_OUT = 2;
    internal const int USER_PRIVATE_MESSAGES_TO_REQUEST_FOR_SHOW_MORE = 10;

    public IPrivateChatComponentView View { get; private set; }
    
    private enum ChatWindowVisualState { NONE_VISIBLE, INPUT_MODE }

    private readonly DataStore dataStore;
    private readonly IUserProfileBridge userProfileBridge;
    private readonly IChatController chatController;
    private readonly IFriendsController friendsController;
    private readonly ISocialAnalytics socialAnalytics;
    private readonly IMouseCatcher mouseCatcher;
    private readonly InputAction_Trigger toggleChatTrigger;
    private ChatHUDController chatHudController;
    private UserProfile conversationProfile;
    private bool skipChatInputTrigger;
    private float lastRequestTime;
    private ChatWindowVisualState currentState;
    private CancellationTokenSource deactivateFadeOutCancellationToken = new CancellationTokenSource();
    private CancellationTokenSource markMessagesAsSeenCancellationToken = new CancellationTokenSource();
    private bool shouldRequestMessages;

    internal BaseVariable<HashSet<string>> visibleTaskbarPanels => dataStore.HUDs.visibleTaskbarPanels;
    internal string ConversationUserId { get; set; } = string.Empty;

    public event Action OnBack;
    public event Action OnClosed;

    public PrivateChatWindowController(DataStore dataStore,
        IUserProfileBridge userProfileBridge,
        IChatController chatController,
        IFriendsController friendsController,
        ISocialAnalytics socialAnalytics,
        IMouseCatcher mouseCatcher,
        InputAction_Trigger toggleChatTrigger)
    {
        this.dataStore = dataStore;
        this.userProfileBridge = userProfileBridge;
        this.chatController = chatController;
        this.friendsController = friendsController;
        this.socialAnalytics = socialAnalytics;
        this.mouseCatcher = mouseCatcher;
        this.toggleChatTrigger = toggleChatTrigger;
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

        chatHudController = new ChatHUDController(dataStore, userProfileBridge, false);
        chatHudController.Initialize(view.ChatHUD);
        chatHudController.OnInputFieldSelected += HandleInputFieldSelected;
        chatHudController.OnSendMessage += HandleSendChatMessage;
        chatHudController.OnMessageSentBlockedBySpam += HandleMessageBlockedBySpam;

        chatController.OnAddMessage -= HandleMessageReceived;
        chatController.OnAddMessage += HandleMessageReceived;

        toggleChatTrigger.OnTriggered += HandleChatInputTriggered;

        currentState = ChatWindowVisualState.INPUT_MODE;
    }

    public void Setup(string newConversationUserId)
    {
        if (string.IsNullOrEmpty(newConversationUserId) || newConversationUserId == ConversationUserId)
            return;

        var newConversationUserProfile = userProfileBridge.Get(newConversationUserId);

        ConversationUserId = newConversationUserId;
        conversationProfile = newConversationUserProfile;
        chatHudController.ClearAllEntries();
        shouldRequestMessages = true;
    }

    public void SetVisibility(bool visible)
    {
        if (View.IsActive == visible)
            return;

        SetVisiblePanelList(visible);
        if (visible)
        {
            View?.SetLoadingMessagesActive(false);
            View?.SetOldMessagesLoadingActive(false);

            if (conversationProfile != null)
            {
                var userStatus = friendsController.GetUserStatus(ConversationUserId);
                View.Setup(conversationProfile,
                    userStatus.presence == PresenceStatus.ONLINE,
                    userProfileBridge.GetOwn().IsBlocked(ConversationUserId));

                if (shouldRequestMessages)
                {
                    RequestPrivateMessages(
                        ConversationUserId,
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
            chatHudController.OnSendMessage -= HandleSendChatMessage;
            chatHudController.OnMessageSentBlockedBySpam -= HandleMessageBlockedBySpam;
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

    private void HandleMessageReceived(ChatMessage message)
    {
        if (!IsMessageFomCurrentConversation(message))
            return;

        chatHudController.AddChatMessage(message, limitMaxEntries: false);

        if (View.IsActive)
        {
            markMessagesAsSeenCancellationToken.Cancel();
            markMessagesAsSeenCancellationToken = new CancellationTokenSource();
            // since there could be many messages coming in a row, avoid making the call instantly for each message
            // instead make just one call after the iteration finishes
            MarkMessagesAsSeenDelayed(markMessagesAsSeenCancellationToken.Token).Forget();
        }

        View?.SetLoadingMessagesActive(false);
        View?.SetOldMessagesLoadingActive(false);

        deactivateFadeOutCancellationToken.Cancel();
        deactivateFadeOutCancellationToken = new CancellationTokenSource();
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
        friendsController.RemoveFriend(friendId);
        Hide();
    }

    private bool IsMessageFomCurrentConversation(ChatMessage message)
    {
        return message.messageType == ChatMessage.Type.PRIVATE &&
               (message.sender == ConversationUserId || message.recipient == ConversationUserId);
    }

    private void MarkUserChatMessagesAsRead() =>
        chatController.MarkMessagesAsSeen(ConversationUserId);
    
    private async UniTask MarkMessagesAsSeenDelayed(CancellationToken cancellationToken)
    {
        await UniTask.NextFrame(cancellationToken);
        if (cancellationToken.IsCancellationRequested) return;
        MarkUserChatMessagesAsRead();
    }

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

    private async UniTaskVoid WaitThenFadeOutMessages(CancellationToken cancellationToken)
    {
        await UniTask.SwitchToMainThread(cancellationToken);
        if (cancellationToken.IsCancellationRequested)
            return;
        chatHudController.FadeOutMessages();
        currentState = ChatWindowVisualState.NONE_VISIBLE;
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

        var currentPrivateMessages = chatController.GetPrivateAllocatedEntriesByUser(ConversationUserId);

        var oldestMessageId = currentPrivateMessages
            .OrderBy(x => x.timestamp)
            .Select(x => x.messageId)
            .FirstOrDefault();

        chatController.GetPrivateMessages(
            ConversationUserId,
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
}