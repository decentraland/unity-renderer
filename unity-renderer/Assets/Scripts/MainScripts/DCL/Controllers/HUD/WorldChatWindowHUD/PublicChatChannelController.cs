using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Interface;
using SocialFeaturesAnalytics;
using UnityEngine;

public class PublicChatChannelController : IHUD
{
    public IChannelChatWindowView View { get; private set; }
    public event Action OnBack;
    public event Action OnClosed;
    public event Action<bool> OnPreviewModeChanged;

    private readonly IChatController chatController;
    private readonly ILastReadMessagesService lastReadMessagesService;
    private readonly IUserProfileBridge userProfileBridge;
    private readonly DataStore dataStore;
    private readonly IProfanityFilter profanityFilter;
    private readonly ISocialAnalytics socialAnalytics;
    private readonly IMouseCatcher mouseCatcher;
    private readonly InputAction_Trigger toggleChatTrigger;
    private ChatHUDController chatHudController;
    private double initTimeInSeconds;
    private string channelId;
    private CancellationTokenSource deactivatePreviewCancellationToken = new CancellationTokenSource();
    private CancellationTokenSource deactivatePreviewHUDControllerCancellationToken = new CancellationTokenSource();

    private bool skipChatInputTrigger;
    private string lastPrivateMessageRecipient = string.Empty;

    private UserProfile ownProfile => userProfileBridge.GetOwn();
    
    public enum ChatWindowVisualState { NONE_VISIBLE, INPUT_MODE, PREVIEW_MODE }
    private ChatWindowVisualState _currentState;
    public PublicChatChannelController(IChatController chatController,
        ILastReadMessagesService lastReadMessagesService,
        IUserProfileBridge userProfileBridge,
        DataStore dataStore,
        IProfanityFilter profanityFilter,
        ISocialAnalytics socialAnalytics,
        IMouseCatcher mouseCatcher,
        InputAction_Trigger toggleChatTrigger)
    {
        this.chatController = chatController;
        this.lastReadMessagesService = lastReadMessagesService;
        this.userProfileBridge = userProfileBridge;
        this.dataStore = dataStore;
        this.profanityFilter = profanityFilter;
        this.socialAnalytics = socialAnalytics;
        this.mouseCatcher = mouseCatcher;
        this.toggleChatTrigger = toggleChatTrigger;
    }

    public void Initialize(IChannelChatWindowView view = null)
    {
        view ??= PublicChatChannelComponentView.Create();
        View = view;
        view.OnClose += HandleViewClosed;
        view.OnBack += HandleViewBacked;
        view.OnFocused += HandleViewFocused;
        View.OnClick += HandleViewClicked;


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
        
        _currentState = ChatWindowVisualState.PREVIEW_MODE;
        WaitThenDeactivatePreviewChatHUDController(deactivatePreviewHUDControllerCancellationToken.Token).Forget();
    }

    public void Setup(string channelId)
    {
        if (string.IsNullOrEmpty(channelId) || channelId == this.channelId) return;
        this.channelId = channelId;

        // TODO: retrieve data from a channel provider
        View.Configure(new PublicChatChannelModel(this.channelId, "nearby",
            "Talk to the people around you. If you move far away from someone you will lose contact. All whispers will be displayed."));

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
            View.OnClick -= HandleViewClicked; 
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
        socialAnalytics.SendChannelMessageSent(message.sender, message.body.Length, channelId);
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
        var list = chatController.GetEntries();
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
        View.ActivatePreviewInstantly();
        chatHudController.ActivatePreview();
        OnPreviewModeChanged?.Invoke(true);
    }

    private void MarkChatMessagesAsRead() => lastReadMessagesService.MarkAllRead(channelId);

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
        deactivatePreviewHUDControllerCancellationToken.Cancel();
        deactivatePreviewHUDControllerCancellationToken = new CancellationTokenSource();
        
