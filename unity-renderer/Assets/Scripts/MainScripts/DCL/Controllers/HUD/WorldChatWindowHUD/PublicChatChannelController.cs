using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Interface;
using SocialFeaturesAnalytics;

public class PublicChatChannelController : IHUD
{
    public IChannelChatWindowView View { get; private set; }
    public event Action OnBack;
    public event Action OnClosed;

    private readonly IChatController chatController;
    private readonly ILastReadMessagesService lastReadMessagesService;
    private readonly IUserProfileBridge userProfileBridge;
    private readonly DataStore dataStore;
    private readonly IProfanityFilter profanityFilter;
    private readonly ISocialAnalytics socialAnalytics;
    private readonly IMouseCatcher mouseCatcher;
    private ChatHUDController chatHudController;
    private double initTimeInSeconds;
    private string channelId;
    private CancellationTokenSource deactivatePreviewCancellationToken = new CancellationTokenSource();
    internal string lastPrivateMessageRecipient = string.Empty;

    private UserProfile ownProfile => userProfileBridge.GetOwn();

    public PublicChatChannelController(IChatController chatController,
        ILastReadMessagesService lastReadMessagesService,
        IUserProfileBridge userProfileBridge,
        DataStore dataStore,
        IProfanityFilter profanityFilter,
        ISocialAnalytics socialAnalytics,
        IMouseCatcher mouseCatcher)
    {
        this.chatController = chatController;
        this.lastReadMessagesService = lastReadMessagesService;
        this.userProfileBridge = userProfileBridge;
        this.dataStore = dataStore;
        this.profanityFilter = profanityFilter;
        this.socialAnalytics = socialAnalytics;
        this.mouseCatcher = mouseCatcher;
    }

    public void Initialize(IChannelChatWindowView view = null)
    {
        view ??= PublicChatChannelComponentView.Create();
        View = view;
        view.OnClose += HandleViewClosed;
        view.OnBack += HandleViewBacked;
        view.OnFocused += HandleViewFocused;

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

        mouseCatcher.OnMouseLock += view.ActivatePreview;

        initTimeInSeconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
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
        
        mouseCatcher.OnMouseLock -= View.ActivatePreview;
        mouseCatcher.OnMouseUnlock -= View.DeactivatePreview;

        if (View != null)
        {
            View.OnFocused -= HandleViewFocused;
            View.Dispose();
        }
    }

    public void SendChatMessage(ChatMessage message)
    {
        bool isValidMessage = !string.IsNullOrEmpty(message.body) && !string.IsNullOrWhiteSpace(message.body);
        bool isPrivateMessage = message.messageType == ChatMessage.Type.PRIVATE;

        if (isPrivateMessage && isValidMessage)
            lastPrivateMessageRecipient = message.recipient;
        else
            lastPrivateMessageRecipient = null;

        if (!isValidMessage)
        {
            chatHudController.ResetInputField(true);
            return;
        }

        chatHudController.ResetInputField();
        chatHudController.FocusInputField();

        if (isPrivateMessage)
            message.body = $"/w {message.recipient} {message.body}";

        chatController.Send(message);
        socialAnalytics.SendChannelMessageSent(message.sender, message.body.Length, channelId);
    }

    public void SetVisibility(bool visible)
    {
        if (visible)
        {
            View.Show();
            MarkChatMessagesAsRead();
            chatHudController.FocusInputField();
        }
        else
            View.Hide();
    }

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
    }
    
    private void HandleInputFieldSelected()
    {
        deactivatePreviewCancellationToken.Cancel();
        deactivatePreviewCancellationToken = new CancellationTokenSource();
        View.DeactivatePreview();
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
            View.DeactivatePreview();
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
        View.ActivatePreview();
    }
}