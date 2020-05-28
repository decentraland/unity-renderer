using DCL.Interface;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PrivateChatWindowHUDController : IHUD
{
    internal const string PLAYER_PREFS_LAST_READ_CHAT_MESSAGES = "LastReadChatMessages";

    public PrivateChatWindowHUDView view;
    public bool resetInputFieldOnSubmit = true;

    ChatHUDController chatHudController;
    IChatController chatController;
    public string conversationUserId { get; private set; } = string.Empty;
    public string conversationUserName { get; private set; } = string.Empty;

    public event System.Action OnPressBack;

    public void Initialize(IChatController chatController)
    {
        view = PrivateChatWindowHUDView.Create(this);
        view.OnPressBack -= View_OnPressBack;
        view.OnPressBack += View_OnPressBack;

        view.chatHudView.inputField.onSelect.RemoveListener(ChatHUDViewInputField_OnSelect);
        view.chatHudView.inputField.onSelect.AddListener(ChatHUDViewInputField_OnSelect);

        chatHudController = new ChatHUDController();
        chatHudController.Initialize(view.chatHudView);
        LoadLatestReadChatMessagesStatus();

        view.OnSendMessage += SendChatMessage;

        this.chatController = chatController;

        if (chatController != null)
        {
            chatController.OnAddMessage -= OnAddMessage;
            chatController.OnAddMessage += OnAddMessage;
        }

        SetVisibility(false);
    }

    void View_OnPressBack()
    {
        OnPressBack?.Invoke();
    }

    public void Configure(string newConversationUserId)
    {
        if (string.IsNullOrEmpty(newConversationUserId) || newConversationUserId == conversationUserId) return;

        UserProfile newConversationUserProfile = UserProfileController.userProfilesCatalog.Get(newConversationUserId);

        conversationUserId = newConversationUserId;
        conversationUserName = newConversationUserProfile.userName;

        view.ConfigureTitle(conversationUserName);
        view.ConfigureProfilePicture(newConversationUserProfile.faceSnapshot);
        view.ConfigureJumpInButton(newConversationUserProfile.userId);

        view.chatHudView.CleanAllEntries();

        var messageEntries = chatController.GetEntries().Where((x) => IsMessageFomCurrentConversation(x)).ToList();
        foreach (var v in messageEntries)
        {
            OnAddMessage(v);
        }
    }

    public void SendChatMessage(ChatMessage message)
    {
        if (string.IsNullOrEmpty(conversationUserName)) return;

        bool isValidMessage = !string.IsNullOrEmpty(message.body) && !string.IsNullOrWhiteSpace(message.body) && !string.IsNullOrEmpty(message.recipient);

        if (!isValidMessage || resetInputFieldOnSubmit)
        {
            view.chatHudView.ResetInputField();
            view.chatHudView.FocusInputField();
        }

        if (!isValidMessage) return;

        // If Kernel allowed for private messages without the whisper param we could avoid this line
        message.body = $"/w {message.recipient} {message.body}";

        WebInterface.SendChatMessage(message);
    }

    public void SetVisibility(bool visible)
    {
        if (view.gameObject.activeSelf == visible) return;

        view.gameObject.SetActive(visible);

        if (visible)
        {
            // The messages from 'conversationUserId' are marked as read once the private chat is opened
            MarkUserChatMessagesAsRead(conversationUserId);
            view.chatHudView.scrollRect.verticalNormalizedPosition = 0;
        }
    }

    public void Dispose()
    {
        view.chatHudView.inputField.onSelect.RemoveListener(ChatHUDViewInputField_OnSelect);

        view.OnPressBack -= View_OnPressBack;

        if (chatController != null)
            chatController.OnAddMessage -= OnAddMessage;

        UnityEngine.Object.Destroy(view);
    }

    void OnAddMessage(ChatMessage message)
    {
        if (!IsMessageFomCurrentConversation(message)) return;

        view.chatHudView.controller.AddChatMessage(ChatHUDController.ChatMessageToChatEntry(message));

        if (view.chatHudView.inputField.isFocused)
        {
            // The messages from 'conversationUserId' are marked as read if the player was already focused on the input field of the private chat
            MarkUserChatMessagesAsRead(conversationUserId);
        }
    }

    bool IsMessageFomCurrentConversation(ChatMessage message)
    {
        return message.messageType == ChatMessage.Type.PRIVATE && (message.sender == conversationUserId || message.recipient == conversationUserId);
    }

    public void ForceFocus()
    {
        SetVisibility(true);
        view.chatHudView.FocusInputField();
    }

    private void MarkUserChatMessagesAsRead(string userId)
    {
        CommonScriptableObjects.lastReadChatMessages.Remove(userId);
        CommonScriptableObjects.lastReadChatMessages.Add(userId, System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        SaveLatestReadChatMessagesStatus();
    }

    private void SaveLatestReadChatMessagesStatus()
    {
        List<KeyValuePair<string, long>> lastReadChatMessagesList = new List<KeyValuePair<string, long>>();
        using (var iterator = CommonScriptableObjects.lastReadChatMessages.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                lastReadChatMessagesList.Add(new KeyValuePair<string, long>(iterator.Current.Key, iterator.Current.Value));
            }
        }

        PlayerPrefs.SetString(PLAYER_PREFS_LAST_READ_CHAT_MESSAGES, JsonConvert.SerializeObject(lastReadChatMessagesList));
        PlayerPrefs.Save();
    }

    private void LoadLatestReadChatMessagesStatus()
    {
        CommonScriptableObjects.lastReadChatMessages.Clear();

        List<KeyValuePair<string, long>> lastReadChatMessagesList = JsonConvert.DeserializeObject<List<KeyValuePair<string, long>>>(PlayerPrefs.GetString(PLAYER_PREFS_LAST_READ_CHAT_MESSAGES));
        if (lastReadChatMessagesList != null)
        {
            foreach (var item in lastReadChatMessagesList)
            {
                CommonScriptableObjects.lastReadChatMessages.Add(item.Key, item.Value);
            }
        }
    }

    private void ChatHUDViewInputField_OnSelect(string message)
    {
        // The messages from 'conversationUserId' are marked as read if the player clicks on the input field of the private chat
        MarkUserChatMessagesAsRead(conversationUserId);
    }
}
