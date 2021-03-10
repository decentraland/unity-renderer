using DCL.Interface;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// Shows the number of unread messages from a the world chat.
/// </summary>
public class UnreadWorldNotificationBadge : MonoBehaviour
{
    public TextMeshProUGUI notificationText;
    public GameObject notificationContainer;
    public int maxNumberToShow = 9;

    private IChatController currentChatController;
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

    private void Start()
    {
        Initialize(ChatController.i);
    }

    /// <summary>
    /// Prepares the notification badge for listening to the world chat
    /// </summary>
    /// <param name="chatController">Chat Controlled to be listened</param>
    public void Initialize(IChatController chatController)
    {
        if (chatController == null)
            return;

        currentChatController = chatController;
        currentTimestampReading = CommonScriptableObjects.lastReadWorldChatMessages.Get();
        UpdateUnreadMessages();

        currentChatController.OnAddMessage -= ChatController_OnAddMessage;
        currentChatController.OnAddMessage += ChatController_OnAddMessage;

        CommonScriptableObjects.lastReadWorldChatMessages.OnChange -= LastReadWorldChatMessages_OnChange;
        CommonScriptableObjects.lastReadWorldChatMessages.OnChange += LastReadWorldChatMessages_OnChange;
    }

    private void OnDestroy()
    {
        if (currentChatController == null)
            return;

        currentChatController.OnAddMessage -= ChatController_OnAddMessage;
        CommonScriptableObjects.lastReadWorldChatMessages.OnChange -= LastReadWorldChatMessages_OnChange;
    }

    private void ChatController_OnAddMessage(ChatMessage newMessage)
    {
        if (newMessage.messageType == ChatMessage.Type.PUBLIC &&
            newMessage.sender != UserProfile.GetOwnUserProfile().userId)
        {
            // A new message from world is received
            UpdateUnreadMessages();
        }
    }

    private void LastReadWorldChatMessages_OnChange(long current, long previous)
    {
        // The player reads the latest messages of [userId]
        currentTimestampReading = current;
        currentUnreadMessages = 0;
        UpdateUnreadMessages();
    }

    private void UpdateUnreadMessages()
    {
        currentUnreadMessages = currentChatController.GetEntries().Count(
            msg => msg.messageType == ChatMessage.Type.PUBLIC &&
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
