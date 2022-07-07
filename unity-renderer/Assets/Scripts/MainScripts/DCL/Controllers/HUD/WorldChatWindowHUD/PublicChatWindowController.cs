using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Interface;

public class PublicChatWindowController : IHUD
{
    public IPublicChatWindowView View { get; private set; }
    
    private enum ChatWindowVisualState { NONE_VISIBLE, INPUT_MODE, PREVIEW_MODE }

    public event Action OnBack;
    public event Action OnClosed;
    public event Action<bool> OnPreviewModeChanged;

    private readonly IChatController chatController;
    private readonly IUserProfileBridge userProfileBridge;
    private readonly DataStore dataStore;
    private readonly IProfanityFilter profanityFilter;
    private readonly IMouseCatcher mouseCatcher;
    private readonly InputAction_Trigger toggleChatTrigger;
    private ChatHUDController chatHudController;
    private double initTimeInSeconds;
    private string channelId;
    private ChatWindowVisualState currentState;
    private CancellationTokenSource deactivatePreviewCancellationToken = new CancellationTokenSource();
    private CancellationTokenSource deactivateFadeOutCancellationToken = new CancellationTokenSource();

    private bool skipChatInputTrigger;
    private string lastPrivateMessageRecipient = string.Empty;

    private UserProfile ownProfile => userProfileBridge.GetOwn();
    
    public PublicChatWindowController(IChatController chatController,
        IUserProfileBridge userProfileBridge,
        DataStore dataStore,
        IProfanityFilter profanityFilter,
        IMouseCatcher mouseCatcher,
        InputAction_Trigger toggleChatTrigger)
    {
        this.chatController = chatController;
        this.userProfileBridge = userProfileBridge;
        this.dataStore = dataStore;
        this.profanityFilter = profanityFilter;
        this.mouseCatcher = mouseCatcher;
        this.toggleChatTrigger = toggleChatTrigger;
    }

    public void Initialize(IPublicChatWindowView view = null)
    {
        view ??= PublicChatWindowComponentView.Create();
        View = view;
        view.OnClose += HandleViewClosed;
        view.OnBack += HandleViewBacked;
        view.OnFocused += HandleViewFocused;
        view.OnClickOverWindow += HandleViewClicked;

        chatHudController = new ChatHUDController(dataStore,
            userProfileBridge,
            true,
            profanityFilter);
        chatHudController.Initialize(view.ChatHUD);
        chatHudController.OnSendMessage += SendChatMessage;
        chatHudController.OnMessageUpdated += HandleMessageInputUpdated;
        chatHudController.OnInputFieldSelected -= HandleInputFieldSelected;
        chatHudController.OnInputFieldSelected += HandleInputFieldSelected;
        chatHudController.OnInputFieldDeselected -= HandleInputFieldDeselected;
        chatHudController.OnInputFieldDeselected += HandleInputFieldDeselected;

        chatController.OnAddMessage -= HandleMessageReceived;
        chatController.OnAddMessage += HandleMessageReceived;

        if (mouseCatcher != null)
            mouseCatcher.OnMouseLock += ActivatePreview;

        toggleChatTrigger.OnTriggered += HandleChatInputTriggered;

        initTimeInSeconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
        
        currentState = ChatWindowVisualState.PREVIEW_MODE;
        WaitThenFadeOutMessages(deactivateFadeOutCancellationToken.Token).Forget();
    }

    public void Setup(string channelId)
    {
        if (string.IsNullOrEmpty(channelId) || channelId == this.channelId) return;
        this.channelId = channelId;

        // TODO: retrieve data from a channel provider
        View.Configure(new PublicChatModel(this.channelId, "nearby",
            "Talk to the people around you. If you move far away from someone you will lose contact. All whispers will be displayed.",
            0));

        ReloadAllChats().Forget();
    }

    public void Dispose()
    {
        View.OnClose -= HandleViewClosed;
        View.OnBack -= HandleViewBacked;

        if (chatController != null)
            chatController.OnAddMessage -= HandleMessageReceived;

        chatHudController.OnSendMessage -= SendChatMessage;
        chatHudController.OnMessageUpdated -= HandleMessageInputUpdated;
        chatHudController.OnInputFieldSelected -= HandleInputFieldSelected;
        chatHudController.OnInputFieldDeselected -= HandleInputFieldDeselected;

        if (mouseCatcher != null)
            mouseCatcher.OnMouseLock -= ActivatePreview;
        
        toggleChatTrigger.OnTriggered -= HandleChatInputTriggered;

        if (View != null)
        {
            View.OnFocused -= HandleViewFocused;
            View.OnClickOverWindow -= HandleViewClicked; 
            View.Dispose();
        }
    }

