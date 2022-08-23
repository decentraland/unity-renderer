using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Interface;
using SocialFeaturesAnalytics;
using UnityEngine;
using System.Collections.Generic;

public class PublicChatWindowController : IHUD
{
    public IPublicChatWindowView View { get; private set; }
    
    private enum ChatWindowVisualState { NONE_VISIBLE, INPUT_MODE, PREVIEW_MODE }
    private const int FADEOUT_DELAY = 30000;

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
    private string channelId;
    private ChatWindowVisualState currentState;
    private CancellationTokenSource deactivatePreviewCancellationToken = new CancellationTokenSource();
    private CancellationTokenSource deactivateFadeOutCancellationToken = new CancellationTokenSource();

    private bool skipChatInputTrigger;
    private string lastPrivateMessageRecipient = string.Empty;

    private UserProfile ownProfile => userProfileBridge.GetOwn();
    internal BaseVariable<HashSet<string>> visibleTaskbarPanels => dataStore.HUDs.visibleTaskbarPanels;
    internal BaseVariable<Transform> notificationPanelTransform => dataStore.HUDs.notificationPanelTransform;

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

        if (notificationPanelTransform.Get() == null)
        {
            view.OnFocused += HandleViewFocused;
            View.OnClickOverWindow += HandleViewClicked;
        }
        if (mouseCatcher != null)
            mouseCatcher.OnMouseLock += ActivatePreview;

        chatHudController = new ChatHUDController(dataStore,
            userProfileBridge,
            true,
            profanityFilter);
        chatHudController.Initialize(view.ChatHUD);
        chatHudController.OnSendMessage += SendChatMessage;
        chatHudController.OnInputFieldSelected -= HandleInputFieldSelected;
        chatHudController.OnInputFieldSelected += HandleInputFieldSelected;
        chatHudController.OnInputFieldDeselected -= HandleInputFieldDeselected;
        chatHudController.OnInputFieldDeselected += HandleInputFieldDeselected;

        chatController.OnAddMessage -= HandleMessageReceived;
        chatController.OnAddMessage += HandleMessageReceived;

        toggleChatTrigger.OnTriggered += HandleChatInputTriggered;
        
        currentState = ChatWindowVisualState.PREVIEW_MODE;
        WaitThenFadeOutMessages(deactivateFadeOutCancellationToken.Token).Forget();
    }

    public void Setup(string channelId)
    {
        if (string.IsNullOrEmpty(channelId) || channelId == this.channelId) return;
        this.channelId = channelId;

        var channel = chatController.GetAllocatedChannel(channelId);
        View.Configure(new PublicChatModel(this.channelId,
            channel.Name,
            channel.Description,
            channel.LastMessageTimestamp,
            channel.Joined,
            channel.MemberCount));

        ReloadAllChats().Forget();
    }

    public void Dispose()
    {
        View.OnClose -= HandleViewClosed;
        View.OnBack -= HandleViewBacked;

        if (chatController != null)
            chatController.OnAddMessage -= HandleMessageReceived;

        chatHudController.OnSendMessage -= SendChatMessage;
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
        SetVisiblePanelList(visible);
        if (visible)
        {
            View.Show();
            MarkChannelMessagesAsRead();
            
            if (focusInputField)
                chatHudController.FocusInputField();
        }
        else
        {   
            chatHudController.UnfocusInputField();
            View.Hide();
        }
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
        SetVisiblePanelList(false);
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

    internal void MarkChannelMessagesAsRead() => chatController.MarkChannelMessagesAsSeen(channelId);

    private void HandleViewClosed()
    {
        OnClosed?.Invoke();
    }

    private void HandleViewBacked() 
    {
        OnBack?.Invoke(); 
    }

    private void HandleMessageReceived(ChatMessage message)
    {
        if (message.messageType != ChatMessage.Type.PUBLIC
            && message.messageType != ChatMessage.Type.SYSTEM) return;
        if (!string.IsNullOrEmpty(message.recipient)) return;

        chatHudController.AddChatMessage(message, View.IsActive);

        if (View.IsActive)
            MarkChannelMessagesAsRead();
        
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
        if (notificationPanelTransform.Get() == null)
            await UniTask.Delay(FADEOUT_DELAY, cancellationToken: cancellationToken);

        await UniTask.SwitchToMainThread(cancellationToken);
        if (cancellationToken.IsCancellationRequested) return;
        chatHudController.FadeOutMessages();
        currentState = ChatWindowVisualState.NONE_VISIBLE;
    }
    
    public void ActivatePreview()
    {
        SetVisiblePanelList(false);
        View.ActivatePreview();
        chatHudController.ActivatePreview();
        currentState = ChatWindowVisualState.PREVIEW_MODE;
        WaitThenFadeOutMessages(deactivateFadeOutCancellationToken.Token).Forget();
        OnPreviewModeChanged?.Invoke(true);
    }
    
    public void ActivatePreviewOnMessages()
    {
        SetVisiblePanelList(false);
        chatHudController.ActivatePreview();
        OnPreviewModeChanged?.Invoke(true);
        currentState = ChatWindowVisualState.PREVIEW_MODE;
    }

    public void DeactivatePreview()
    {
        SetVisiblePanelList(true);
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
        if (!View.IsActive) return;
        chatHudController.FocusInputField();
    }
}