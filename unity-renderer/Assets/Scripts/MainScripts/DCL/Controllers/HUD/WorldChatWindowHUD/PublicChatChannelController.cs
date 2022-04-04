using System;
using System.Linq;
using DCL;
using DCL.Helpers;
using DCL.Interface;

public class PublicChatChannelController : IHUD
{
    private const string LAST_READ_MESSAGES_PREFS_KEY = "LastReadWorldChatMessages";

    public IChannelChatWindowView view;
    public event Action OnBack;
    public event Action OnClosed;

    private readonly IChatController chatController;
    private readonly IMouseCatcher mouseCatcher;
    private readonly IPlayerPrefs playerPrefs;
    private readonly LongVariable lastReadWorldChatMessages;
    private readonly IUserProfileBridge userProfileBridge;
    private readonly InputAction_Trigger closeWindowTrigger;
    private ChatHUDController chatHudController;

    internal string lastPrivateMessageRecipient = string.Empty;
    private double initTimeInSeconds;

    private UserProfile ownProfile => UserProfile.GetOwnUserProfile();

    public PublicChatChannelController(IChatController chatController,
        IMouseCatcher mouseCatcher,
        IPlayerPrefs playerPrefs,
        LongVariable lastReadWorldChatMessages,
        IUserProfileBridge userProfileBridge,
        InputAction_Trigger closeWindowTrigger)
    {
        this.chatController = chatController;
        this.mouseCatcher = mouseCatcher;
        this.playerPrefs = playerPrefs;
        this.lastReadWorldChatMessages = lastReadWorldChatMessages;
        this.userProfileBridge = userProfileBridge;
        this.closeWindowTrigger = closeWindowTrigger;
    }

    public void Initialize(IChannelChatWindowView view = null)
    {
        view ??= PublicChatChannelComponentView.Create();
        this.view = view;
        view.OnClose += HandleViewClosed;
        view.OnBack += HandleViewBacked;
        closeWindowTrigger.OnTriggered -= HandleCloseInputTriggered;
        closeWindowTrigger.OnTriggered += HandleCloseInputTriggered;

        chatHudController = new ChatHUDController(DataStore.i,
            new UserProfileWebInterfaceBridge(),
            true,
            ProfanityFilterSharedInstances.regexFilter);
        chatHudController.Initialize(view.ChatHUD);
        chatHudController.OnSendMessage += SendChatMessage;
        chatHudController.OnMessageUpdated += HandleMessageInputUpdated;
        chatHudController.OnInputFieldSelected += HandleInputFieldSelected;
        LoadLatestReadWorldChatMessagesStatus();

        if (chatController != null)
        {
            chatController.OnAddMessage -= HandleMessageReceived;
            chatController.OnAddMessage += HandleMessageReceived;
        }

        if (mouseCatcher != null)
        {
            mouseCatcher.OnMouseLock += view.ActivatePreview;
        }
        
        initTimeInSeconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
    }

    public void Setup(string channelId)
    {
        // TODO: retrieve data from a channel provider
        view.Setup(channelId, "General", "Any useful description here");
        
        chatHudController.ClearAllEntries();
        var messageEntries = chatController.GetEntries()
            .ToList();
        foreach (var v in messageEntries)
            HandleMessageReceived(v);
    }

    public void Dispose()
    {
        view.OnClose -= HandleViewClosed;
        view.OnBack -= HandleViewBacked;

        if (chatController != null)
            chatController.OnAddMessage -= HandleMessageReceived;

        if (mouseCatcher != null)
            mouseCatcher.OnMouseLock -= view.ActivatePreview;

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

            if (!isPrivateMessage && !view.IsPreview)
            {
                view.ActivatePreview();
                mouseCatcher.LockCursor();
            }

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
        if (!visible)
            view.Hide();
        else
        {
            view.Show();
            MarkChatMessagesAsRead();
            chatHudController.FocusInputField();
        }
    }

    private void MarkChatMessagesAsRead(long timestamp = 0)
    {
        long timeMark = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (timestamp != 0 && timestamp > timeMark)
            timeMark = timestamp;

        lastReadWorldChatMessages.Set(timeMark);
        SaveLatestReadWorldChatMessagesStatus();
    }

    public void ResetInputField() => chatHudController.ResetInputField();

    private void HandleCloseInputTriggered(DCLAction_Trigger action) => HandleViewClosed();

    private void SaveLatestReadWorldChatMessagesStatus()
    {
        playerPrefs.Set(LAST_READ_MESSAGES_PREFS_KEY,
            lastReadWorldChatMessages.Get().ToString());
        playerPrefs.Save();
    }

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

        chatHudController.AddChatMessage(message, view.IsPreview);

        if (message.messageType == ChatMessage.Type.PRIVATE && message.recipient == ownProfile.userId)
            lastPrivateMessageRecipient = userProfileBridge.Get(message.sender).userName;
    }

    private void LoadLatestReadWorldChatMessagesStatus()
    {
        CommonScriptableObjects.lastReadWorldChatMessages.Set(0);
        string storedLastReadWorldChatMessagesString =
            PlayerPrefsUtils.GetString(LAST_READ_MESSAGES_PREFS_KEY);
        CommonScriptableObjects.lastReadWorldChatMessages.Set(Convert.ToInt64(
            string.IsNullOrEmpty(storedLastReadWorldChatMessagesString)
                ? 0
                : Convert.ToInt64(storedLastReadWorldChatMessagesString)));
    }
}