    private void SendChatMessage(ChatMessage message)
    {
        var isValidMessage = !string.IsNullOrEmpty(message.body) && !string.IsNullOrWhiteSpace(message.body);
        var isPrivateMessage = message.messageType == ChatMessage.Type.PRIVATE;

        if (isPrivateMessage && isValidMessage)
            lastPrivateMessageRecipient = message.recipient;
        else
            lastPrivateMessageRecipient = null;

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

        if (isPrivateMessage)
            message.body = $"/w {message.recipient} {message.body}";

        chatController.Send(message);
    }
    
    public void SetVisibility(bool visible, bool focusInputField)
    {
        if (visible)
        {
            View.Show();
            MarkChatMessagesAsRead();
            
            if (focusInputField)
                chatHudController.FocusInputField();
        }
        else
        {
            chatHudController.UnfocusInputField();
            View.Hide();
        }
    }

    public void SetVisibility(bool visible) => SetVisibility(visible, false);

    private async UniTaskVoid ReloadAllChats()
    {
        chatHudController.ClearAllEntries();

        const int entriesPerFrame = 10;
        // TODO: filter entries by channelId
        var list = chatController.GetAllocatedEntries();
        if (list.Count == 0) return;

        for (var i = list.Count - 1; i >= 0; i--)
        {
            var message = list[i];
            if (i % entriesPerFrame == 0) await UniTask.NextFrame();
            HandleMessageReceived(message);
        }
    }

    public void ActivatePreviewModeInstantly()
    {
        deactivatePreviewCancellationToken.Cancel();
        deactivatePreviewCancellationToken = new CancellationTokenSource();
        deactivateFadeOutCancellationToken.Cancel();
        deactivateFadeOutCancellationToken = new CancellationTokenSource();
        
        View.ActivatePreviewInstantly();
        chatHudController.ActivatePreview();
        currentState = ChatWindowVisualState.PREVIEW_MODE;
        WaitThenFadeOutMessages(deactivateFadeOutCancellationToken.Token).Forget();
        OnPreviewModeChanged?.Invoke(true);
    }

    private void MarkChatMessagesAsRead() => chatController.MarkMessagesAsSeen(channelId);

    private void HandleViewClosed() => OnClosed?.Invoke();

    private void HandleViewBacked() => OnBack?.Invoke();

    private void HandleMessageInputUpdated(string message)
    {
        if (!string.IsNullOrEmpty(lastPrivateMessageRecipient) && message == "/r ")
            chatHudController.SetInputFieldText($"/w {lastPrivateMessageRecipient} ");
    }

    private bool IsOldPrivateMessage(ChatMessage message)
    {
        if (message.messageType != ChatMessage.Type.PRIVATE) return false;
        var timestampInSeconds = message.timestamp / 1000.0;
        return timestampInSeconds < initTimeInSeconds;
    }

    private void HandleMessageReceived(ChatMessage message)
    {
        if (IsOldPrivateMessage(message)) return;

        chatHudController.AddChatMessage(message, View.IsActive);

        if (View.IsActive)
            MarkChatMessagesAsRead();

        if (message.messageType == ChatMessage.Type.PRIVATE && message.recipient == ownProfile.userId)
            lastPrivateMessageRecipient = userProfileBridge.Get(message.sender).userName;
        
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

    private void HandleInputFieldSelected()
    {
        deactivatePreviewCancellationToken.Cancel();
        deactivatePreviewCancellationToken = new CancellationTokenSource();
        DeactivatePreview();
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
        if (cancellationToken.IsCancellationRequested) return;
        currentState = ChatWindowVisualState.PREVIEW_MODE;
        ActivatePreview();
    }
    
    private async UniTaskVoid WaitThenFadeOutMessages(CancellationToken cancellationToken)
    {
        await UniTask.Delay(30000, cancellationToken: cancellationToken);
        await UniTask.SwitchToMainThread(cancellationToken);
        if (cancellationToken.IsCancellationRequested) return;
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

    private void ActivatePreviewOnMessages()
    {
        chatHudController.ActivatePreview();
        OnPreviewModeChanged?.Invoke(true);
        currentState = ChatWindowVisualState.PREVIEW_MODE;
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
}