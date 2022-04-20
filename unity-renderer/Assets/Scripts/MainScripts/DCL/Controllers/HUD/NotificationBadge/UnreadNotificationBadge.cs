using System;
using DCL.Interface;
using TMPro;
using UnityEngine;

/// <summary>
/// Shows the number of unread messages from a friend.
/// </summary>
public class UnreadNotificationBadge : MonoBehaviour
{
    public TextMeshProUGUI notificationText;
    public GameObject notificationContainer;
    public int maxNumberToShow = 9;

    private IChatController currentChatController;
    private string currentUserId;
    private int currentUnreadMessagesValue;
    private ILastReadMessagesService lastReadMessagesService;
    private bool isInitialized;

    public int currentUnreadMessages
    {
        get => currentUnreadMessagesValue;
        set
        {
            currentUnreadMessagesValue = value;
            
            if (currentUnreadMessagesValue > 0)
            {
                notificationContainer.SetActive(true);
                notificationText.text = currentUnreadMessagesValue <= maxNumberToShow ? currentUnreadMessagesValue.ToString() : string.Format("+{0}", maxNumberToShow);
            }
            else
            {
                notificationContainer.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Prepares the notification badge for listening to a specific user
    /// </summary>
    /// <param name="chatController">Chat Controlled to be listened</param>
    /// <param name="userId">User ID to listen to</param>
    /// <param name="lastReadMessagesService">Service that handles unread messages</param>
    public void Initialize(IChatController chatController, string userId, ILastReadMessagesService lastReadMessagesService)
    {
        if (chatController == null)
            return;

        this.lastReadMessagesService = lastReadMessagesService;
        currentChatController = chatController;
        currentUserId = userId;

        UpdateUnreadMessages();

        currentChatController.OnAddMessage -= ChatController_OnAddMessage;
        currentChatController.OnAddMessage += ChatController_OnAddMessage;

        isInitialized = true;
    }

    private void OnEnable()
    {
        if (!isInitialized) return;
        UpdateUnreadMessages();
    }

    private void OnDestroy()
    {
        if (currentChatController == null)
            return;

        currentChatController.OnAddMessage -= ChatController_OnAddMessage;
    }

    private void ChatController_OnAddMessage(ChatMessage newMessage)
    {
        if (newMessage.messageType == ChatMessage.Type.PRIVATE &&
            newMessage.sender == currentUserId)
        {
            // A new message from [userId] is received
            UpdateUnreadMessages();
        }
    }

    private void UpdateUnreadMessages() => currentUnreadMessages = lastReadMessagesService.GetUnreadCount(currentUserId);
}