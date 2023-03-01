using DCL;
using DCL.Chat;
using DCL.Social.Chat;
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

    private int currentUnreadMessagesValue;
    private IChatController chatController;

    public int CurrentUnreadMessages
    {
        get => currentUnreadMessagesValue;
        set
        {
            currentUnreadMessagesValue = value;
            if (currentUnreadMessagesValue > 0)
            {
                notificationContainer.SetActive(true);
                notificationText.text = currentUnreadMessagesValue <= maxNumberToShow
                    ? currentUnreadMessagesValue.ToString()
                    : $"+{maxNumberToShow}";
            }
            else
            {
                notificationContainer.SetActive(false);
            }
        }
    }

    private void Start()
    {
        Initialize(Environment.i.serviceLocator.Get<IChatController>());
    }

    private void OnDestroy()
    {
        if (chatController != null)
            chatController.OnTotalUnseenMessagesUpdated -= UpdateTotalUnseenMessages;
    }

    private void OnEnable() => UpdateTotalUnseenMessages();

    /// <summary>
    /// Prepares the notification badge for listening to the world chat
    /// </summary>
    /// <param name="chatController">Chat Controlled to be listened</param>
    public void Initialize(IChatController chatController)
    {
        if (chatController == null) return;
        this.chatController = chatController;
        chatController.OnTotalUnseenMessagesUpdated += UpdateTotalUnseenMessages;
        UpdateTotalUnseenMessages();
    }

    private void UpdateTotalUnseenMessages(int totalUnseenMessages) =>
        CurrentUnreadMessages = totalUnseenMessages;

    private void UpdateTotalUnseenMessages()
    {
        if (chatController == null) return;
        CurrentUnreadMessages = chatController.TotalUnseenMessages;
    }
}
