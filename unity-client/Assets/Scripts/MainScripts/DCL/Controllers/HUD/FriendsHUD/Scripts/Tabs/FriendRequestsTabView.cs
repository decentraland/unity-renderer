using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendRequestsTabView : FriendsTabViewBase
{
    [SerializeField] internal EntryList receivedRequestsList = new EntryList();
    [SerializeField] internal EntryList sentRequestsList = new EntryList();

    [SerializeField] internal TMP_InputField friendSearchInputField;
    [SerializeField] internal Button addFriendButton;

    [Header("Notifications")] [SerializeField]
    internal Notification requestSentNotification;

    [SerializeField] internal Notification friendSearchFailedNotification;
    [SerializeField] internal Notification acceptedFriendNotification;
    [SerializeField] internal Notification alreadyFriendsNotification;

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

        alreadyFriendsNotification.model.timer = owner.notificationsDuration;
        alreadyFriendsNotification.model.groupID = FriendsHUDView.NOTIFICATIONS_ID;

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

        var entry = GetEntry(userId) as FriendRequestEntry;

        if (entry == null)
            return false;

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

        var entry = entries[userId] as FriendRequestEntry;

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

    void SendFriendRequest(string friendUserName)
    {
        if (string.IsNullOrEmpty(friendUserName)) return;

        friendSearchInputField.placeholder.enabled = true;
        friendSearchInputField.text = string.Empty;

        addFriendButton.gameObject.SetActive(false);

        if (!AlreadyFriends(friendUserName))
        {
            requestSentNotification.model.message = $"Your request to {friendUserName} successfully sent!";
            NotificationsController.i.ShowNotification(requestSentNotification);

            OnFriendRequestSent?.Invoke(friendUserName);
        }
        else
        {
            NotificationsController.i.ShowNotification(alreadyFriendsNotification);
        }
    }

    bool AlreadyFriends(string friendUserName)
    {
        var friendUserProfile = UserProfileController.GetProfileByName(friendUserName);

        return friendUserProfile != null
               && FriendsController.i.friends.ContainsKey(friendUserProfile.userId)
               && FriendsController.i.friends[friendUserProfile.userId].friendshipStatus == FriendshipStatus.FRIEND;
    }

    public void DisplayFriendUserNotFound()
    {
        NotificationsController.i.ShowNotification(friendSearchFailedNotification);
        addFriendButton.interactable = false;
    }

    void OnSearchInputValueChanged(string friendUserName)
    {
        if (!addFriendButton.gameObject.activeSelf)
            addFriendButton.gameObject.SetActive(true);

        if (!addFriendButton.interactable)
            addFriendButton.interactable = true;

        if (!string.IsNullOrEmpty(friendUserName))
            NotificationsController.i.DismissAllNotifications(FriendsHUDView.NOTIFICATIONS_ID);
    }

    void OnFriendRequestReceivedAccepted(FriendRequestEntry requestEntry)
    {
        // Add placeholder friend to avoid affecting UX by roundtrip with kernel
        FriendsController.i.UpdateFriendshipStatus(new FriendsController.FriendshipUpdateStatusMessage()
        {
            userId = requestEntry.userId,
            action = FriendshipAction.APPROVED
        });

        FriendsController.i.UpdateUserStatus(new FriendsController.UserStatus()
        {
            userId = requestEntry.userId,
            presence = PresenceStatus.OFFLINE
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
}