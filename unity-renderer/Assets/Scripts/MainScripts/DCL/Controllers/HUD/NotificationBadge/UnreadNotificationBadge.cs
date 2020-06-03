using DCL.Interface;
using System.Linq;
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
    private long currentTimestampReading;
    private int currentUnreadMessagesValue;

    public int currentUnreadMessages
    {
        get => currentUnreadMessagesValue;
        set
        {
            currentUnreadMessagesValue = value;
            RefreshNotificationBadge();
        }
    }

    /// <summary>
    /// Prepares the notification badge for listening to a specific user
    /// </summary>
    /// <param name="chatController">Chat Controlled to be listened</param>
    /// <param name="userId">User ID to listen to</param>
    public void Initialize(IChatController chatController, string userId)
    {
        if (chatController == null)
            return;

        currentChatController = chatController;
        currentUserId = userId;

        CommonScriptableObjects.lastReadChatMessages.TryGetValue(currentUserId, out currentTimestampReading);
        UpdateUnreadMessages();

        currentChatController.OnAddMessage -= ChatController_OnAddMessage;
        currentChatController.OnAddMessage += ChatController_OnAddMessage;

        CommonScriptableObjects.lastReadChatMessages.OnAdded -= LastReadChatMessages_OnAdded;
        CommonScriptableObjects.lastReadChatMessages.OnAdded += LastReadChatMessages_OnAdded;
    }

    private void OnDestroy()
    {
        if (currentChatController == null)
            return;

        currentChatController.OnAddMessage -= ChatController_OnAddMessage;
        CommonScriptableObjects.lastReadChatMessages.OnAdded -= LastReadChatMessages_OnAdded;
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

    private void LastReadChatMessages_OnAdded(string addedKey, long addedValue)
    {
        if (addedKey == currentUserId)
        {
            // The player reads the latest messages of [userId]
            currentTimestampReading = addedValue;
            currentUnreadMessages = 0;
            UpdateUnreadMessages();
        }
    }

    private void UpdateUnreadMessages()
    {
        currentUnreadMessages = currentChatController.GetEntries().Count(
            msg => msg.messageType == ChatMessage.Type.PRIVATE &&
            msg.sender == currentUserId &&
            msg.timestamp > (ulong)currentTimestampReading);
    }

    private void RefreshNotificationBadge()
    {
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
