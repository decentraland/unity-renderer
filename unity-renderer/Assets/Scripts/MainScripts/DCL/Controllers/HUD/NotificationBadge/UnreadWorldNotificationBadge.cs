using DCL;
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
    private ILastReadMessagesService lastReadMessagesService;

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
                    : string.Format("+{0}", maxNumberToShow);
            }
            else
            {
                notificationContainer.SetActive(false);
            }
        }
    }

    private void Start()
    {
        Initialize(Environment.i.serviceLocator.Get<ILastReadMessagesService>());
    }

    /// <summary>
    /// Prepares the notification badge for listening to the world chat
    /// </summary>
    /// <param name="chatController">Chat Controlled to be listened</param>
    public void Initialize(ILastReadMessagesService lastReadMessagesService)
    {
        this.lastReadMessagesService = lastReadMessagesService;
        lastReadMessagesService.OnUpdated += UpdateUnreadMessages;
        UpdateUnreadMessages();
    }

    private void OnDestroy()
    {
        lastReadMessagesService.OnUpdated -= UpdateUnreadMessages;
    }

    private void UpdateUnreadMessages(string channelId) => UpdateUnreadMessages();

    private void UpdateUnreadMessages() =>
        CurrentUnreadMessages = lastReadMessagesService.GetAllUnreadCount();
}