        if (_currentState.Equals(ChatWindowVisualState.INPUT_MODE))
        {
            //Input window is present. Start counter to deactivate
            if (chatHudController.IsInputSelected) return;
            WaitThenActivatePreview(deactivatePreviewCancellationToken.Token).Forget();
        }
        else
        {
            //If the chatHudController is visible, just restart the counter
            if (_currentState.Equals(ChatWindowVisualState.PREVIEW_MODE))
            {
                //Start Fade Out Counter
                WaitThenDeactivatePreviewChatHUDController(deactivatePreviewHUDControllerCancellationToken.Token).Forget();
            }
            else
            {
                //If the chat hud controller is not visible, we got to activate preview immediately
                ActivatePreview();
            }
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
        if (View.IsFocused) return;
        WaitThenActivatePreview(deactivatePreviewCancellationToken.Token).Forget();
    }

    private void HandleViewFocused(bool focused)
    {
        if (focused)
        {
            deactivatePreviewCancellationToken.Cancel();
            deactivatePreviewCancellationToken = new CancellationTokenSource();
            deactivatePreviewHUDControllerCancellationToken.Cancel();
            deactivatePreviewHUDControllerCancellationToken = new CancellationTokenSource();
            //Means something is visible. Restarting the counters is enough
            if (_currentState.Equals(ChatWindowVisualState.INPUT_MODE) || _currentState.Equals(ChatWindowVisualState.PREVIEW_MODE))
                return;
            
            //We have to activate preview only for the ChatHudController and dont restart the timer
            ActivatePreviewOnlyHUDController();
        }
        else
        {
            if (chatHudController.IsInputSelected) return;
            //Means Input Window is visible
            if (_currentState.Equals(ChatWindowVisualState.INPUT_MODE))
            {
                //Input window is present. Start counter to deactivate
                WaitThenActivatePreview(deactivatePreviewCancellationToken.Token).Forget();
                return;
            }
            //Means ChatHUDController is visible
            if (_currentState.Equals(ChatWindowVisualState.PREVIEW_MODE))
            {
                //Start Fade Out Counter
                WaitThenDeactivatePreviewChatHUDController(deactivatePreviewHUDControllerCancellationToken.Token).Forget();
            }
        }
    }
    
    private void HandleViewClicked()
    {
        if (_currentState.Equals(ChatWindowVisualState.INPUT_MODE))
            return;
        DeactivatePreview();
    }

    private async UniTaskVoid WaitThenActivatePreview(CancellationToken cancellationToken)
    {
        await UniTask.Delay(3000, cancellationToken: cancellationToken);
        await UniTask.SwitchToMainThread(cancellationToken);
        if (cancellationToken.IsCancellationRequested) return;
        _currentState = ChatWindowVisualState.PREVIEW_MODE;
        ActivatePreview();
    }
    
    private async UniTaskVoid WaitThenDeactivatePreviewChatHUDController(CancellationToken cancellationToken)
    {
        //Moved the fadeout counter to this class, since it seemed more appropiate. Left a testing value of 5 secs, this should be changed to 30 when released
        await UniTask.Delay(5000, cancellationToken: cancellationToken);
        await UniTask.SwitchToMainThread(cancellationToken);
        if (cancellationToken.IsCancellationRequested) return;
        chatHudController.DeactivatePreview(true);
        _currentState = ChatWindowVisualState.NONE_VISIBLE;
    }
    
    public void ActivatePreview()
    {
        View.ActivatePreview();
        chatHudController.ActivatePreview();
        _currentState = ChatWindowVisualState.PREVIEW_MODE;
        WaitThenDeactivatePreviewChatHUDController(deactivatePreviewHUDControllerCancellationToken.Token).Forget();
        OnPreviewModeChanged?.Invoke(true);
    }
    
    public void ActivatePreviewOnlyHUDController()
    {
        chatHudController.ActivatePreview();
        OnPreviewModeChanged?.Invoke(true);
        _currentState = ChatWindowVisualState.PREVIEW_MODE;
    }

    public void DeactivatePreview()
    {
        deactivatePreviewCancellationToken.Cancel();
        deactivatePreviewCancellationToken = new CancellationTokenSource();
        deactivatePreviewHUDControllerCancellationToken.Cancel();
        deactivatePreviewHUDControllerCancellationToken = new CancellationTokenSource();
        
        View.DeactivatePreview();
        chatHudController.DeactivatePreview(false);
        OnPreviewModeChanged?.Invoke(false);
        _currentState = ChatWindowVisualState.INPUT_MODE;
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