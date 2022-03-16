using DCL.Interface;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Helpers;
using UnityEngine;

public class PrivateChatWindowHUDController : IHUD
{
    private const string PLAYER_PREFS_LAST_READ_CHAT_MESSAGES = "LastReadChatMessages";

    public IPrivateChatComponentView view;

    public string conversationUserId { get; private set; } = string.Empty;
    
    private string conversationUserName = string.Empty;
    private ChatHUDController chatHudController;
    private IChatController chatController;
    private UserProfile conversationProfile;

    public event System.Action OnPressBack;

    public void Initialize(IChatController chatController, IPrivateChatComponentView view = null)
    {
        view ??= PrivateChatWindowHUDView.Create();
        this.view = view;
        view.OnPressBack -= View_OnPressBack;
        view.OnPressBack += View_OnPressBack;
        view.OnInputFieldSelected -= ChatHUDViewInputField_OnSelect;
        view.OnInputFieldSelected += ChatHUDViewInputField_OnSelect;
        view.OnClose += OnCloseView;
        view.OnMinimize += OnMinimizeView;

        chatHudController = new ChatHUDController(DataStore.i);
        chatHudController.Initialize(view.ChatHUD);
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

    private void OnMinimizeView() => SetVisibility(false);

    public void Configure(string newConversationUserId)
    {
        if (string.IsNullOrEmpty(newConversationUserId) || newConversationUserId == conversationUserId)
            return;

        UserProfile newConversationUserProfile = UserProfileController.userProfilesCatalog.Get(newConversationUserId);

        conversationUserId = newConversationUserId;
        conversationUserName = newConversationUserProfile.userName;
        conversationProfile = newConversationUserProfile;

        view.Setup(newConversationUserProfile);

        view.CleanAllEntries();

        var messageEntries = chatController.GetEntries().Where(IsMessageFomCurrentConversation).ToList();
        foreach (var v in messageEntries)
        {
            OnAddMessage(v);
        }
    }

    public void SendChatMessage(ChatMessage message)
    {
        if (string.IsNullOrEmpty(conversationUserName))
            return;

        bool isValidMessage = !string.IsNullOrEmpty(message.body)
                              && !string.IsNullOrWhiteSpace(message.body)
                              && !string.IsNullOrEmpty(message.recipient);
      
        view.ResetInputField();
        view.FocusInputField();

        if (!isValidMessage)
            return;

        // If Kernel allowed for private messages without the whisper param we could avoid this line
        message.body = $"/w {message.recipient} {message.body}";

        WebInterface.SendChatMessage(message);
    }

    public void SetVisibility(bool visible)
    {
        if (view.IsActive == visible) return;

        if (visible)
        {
            if (conversationProfile != null)
                view.Setup(conversationProfile);
            view.Show();
            // The messages from 'conversationUserId' are marked as read once the private chat is opened
            MarkUserChatMessagesAsRead(conversationUserId);
        }
        else
        {
            view.Hide();
        }
    }

    public void Dispose()
    {
        view.OnInputFieldSelected -= ChatHUDViewInputField_OnSelect;
        view.OnPressBack -= View_OnPressBack;
        view.OnClose -= OnCloseView;
        view.OnMinimize -= OnMinimizeView;

        if (chatController != null)
            chatController.OnAddMessage -= OnAddMessage;

        view?.Dispose();
    }
    
    public void ForceFocus()
    {
        SetVisibility(true);
        view.FocusInputField();
    }

    private void OnAddMessage(ChatMessage message)
    {
        if (!IsMessageFomCurrentConversation(message))
            return;

        chatHudController.AddChatMessage(ChatHUDController.ChatMessageToChatEntry(message));

        if (conversationProfile.userId == conversationUserId)
        {
            // The messages from 'conversationUserId' are marked as read if his private chat window is currently open
            MarkUserChatMessagesAsRead(conversationUserId, (long) message.timestamp);
        }
    }
    
    private void OnCloseView() => SetVisibility(false);
    
    private void View_OnPressBack() => OnPressBack?.Invoke();

    private bool IsMessageFomCurrentConversation(ChatMessage message)
    {
        return message.messageType == ChatMessage.Type.PRIVATE &&
               (message.sender == conversationUserId || message.recipient == conversationUserId);
    }

    private void MarkUserChatMessagesAsRead(string userId, long? timestamp = null)
    {
        long timeMark = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (timestamp != null && timestamp.Value > timeMark)
            timeMark = timestamp.Value;

        CommonScriptableObjects.lastReadChatMessages.Remove(userId);
        CommonScriptableObjects.lastReadChatMessages.Add(userId, timeMark);
        SaveLatestReadChatMessagesStatus();
    }

    private void SaveLatestReadChatMessagesStatus()
    {
        List<KeyValuePair<string, long>> lastReadChatMessagesList = new List<KeyValuePair<string, long>>();
        using (var iterator = CommonScriptableObjects.lastReadChatMessages.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                lastReadChatMessagesList.Add(new KeyValuePair<string, long>(iterator.Current.Key,
                    iterator.Current.Value));
            }
        }

        PlayerPrefsUtils.SetString(PLAYER_PREFS_LAST_READ_CHAT_MESSAGES,
            JsonConvert.SerializeObject(lastReadChatMessagesList));
        PlayerPrefsUtils.Save();
    }

    private void LoadLatestReadChatMessagesStatus()
    {
        CommonScriptableObjects.lastReadChatMessages.Clear();

        List<KeyValuePair<string, long>> lastReadChatMessagesList =
            JsonConvert.DeserializeObject<List<KeyValuePair<string, long>>>(
                PlayerPrefs.GetString(PLAYER_PREFS_LAST_READ_CHAT_MESSAGES));
        if (lastReadChatMessagesList != null)
        {
            foreach (var item in lastReadChatMessagesList)
            {
                CommonScriptableObjects.lastReadChatMessages.Add(item.Key, item.Value);
            }
        }
    }

    private void ChatHUDViewInputField_OnSelect()
    {
        // The messages from 'conversationUserId' are marked as read if the player clicks on the input field of the private chat
        MarkUserChatMessagesAsRead(conversationUserId);
    }
}