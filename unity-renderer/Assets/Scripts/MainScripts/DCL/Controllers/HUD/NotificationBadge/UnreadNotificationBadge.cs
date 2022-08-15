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

    private IChatController chatController;
    private string currentUserId;
    private int currentUnreadMessagesValue;
    private bool isInitialized;

    public int CurrentUnreadMessages
    {
        get => currentUnreadMessagesValue;
        set
        {
            currentUnreadMessagesValue = value;
            
            if (currentUnreadMessagesValue > 0)
            {
                notificationContainer.SetActive(true);
                notificationText.text = currentUnreadMessagesValue <= maxNumberToShow ? currentUnreadMessagesValue.ToString() : $"+{maxNumberToShow}";
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
    public void Initialize(IChatController chatController, string userId)
    {
        if (chatController == null)
            return;

        this.chatController = chatController;
        currentUserId = userId;

        UpdateUnreadMessages();

        chatController.OnUserUnseenMessagesUpdated -= HandleUnseenMessagesUpdated;
        chatController.OnUserUnseenMessagesUpdated += HandleUnseenMessagesUpdated;

        chatController.OnChannelUnseenMessagesUpdated -= HandleChannelUnseenMessagesUpdated;
        chatController.OnChannelUnseenMessagesUpdated += HandleChannelUnseenMessagesUpdated;

        isInitialized = true;
    }

    private void HandleUnseenMessagesUpdated(string userId, int unseenMessages)
    {
        if (userId != currentUserId) return;
        CurrentUnreadMessages = unseenMessages;
    }

    private void HandleChannelUnseenMessagesUpdated(string channelId, int unseenMessages)
    {
        if (channelId != currentUserId) return;
        CurrentUnreadMessages = unseenMessages;
    }

    private void OnEnable()
    {
        if (!isInitialized) return;
        UpdateUnreadMessages();
    }

    private void OnDestroy()
    {
        if (chatController != null)
        {
            chatController.OnUserUnseenMessagesUpdated -= HandleUnseenMessagesUpdated;
            chatController.OnChannelUnseenMessagesUpdated -= HandleChannelUnseenMessagesUpdated;
        }
    }

    private void UpdateUnreadMessages() =>
        CurrentUnreadMessages = chatController.GetAllocatedUnseenMessages(currentUserId) + chatController.GetAllocatedUnseenChannelMessages(currentUserId);
}