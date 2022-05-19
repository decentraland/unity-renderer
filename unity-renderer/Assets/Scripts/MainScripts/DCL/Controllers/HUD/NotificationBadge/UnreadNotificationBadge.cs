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
    /// <param name="lastReadMessagesService">Service that handles unread messages</param>
    public void Initialize(IChatController chatController, string userId, ILastReadMessagesService lastReadMessagesService)
    {
        if (chatController == null)
            return;

        this.lastReadMessagesService = lastReadMessagesService;
        currentChatController = chatController;
        currentUserId = userId;

        UpdateUnreadMessages();

        currentChatController.OnAddMessage -= HandleMessageAdded;
        currentChatController.OnAddMessage += HandleMessageAdded;
        lastReadMessagesService.OnUpdated += HandleUnreadMessagesUpdated;

        isInitialized = true;
    }

    private void OnEnable()
    {
        if (!isInitialized) return;
        UpdateUnreadMessages();
    }

    private void OnDestroy()
    {
        if (currentChatController != null)
            currentChatController.OnAddMessage -= HandleMessageAdded;
        if (lastReadMessagesService != null)
            lastReadMessagesService.OnUpdated -= HandleUnreadMessagesUpdated;
    }

    private void HandleMessageAdded(ChatMessage newMessage) => UpdateUnreadMessages();

    private void HandleUnreadMessagesUpdated(string userId)
    {
        if (userId != currentUserId) return;
        UpdateUnreadMessages();
    }

    private void UpdateUnreadMessages() => CurrentUnreadMessages = lastReadMessagesService.GetUnreadCount(currentUserId);
}