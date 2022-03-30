using System;
using System.Linq;
using DCL;
using DCL.Helpers;
using DCL.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class PublicChatChannelController : IHUD
{
    private const string PLAYER_PREFS_LAST_READ_WORLD_CHAT_MESSAGES = "LastReadWorldChatMessages";

    public IChannelChatWindowView view;
    public event Action OnBack;
    public event Action OnClosed;

    private readonly IChatController chatController;
    private readonly IMouseCatcher mouseCatcher;
    private readonly IPlayerPrefs playerPrefs;
    private readonly LongVariable lastReadWorldChatMessages;
    private readonly IUserProfileBridge userProfileBridge;
    private ChatHUDController chatHudController;
    private int invalidSubmitLastFrame;
    private ChatMessage lastWhisperMessageSent;

    internal string lastPrivateMessageReceivedSender = string.Empty;

    private UserProfile ownProfile => UserProfile.GetOwnUserProfile();
    private bool isSocialBarV1Enabled => DataStore.i.featureFlags.flags.Get().IsFeatureEnabled("social_bar_v1");

    public PublicChatChannelController(IChatController chatController,
        IMouseCatcher mouseCatcher,
        IPlayerPrefs playerPrefs,
        LongVariable lastReadWorldChatMessages,
        IUserProfileBridge userProfileBridge)
    {
        this.chatController = chatController;
        this.mouseCatcher = mouseCatcher;
        this.playerPrefs = playerPrefs;
        this.lastReadWorldChatMessages = lastReadWorldChatMessages;
        this.userProfileBridge = userProfileBridge;
    }

    public void Initialize(IChannelChatWindowView view = null)
    {
        if (view == null)
        {
            if (isSocialBarV1Enabled)
                view = PublicChatChannelComponentView.Create();
            else
                view = ChannelChatWindowView.Create();
        }
        
        this.view = view;
        view.OnClose += HandleViewClosed;
        view.OnBack += HandleViewBacked;

        chatHudController = new ChatHUDController(DataStore.i,
            new UserProfileWebInterfaceBridge(),
            ProfanityFilterSharedInstances.regexFilter);
        chatHudController.Initialize(view.ChatHUD);
        chatHudController.OnSendMessage += SendChatMessage;
        chatHudController.OnMessageUpdated += OnMessageUpdated;
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
        chatHudController.OnMessageUpdated -= OnMessageUpdated;

        view?.Dispose();
    }

    public void SendChatMessage(ChatMessage message)
    {
        bool isValidMessage = !string.IsNullOrEmpty(message.body) && !string.IsNullOrWhiteSpace(message.body);
        bool isPrivateMessage = message.messageType == ChatMessage.Type.PRIVATE;
        
        if (isPrivateMessage && !string.IsNullOrEmpty(message.body))
            lastWhisperMessageSent = message;
        else
            lastWhisperMessageSent = null;

        chatHudController.SetInputFieldText(lastWhisperMessageSent != null
            ? $"/w {lastWhisperMessageSent.recipient} "
            : string.Empty);

        if (!isValidMessage)
        {
            chatHudController.ResetInputField(true);

            if (!isPrivateMessage && !view.IsPreview)
            {
                view.ActivatePreview();
                SceneReferences.i.mouseCatcher.LockCursor();
                invalidSubmitLastFrame = Time.frameCount;
            }

            return;
        }

        chatHudController.ResetInputField();
        chatHudController.FocusInputField();

        if (isPrivateMessage)
        {
            message.body = $"/w {message.recipient} {message.body}";
        }

        chatController.Send(message);
    }

    public void SetVisibility(bool visible)
    {
        if (!visible)
            view.Hide();
        else
            view.Show();
    }

    public bool OnPressReturn()
    {
        if (EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject != null &&
            EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() != null)
            return false;

        if (Time.frameCount - invalidSubmitLastFrame < 2)
            return false;

        ForceFocus();
        return true;
    }
    
    public void MarkChatMessagesAsRead(long timestamp = 0)
    {
        long timeMark = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (timestamp != 0 && timestamp > timeMark)
            timeMark = timestamp;

        lastReadWorldChatMessages.Set(timeMark);
        SaveLatestReadWorldChatMessagesStatus();
    }

    public void ResetInputField() => chatHudController.ResetInputField();

    private void ForceFocus(string setInputText = null)
    {
        SetVisibility(true);
        chatHudController.FocusInputField();
        view.DeactivatePreview();
        SceneReferences.i?.mouseCatcher.UnlockCursor();

        if (!string.IsNullOrEmpty(setInputText))
            chatHudController.SetInputFieldText(setInputText);
    }

    private void SaveLatestReadWorldChatMessagesStatus()
    {
        playerPrefs.Set(PLAYER_PREFS_LAST_READ_WORLD_CHAT_MESSAGES,
            lastReadWorldChatMessages.Get().ToString());
        playerPrefs.Save();
    }

    private void HandleViewClosed() => OnClosed?.Invoke();
    
    private void HandleViewBacked() => OnBack?.Invoke();

    private void OnMessageUpdated(string message)
    {
        if (!string.IsNullOrEmpty(lastPrivateMessageReceivedSender) && message == "/r ")
            chatHudController.SetInputFieldText($"/w {lastPrivateMessageReceivedSender} ");
    }

    private bool IsOldPrivateMessage(ChatMessage message)
    {
        if (message.messageType != ChatMessage.Type.PRIVATE)
            return false;

        double timestampAsSeconds = message.timestamp / 1000.0f;

        if (timestampAsSeconds < chatController.initTime)
            return true;

        return false;
    }

    private void HandleMessageReceived(ChatMessage message)
    {
        if (IsOldPrivateMessage(message)) return;

        chatHudController.AddChatMessage(message, view.IsPreview);

        if (message.messageType == ChatMessage.Type.PRIVATE && message.recipient == ownProfile.userId)
            lastPrivateMessageReceivedSender = userProfileBridge.Get(message.sender).userName;
    }

    private void LoadLatestReadWorldChatMessagesStatus()
    {
        CommonScriptableObjects.lastReadWorldChatMessages.Set(0);
        string storedLastReadWorldChatMessagesString =
            PlayerPrefsUtils.GetString(PLAYER_PREFS_LAST_READ_WORLD_CHAT_MESSAGES);
        CommonScriptableObjects.lastReadWorldChatMessages.Set(Convert.ToInt64(
            string.IsNullOrEmpty(storedLastReadWorldChatMessagesString)
                ? 0
                : Convert.ToInt64(storedLastReadWorldChatMessagesString)));
    }
}