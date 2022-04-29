using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Interface;

public class PublicChatChannelController : IHUD
{
    public IChannelChatWindowView view;
    public event Action OnBack;
    public event Action OnClosed;

    private readonly IChatController chatController;
    private readonly ILastReadMessagesService lastReadMessagesService;
    private readonly IUserProfileBridge userProfileBridge;
    private readonly InputAction_Trigger closeWindowTrigger;
    private readonly DataStore dataStore;
    private readonly RegexProfanityFilter regexProfanityFilter;
    private ChatHUDController chatHudController;
    private double initTimeInSeconds;
    private string channelId;
    internal string lastPrivateMessageRecipient = string.Empty;

    private UserProfile ownProfile => userProfileBridge.GetOwn();

    public PublicChatChannelController(IChatController chatController,
        ILastReadMessagesService lastReadMessagesService,
        IUserProfileBridge userProfileBridge,
        InputAction_Trigger closeWindowTrigger,
        DataStore dataStore,
        RegexProfanityFilter regexProfanityFilter)
    {
        this.chatController = chatController;
        this.lastReadMessagesService = lastReadMessagesService;
        this.userProfileBridge = userProfileBridge;
        this.closeWindowTrigger = closeWindowTrigger;
        this.dataStore = dataStore;
        this.regexProfanityFilter = regexProfanityFilter;
    }

    public void Initialize(IChannelChatWindowView view = null)
    {
        view ??= PublicChatChannelComponentView.Create();
        this.view = view;
        view.OnClose += HandleViewClosed;
        view.OnBack += HandleViewBacked;
        closeWindowTrigger.OnTriggered -= HandleCloseInputTriggered;
        closeWindowTrigger.OnTriggered += HandleCloseInputTriggered;

        chatHudController = new ChatHUDController(dataStore,
            userProfileBridge,
            true,
            regexProfanityFilter);
        chatHudController.Initialize(view.ChatHUD);
        chatHudController.OnSendMessage += SendChatMessage;
        chatHudController.OnMessageUpdated += HandleMessageInputUpdated;
        chatHudController.OnInputFieldSelected += HandleInputFieldSelected;

        chatController.OnAddMessage -= HandleMessageReceived;
        chatController.OnAddMessage += HandleMessageReceived;

        initTimeInSeconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
    }

    public void Setup(string channelId)
    {
        if (string.IsNullOrEmpty(channelId) || channelId == this.channelId) return;
        this.channelId = channelId;

        // TODO: retrieve data from a channel provider
        view.Setup(this.channelId, "General", "Any useful description here");

        ReloadAllChats().Forget();
    }

    public void Dispose()
    {
        view.OnClose -= HandleViewClosed;
        view.OnBack -= HandleViewBacked;

        if (chatController != null)
            chatController.OnAddMessage -= HandleMessageReceived;

        chatHudController.OnSendMessage -= SendChatMessage;
        chatHudController.OnMessageUpdated -= HandleMessageInputUpdated;

        view?.Dispose();
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
    }

    public void SetVisibility(bool visible)
    {
        if (visible)
        {
            view.Show();
            MarkChatMessagesAsRead();
            chatHudController.FocusInputField();
        }
        else
            view.Hide();
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

    public void ResetInputField() => chatHudController.ResetInputField();

    private void HandleCloseInputTriggered(DCLAction_Trigger action) => HandleViewClosed();

    private void HandleViewClosed() => OnClosed?.Invoke();

    private void HandleViewBacked() => OnBack?.Invoke();

    private void HandleMessageInputUpdated(string message)
    {
        if (!string.IsNullOrEmpty(lastPrivateMessageRecipient) && message == "/r ")
            chatHudController.SetInputFieldText($"/w {lastPrivateMessageRecipient} ");
    }

    private void HandleInputFieldSelected()
    {
        if (string.IsNullOrEmpty(lastPrivateMessageRecipient)) return;
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

        chatHudController.AddChatMessage(message, view.IsActive);

        if (message.messageType == ChatMessage.Type.PRIVATE && message.recipient == ownProfile.userId)
            lastPrivateMessageRecipient = userProfileBridge.Get(message.sender).userName;
    }
}