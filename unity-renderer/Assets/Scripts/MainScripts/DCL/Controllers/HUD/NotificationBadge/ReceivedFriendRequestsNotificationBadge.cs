using DCL;
using DCl.Social.Friends;
using DCL.Social.Friends;
using TMPro;
using UnityEngine;

public class ReceivedFriendRequestsNotificationBadge : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private GameObject notificationContainer;
    [SerializeField] private int maxNumberToShow = 9;

    private IFriendsController friendsController;
    private int currentUnreadMessagesValue;

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
        Initialize(Environment.i.serviceLocator.Get<IFriendsController>());
    }

    private void OnEnable()
    {
        UpdateReceivedRequests();
    }

    public void Initialize(IFriendsController friendsController)
    {
        if (friendsController == null) return;
        this.friendsController = friendsController;
        friendsController.OnTotalFriendRequestUpdated += UpdateReceivedRequests;
        UpdateReceivedRequests();
    }

    private void OnDestroy()
    {
        if (friendsController != null)
            friendsController.OnTotalFriendRequestUpdated -= UpdateReceivedRequests;
    }

    private void UpdateReceivedRequests(int totalReceivedFriendRequests, int totalSentFriendRequests) =>
        CurrentUnreadMessages = totalReceivedFriendRequests;

    private void UpdateReceivedRequests()
    {
        if (friendsController == null) return;
        CurrentUnreadMessages = friendsController.TotalReceivedFriendRequestCount;
    }
}
