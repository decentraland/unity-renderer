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
    internal const int USER_PRIVATE_MESSAGES_TO_REQUEST_FOR_INITIAL_LOAD = 30;
    internal const int USER_PRIVATE_MESSAGES_TO_REQUEST_FOR_SHOW_MORE = 10;
    internal const float REQUEST_MESSAGES_TIME_OUT = 2;

    public IPrivateChatComponentView View { get; private set; }

    private enum ChatWindowVisualState
    {
        NONE_VISIBLE,
        INPUT_MODE,
        PREVIEW_MODE
    }

    private readonly DataStore dataStore;
    private readonly IUserProfileBridge userProfileBridge;
    private readonly IChatController chatController;
    private readonly IFriendsController friendsController;
    private readonly InputAction_Trigger closeWindowTrigger;
    private readonly ISocialAnalytics socialAnalytics;
    private readonly IMouseCatcher mouseCatcher;
    private readonly InputAction_Trigger toggleChatTrigger;
    private ChatHUDController chatHudController;
    private UserProfile conversationProfile;
    private float lastRequestTime;
    private ChatWindowVisualState currentState;
    private CancellationTokenSource deactivatePreviewCancellationToken = new CancellationTokenSource();
    private CancellationTokenSource deactivateFadeOutCancellationToken = new CancellationTokenSource();
    private CancellationTokenSource markMessagesAsSeenCancellationToken = new CancellationTokenSource();
    private bool shouldRequestMessages;

    internal string ConversationUserId { get; set; } = string.Empty;

    public event Action OnPressBack;
    public event Action OnClosed;
    public event Action<bool> OnPreviewModeChanged;

    public PrivateChatWindowController(DataStore dataStore,
        IUserProfileBridge userProfileBridge,
        IChatController chatController,
        IFriendsController friendsController,
        InputAction_Trigger closeWindowTrigger,
        ISocialAnalytics socialAnalytics,
        IMouseCatcher mouseCatcher,
        InputAction_Trigger toggleChatTrigger)
    {
        this.dataStore = dataStore;
        this.userProfileBridge = userProfileBridge;
        this.chatController = chatController;
        this.friendsController = friendsController;
        this.closeWindowTrigger = closeWindowTrigger;
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
        view.OnFocused += HandleViewFocused;
        view.OnRequireMoreMessages += RequestOldConversations;
        view.OnClickOverWindow += HandleViewClicked;

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
            mouseCatcher.OnMouseLock += ActivatePreview;

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
            chatHudController.OnInputFieldDeselected -= HandleInputFieldDeselected;
        }

        if (chatController != null)
            chatController.OnAddMessage -= HandleMessageReceived;

        if (mouseCatcher != null)
            mouseCatcher.OnMouseLock -= ActivatePreview;

        toggleChatTrigger.OnTriggered -= HandleChatInputTriggered;

        if (View != null)
        {
            View.OnPressBack -= HandlePressBack;
            View.OnClose -= Hide;
            View.OnMinimize -= MinimizeView;
            View.OnUnfriend -= Unfriend;
            View.OnFocused -= HandleViewFocused;
            View.OnRequireMoreMessages -= RequestOldConversations;
            View.OnClickOverWindow -= HandleViewClicked;
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
            chatHudController.ResetInputField(true);
            ActivatePreview();
            return;
        }

        // If Kernel allowed for private messages without the whisper param we could avoid this line
        message.body = $"/w {message.recipient} {message.body}";

        chatController.Send(message);
    }

    private void HandleCloseInputTriggered(DCLAction_Trigger action) => Hide();

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

        deactivatePreviewCancellationToken.Cancel();
        deactivatePreviewCancellationToken = new CancellationTokenSource();
        deactivateFadeOutCancellationToken.Cancel();
        deactivateFadeOutCancellationToken = new CancellationTokenSource();

        switch (currentState)
        {
            case ChatWindowVisualState.NONE_VISIBLE:
                ActivatePreview();
                break;
            case ChatWindowVisualState.PREVIEW_MODE:
                WaitThenFadeOutMessages(deactivateFadeOutCancellationToken.Token).Forget();
                break;
        }
    }

    private void Hide()
    {
        SetVisibility(false);
        OnClosed?.Invoke();
    }

    private void HandlePressBack() => OnPressBack?.Invoke();

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
        deactivatePreviewCancellationToken.Cancel();
        deactivatePreviewCancellationToken = new CancellationTokenSource();
        DeactivatePreview();
        // The messages from 'conversationUserId' are marked as read if the player clicks on the input field of the private chat
        MarkUserChatMessagesAsRead();
    }

    private void HandleInputFieldDeselected()
    {
        if (View.IsFocused)
            return;
        WaitThenActivatePreview(deactivatePreviewCancellationToken.Token).Forget();
    }

    private void HandleViewFocused(bool focused)
    {
        if (focused)
        {
            deactivatePreviewCancellationToken.Cancel();
            deactivatePreviewCancellationToken = new CancellationTokenSource();
            deactivateFadeOutCancellationToken.Cancel();
            deactivateFadeOutCancellationToken = new CancellationTokenSource();

            if (currentState.Equals(ChatWindowVisualState.NONE_VISIBLE))
            {
                ActivatePreviewOnMessages();
            }
        }
        else
        {
            if (chatHudController.IsInputSelected)
                return;

            if (currentState.Equals(ChatWindowVisualState.INPUT_MODE))
            {
                WaitThenActivatePreview(deactivatePreviewCancellationToken.Token).Forget();
                return;
            }

            if (currentState.Equals(ChatWindowVisualState.PREVIEW_MODE))
            {
                WaitThenFadeOutMessages(deactivateFadeOutCancellationToken.Token).Forget();
            }
        }
    }

    private void HandleViewClicked()
    {
        if (currentState.Equals(ChatWindowVisualState.INPUT_MODE))
            return;
        DeactivatePreview();
    }

    private async UniTaskVoid WaitThenActivatePreview(CancellationToken cancellationToken)
    {
        await UniTask.Delay(3000, cancellationToken: cancellationToken);
        await UniTask.SwitchToMainThread(cancellationToken);
        if (cancellationToken.IsCancellationRequested)
            return;
        currentState = ChatWindowVisualState.PREVIEW_MODE;
        ActivatePreview();
    }

    private async UniTaskVoid WaitThenFadeOutMessages(CancellationToken cancellationToken)
    {
        await UniTask.Delay(30000, cancellationToken: cancellationToken);
        await UniTask.SwitchToMainThread(cancellationToken);
        if (cancellationToken.IsCancellationRequested)
            return;
        chatHudController.FadeOutMessages();
        currentState = ChatWindowVisualState.NONE_VISIBLE;
    }

    public void ActivatePreview()
    {
        View.ActivatePreview();
        chatHudController.ActivatePreview();
        currentState = ChatWindowVisualState.PREVIEW_MODE;
        WaitThenFadeOutMessages(deactivateFadeOutCancellationToken.Token).Forget();
        OnPreviewModeChanged?.Invoke(true);
    }

    public void ActivatePreviewOnMessages()
    {
        chatHudController.ActivatePreview();
        currentState = ChatWindowVisualState.PREVIEW_MODE;
        OnPreviewModeChanged?.Invoke(true);
    }

    public void DeactivatePreview()
    {
        deactivatePreviewCancellationToken.Cancel();
        deactivatePreviewCancellationToken = new CancellationTokenSource();
        deactivateFadeOutCancellationToken.Cancel();
        deactivateFadeOutCancellationToken = new CancellationTokenSource();

        View.DeactivatePreview();
        chatHudController.DeactivatePreview();
        OnPreviewModeChanged?.Invoke(false);
        currentState = ChatWindowVisualState.INPUT_MODE;
    }

    private void HandleChatInputTriggered(DCLAction_Trigger action)
    {
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
}