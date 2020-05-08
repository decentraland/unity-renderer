using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class FriendRequestsTabView : FriendsTabViewBase
{
    [SerializeField] internal EntryList receivedRequestsList = new EntryList();
    [SerializeField] internal EntryList sentRequestsList = new EntryList();

    [SerializeField] internal TMP_InputField friendSearchInputField;
    [SerializeField] internal Button addFriendButton;

    [Header("Notifications")]
    [SerializeField] internal Notification requestSentNotification;
    [SerializeField] internal Notification friendSearchFailedNotification;
    [SerializeField] internal Notification acceptedFriendNotification;

    public event System.Action<FriendRequestEntry> OnCancelConfirmation;
    public event System.Action<FriendRequestEntry> OnRejectConfirmation;
    public event System.Action<FriendRequestEntry> OnFriendRequestApproved;
    public event System.Action<string> OnFriendRequestSent;

    public override void Initialize(FriendsHUDView owner)
    {
        base.Initialize(owner);

        receivedRequestsList.toggleTextPrefix = "RECEIVED";
        sentRequestsList.toggleTextPrefix = "SENT";

        requestSentNotification.model.timer = owner.notificationsDuration;
        requestSentNotification.model.groupID = FriendsHUDView.NOTIFICATIONS_ID;

        friendSearchFailedNotification.model.timer = owner.notificationsDuration;
        friendSearchFailedNotification.model.groupID = FriendsHUDView.NOTIFICATIONS_ID;

        acceptedFriendNotification.model.timer = owner.notificationsDuration;
        acceptedFriendNotification.model.groupID = FriendsHUDView.NOTIFICATIONS_ID;

        friendSearchInputField.onSubmit.AddListener(SendFriendRequest);
        friendSearchInputField.onValueChanged.AddListener(OnSearchInputValueChanged);
        addFriendButton.onClick.AddListener(() => friendSearchInputField.OnSubmit(null));
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        NotificationsController.i?.DismissAllNotifications(FriendsHUDView.NOTIFICATIONS_ID);
    }

    public void CreateOrUpdateEntry(string userId, FriendEntry.Model model, bool isReceived)
    {
        CreateEntry(userId);
        UpdateEntry(userId, model, isReceived);
    }

    public override bool CreateEntry(string userId)
    {
        if (!base.CreateEntry(userId))
            return false;

        FriendRequestEntry entry = GetEntry(userId) as FriendRequestEntry;

        entry.OnAccepted += OnFriendRequestReceivedAccepted;
        entry.OnRejected += OnEntryRejectButtonPressed;
        entry.OnCancelled += OnEntryCancelButtonPressed;
        return true;
    }

    public override bool RemoveEntry(string userId)
    {
        if (!base.RemoveEntry(userId))
            return false;

        receivedRequestsList.Remove(userId);
        sentRequestsList.Remove(userId);
        return true;
    }

    public bool UpdateEntry(string userId, FriendEntryBase.Model model, bool? isReceived = null)
    {
        if (!base.UpdateEntry(userId, model))
            return false;

        FriendRequestEntry entry = entries[userId] as FriendRequestEntry;

        if (isReceived.HasValue)
        {
            entry.SetReceived(isReceived.Value);

            if (isReceived.Value)
                receivedRequestsList.Add(userId, entry);
            else
                sentRequestsList.Add(userId, entry);
        }

        return true;
    }

    void SendFriendRequest(string friendId)
    {
        requestSentNotification.model.message = $"Your request to {friendId} successfully sent!";
        NotificationsController.i.ShowNotification(requestSentNotification);

        friendSearchInputField.placeholder.enabled = true;
        friendSearchInputField.text = string.Empty;

        addFriendButton.gameObject.SetActive(false);

        OnFriendRequestSent?.Invoke(friendId);
    }

    public void DisplayFriendUserNotFound()
    {
        NotificationsController.i.ShowNotification(friendSearchFailedNotification);
        addFriendButton.interactable = false;
    }

    void OnSearchInputValueChanged(string friendId)
    {
        if (!addFriendButton.gameObject.activeSelf)
            addFriendButton.gameObject.SetActive(true);

        if (!addFriendButton.interactable)
            addFriendButton.interactable = true;

        if (!string.IsNullOrEmpty(friendId))
            NotificationsController.i.DismissAllNotifications(FriendsHUDView.NOTIFICATIONS_ID);
    }

    void OnFriendRequestReceivedAccepted(FriendRequestEntry requestEntry)
    {
        // Add placeholder friend to avoid affecting UX by roundtrip with kernel
        FriendsController.i.UpdateFriendshipStatus(new FriendsController.FriendshipUpdateStatusMessage()
        {
            userId = requestEntry.userId,
            action = FriendsController.FriendshipAction.APPROVED
        });

        FriendsController.i.UpdateUserStatus(new FriendsController.UserStatus()
        {
            userId = requestEntry.userId,
            presence = FriendsController.PresenceStatus.OFFLINE
        });

        acceptedFriendNotification.model.message = $"You and {requestEntry.model.userName} are now friends!";
        NotificationsController.i.ShowNotification(acceptedFriendNotification);

        RemoveEntry(requestEntry.userId);

        OnFriendRequestApproved?.Invoke(requestEntry);
    }

    void OnEntryRejectButtonPressed(FriendRequestEntry requestEntry)
    {
        confirmationDialog.SetText($"Are you sure you want to reject {requestEntry.model.userName} friend request?");
        confirmationDialog.Show(() =>
        {
            RemoveEntry(requestEntry.userId);
            OnRejectConfirmation?.Invoke(requestEntry as FriendRequestEntry);
        });
    }

    void OnEntryCancelButtonPressed(FriendRequestEntry requestEntry)
    {
        confirmationDialog.SetText($"Are you sure you want to cancel {requestEntry.model.userName} friend request?");
        confirmationDialog.Show(() =>
        {
            RemoveEntry(requestEntry.userId);
            OnCancelConfirmation?.Invoke(requestEntry as FriendRequestEntry);
        });
    }

    [ContextMenu("AddFakeRequestReceived")]
    public void AddFakeRequestReceived()
    {
        string id1 = Random.Range(0, 1000000).ToString();
        UserProfileController.i.AddUserProfileToCatalog(new UserProfileModel()
        {
            userId = id1,
            name = "Pravus-" + id1
        });

        FriendsController.i.UpdateFriendshipStatus(new FriendsController.FriendshipUpdateStatusMessage()
        {
            userId = id1,
            action = FriendsController.FriendshipAction.REQUESTED_FROM
        });
    }

    [ContextMenu("AddFakeRequestSent")]
    public void AddFakeRequestSent()
    {
        string id1 = Random.Range(0, 1000000).ToString();

        UserProfileController.i.AddUserProfileToCatalog(new UserProfileModel()
        {
            userId = id1,
            name = "Brian-" + id1
        });

        FriendsController.i.UpdateFriendshipStatus(new FriendsController.FriendshipUpdateStatusMessage()
        {
            userId = id1,
            action = FriendsController.FriendshipAction.REQUESTED_TO
        });
    }
}
