using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Interface;
using SocialFeaturesAnalytics;

public class PrivateChatWindowController : IHUD
{
    public IPrivateChatComponentView View { get; private set; }
    
    private enum ChatWindowVisualState { NONE_VISIBLE, INPUT_MODE, PREVIEW_MODE }

    private readonly DataStore dataStore;
    private readonly IUserProfileBridge userProfileBridge;
    private readonly IChatController chatController;
    private readonly IFriendsController friendsController;
    private readonly InputAction_Trigger closeWindowTrigger;
    private readonly ILastReadMessagesService lastReadMessagesService;
    private readonly ISocialAnalytics socialAnalytics;
    private readonly IMouseCatcher mouseCatcher;
    private readonly InputAction_Trigger toggleChatTrigger;
    private ChatHUDController chatHudController;
    private UserProfile conversationProfile;
    private bool skipChatInputTrigger;
    private ChatWindowVisualState currentState;
    private CancellationTokenSource deactivatePreviewCancellationToken = new CancellationTokenSource();
    private CancellationTokenSource deactivateFadeOutCancellationToken = new CancellationTokenSource();

    private string ConversationUserId { get; set; } = string.Empty;

    public event Action OnPressBack;
    public event Action OnClosed;
    public event Action<bool> OnPreviewModeChanged;

    public PrivateChatWindowController(DataStore dataStore,
        IUserProfileBridge userProfileBridge,
        IChatController chatController,
        IFriendsController friendsController,
        InputAction_Trigger closeWindowTrigger,
        ILastReadMessagesService lastReadMessagesService,
        ISocialAnalytics socialAnalytics,
        IMouseCatcher mouseCatcher,
        InputAction_Trigger toggleChatTrigger)
    {
        this.dataStore = dataStore;
        this.userProfileBridge = userProfileBridge;
        this.chatController = chatController;
        this.friendsController = friendsController;
        this.closeWindowTrigger = closeWindowTrigger;
        this.lastReadMessagesService = lastReadMessagesService;
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

        var userStatus = friendsController.GetUserStatus(ConversationUserId);

        View.Setup(newConversationUserProfile,
            userStatus.presence == PresenceStatus.ONLINE,
            userProfileBridge.GetOwn().IsBlocked(ConversationUserId));

        ReloadAllChats().Forget();
    }

    public void SetVisibility(bool visible)
    {
        if (View.IsActive == visible)
            return;

        if (visible)
        {
            if (conversationProfile != null)
            {
                var userStatus = friendsController.GetUserStatus(ConversationUserId);
                View.Setup(conversationProfile,
                    userStatus.presence == PresenceStatus.ONLINE,
                    userProfileBridge.GetOwn().IsBlocked(ConversationUserId));
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
            View.OnClickOverWindow -= HandleViewClicked;
            View.Dispose();
        }
    }

    private async UniTaskVoid ReloadAllChats()
    {
        chatHudController.ClearAllEntries();

        const int entriesPerFrame = 10;
        var list = chatController.GetEntries();
        if (list.Count == 0)
            return;

        for (var i = list.Count - 1; i >= 0; i--)
        {
            var message = list[i];
            if (i != 0 && i % entriesPerFrame == 0)
                await UniTask.NextFrame();
            if (!IsMessageFomCurrentConversation(message))
                continue;
            chatHudController.AddChatMessage(message, spamFiltering: false);
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
            skipChatInputTrigger = true;
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

        chatHudController.AddChatMessage(message, View.IsActive);

        if (View.IsActive)
        {
            // The messages from 'conversationUserId' are marked as read if his private chat window is currently open
            MarkUserChatMessagesAsRead();
        }

        deactivatePreviewCancellationToken.Cancel();
        deactivatePreviewCancellationToken = new CancellationTokenSource();
        deactivateFadeOutCancellationToken.Cancel();
        deactivateFadeOutCancellationToken = new CancellationTokenSource();
        
        if (currentState.Equals(ChatWindowVisualState.NONE_VISIBLE))
        {
            ActivatePreview();
        }
        else if (currentState.Equals(ChatWindowVisualState.PREVIEW_MODE))
        {
            WaitThenFadeOutMessages(deactivateFadeOutCancellationToken.Token).Forget();
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
        lastReadMessagesService.MarkAllRead(ConversationUserId);

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
}