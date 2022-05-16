using System;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Helpers;
using TMPro;
using UnityEngine;

public class FriendRequestsTabComponentView : BaseComponentView
{
    private const string FRIEND_ENTRIES_POOL_NAME_PREFIX = "FriendRequestEntriesPool_";
    private const string NOTIFICATIONS_ID = "Friends";
    private const int PRE_INSTANTIATED_ENTRIES = 25;
    private const float NOTIFICATIONS_DURATION = 3;

    [SerializeField] private GameObject enabledHeader;
    [SerializeField] private GameObject disabledHeader;
    [SerializeField] private GameObject emptyStateContainer;
    [SerializeField] private GameObject filledStateContainer;
    [SerializeField] private FriendRequestEntry entryPrefab;
    [SerializeField] private CollapsableSortedFriendEntryList receivedRequestsList;
    [SerializeField] private CollapsableSortedFriendEntryList sentRequestsList;
    [SerializeField] private SearchBarComponentView searchBar;
    [SerializeField] private TMP_Text receivedRequestsCountText;
    [SerializeField] private TMP_Text sentRequestsCountText;
    [SerializeField] private UserContextMenu contextMenuPanel;

    [Header("Notifications")] [SerializeField]
    private Notification requestSentNotification;

    [SerializeField] private Notification friendSearchFailedNotification;
    [SerializeField] private Notification acceptedFriendNotification;
    [SerializeField] private Notification alreadyFriendsNotification;
    [SerializeField] private Model model;

    private readonly Dictionary<string, PoolableObject> pooleableEntries = new Dictionary<string, PoolableObject>();
    private readonly Dictionary<string, FriendRequestEntry> entries = new Dictionary<string, FriendRequestEntry>();
    private Pool entryPool;
    private string lastRequestSentUserName;

    public Dictionary<string, FriendRequestEntry> Entries => entries;

    public CollapsableSortedFriendEntryList ReceivedRequestsList => receivedRequestsList;

    public event Action<FriendRequestEntry> OnCancelConfirmation;
    public event Action<FriendRequestEntry> OnRejectConfirmation;
    public event Action<FriendRequestEntry> OnFriendRequestApproved;
    public event Action<string> OnFriendRequestSent;

    public override void OnEnable()
    {
        base.OnEnable();
        searchBar.Configure(new SearchBarComponentModel {placeHolderText = "Search a friend you want to add"});
        searchBar.OnSubmit += SendFriendRequest;
        searchBar.OnSearchText += OnSearchInputValueChanged;
        contextMenuPanel.OnBlock += HandleFriendBlockRequest;
        UpdateLayout();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        searchBar.OnSubmit -= SendFriendRequest;
        searchBar.OnSearchText -= OnSearchInputValueChanged;
        contextMenuPanel.OnBlock -= HandleFriendBlockRequest;
        NotificationsController.i?.DismissAllNotifications(NOTIFICATIONS_ID);
    }

    public void Expand()
    {
        receivedRequestsList.Expand();
        sentRequestsList.Expand();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        enabledHeader.SetActive(true);
        disabledHeader.SetActive(false);
        searchBar.ClearSearch();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        enabledHeader.SetActive(false);
        disabledHeader.SetActive(true);
        contextMenuPanel.Hide();
    }

    public override void RefreshControl()
    {
        Clear();

        foreach (var friend in model.friends)
            Set(friend.userId, friend.model, friend.received);

        if (model.isReceivedRequestsExpanded)
            receivedRequestsList.Expand();
        else
            receivedRequestsList.Collapse();

        if (model.isSentRequestsExpanded)
            sentRequestsList.Expand();
        else
            sentRequestsList.Collapse();
    }

    public void Remove(string userId)
    {
        if (!entries.ContainsKey(userId)) return;

        if (pooleableEntries.TryGetValue(userId, out var pooleableObject))
        {
            entryPool.Release(pooleableObject);
            pooleableEntries.Remove(userId);
        }

        entries.Remove(userId);
        receivedRequestsList.Remove(userId);
        sentRequestsList.Remove(userId);

        UpdateEmptyOrFilledState();
        UpdateCounterLabel();
        UpdateLayout();
    }

    public void Clear()
    {
        entries.ToList().ForEach(pair => Remove(pair.Key));
        receivedRequestsList.Clear();
        sentRequestsList.Clear();
        UpdateEmptyOrFilledState();
        UpdateCounterLabel();
    }

    public FriendRequestEntry Get(string userId) => entries.ContainsKey(userId) ? entries[userId] : null;

    public void Populate(string userId, FriendEntryBase.Model model)
    {
        if (!entries.ContainsKey(userId)) return;

        var entry = entries[userId];
        entry.Populate(model);
    }

    public void Set(string userId, FriendEntryBase.Model model, bool isReceived)
    {
        if (!entries.ContainsKey(userId))
        {
            CreateEntry(userId);
            UpdateLayout();
        }

        var entry = entries[userId];
        entry.Populate(model);
        entry.SetReceived(isReceived);

        if (isReceived)
            receivedRequestsList.Add(userId, entry);
        else
            sentRequestsList.Add(userId, entry);

        UpdateEmptyOrFilledState();
        UpdateCounterLabel();
    }

    public void ShowUserNotFoundNotification()
    {
        friendSearchFailedNotification.model.timer = NOTIFICATIONS_DURATION;
        friendSearchFailedNotification.model.groupID = NOTIFICATIONS_ID;
        NotificationsController.i?.ShowNotification(friendSearchFailedNotification);
    }

    private void UpdateLayout() => ((RectTransform) filledStateContainer.transform).ForceUpdateLayout();

    private void CreateEntry(string userId)
    {
        entryPool = GetEntryPool();
        var newFriendEntry = entryPool.Get();
        pooleableEntries.Add(userId, newFriendEntry);
        var entry = newFriendEntry.gameObject.GetComponent<FriendRequestEntry>();
        if (entry == null) return;
        entries.Add(userId, entry);

        entry.OnAccepted -= OnFriendRequestReceivedAccepted;
        entry.OnAccepted += OnFriendRequestReceivedAccepted;
        entry.OnRejected -= OnEntryRejectButtonPressed;
        entry.OnRejected += OnEntryRejectButtonPressed;
        entry.OnCancelled -= OnEntryCancelButtonPressed;
        entry.OnCancelled += OnEntryCancelButtonPressed;
        entry.OnMenuToggle -= OnEntryMenuToggle;
        entry.OnMenuToggle += OnEntryMenuToggle;
    }

    private Pool GetEntryPool()
    {
        var entryPool = PoolManager.i.GetPool(FRIEND_ENTRIES_POOL_NAME_PREFIX + name + GetInstanceID());
        if (entryPool != null) return entryPool;

        entryPool = PoolManager.i.AddPool(
            FRIEND_ENTRIES_POOL_NAME_PREFIX + name + GetInstanceID(),
            Instantiate(entryPrefab).gameObject,
            maxPrewarmCount: PRE_INSTANTIATED_ENTRIES,
            isPersistent: true);
        entryPool.ForcePrewarm();

        return entryPool;
    }

    private void SendFriendRequest(string friendUserName)
    {
        if (string.IsNullOrEmpty(friendUserName)) return;

        searchBar.ClearSearch();
        lastRequestSentUserName = friendUserName;
        OnFriendRequestSent?.Invoke(friendUserName);
    }

    public void ShowAlreadyFriendsNotification()
    {
        alreadyFriendsNotification.model.timer = NOTIFICATIONS_DURATION;
        alreadyFriendsNotification.model.groupID = NOTIFICATIONS_ID;
        NotificationsController.i?.ShowNotification(alreadyFriendsNotification);
    }

    public void ShowRequestSuccessfullySentNotification()
    {
        requestSentNotification.model.timer = NOTIFICATIONS_DURATION;
        requestSentNotification.model.groupID = NOTIFICATIONS_ID;
        requestSentNotification.model.message = $"Your request to {lastRequestSentUserName} successfully sent!";
        NotificationsController.i?.ShowNotification(requestSentNotification);
    }

    private void UpdateEmptyOrFilledState()
    {
        emptyStateContainer.SetActive(entries.Count == 0);
        filledStateContainer.SetActive(entries.Count > 0);
    }

    private void OnSearchInputValueChanged(string friendUserName)
    {
        if (!string.IsNullOrEmpty(friendUserName))
            NotificationsController.i?.DismissAllNotifications(NOTIFICATIONS_ID);
    }

    private void OnFriendRequestReceivedAccepted(FriendRequestEntry requestEntry)
    {
        // Add placeholder friend to avoid affecting UX by roundtrip with kernel
        FriendsController.i?.UpdateFriendshipStatus(new FriendsController.FriendshipUpdateStatusMessage
        {
            userId = requestEntry.model.userId,
            action = FriendshipAction.APPROVED
        });

        ShowFriendAcceptedNotification(requestEntry);
        Remove(requestEntry.model.userId);
        OnFriendRequestApproved?.Invoke(requestEntry);
    }

    private void ShowFriendAcceptedNotification(FriendRequestEntry requestEntry)
    {
        acceptedFriendNotification.model.timer = NOTIFICATIONS_DURATION;
        acceptedFriendNotification.model.groupID = NOTIFICATIONS_ID;
        acceptedFriendNotification.model.message = $"You and {requestEntry.model.userName} are now friends!";
        NotificationsController.i?.ShowNotification(acceptedFriendNotification);
    }

    private void OnEntryRejectButtonPressed(FriendRequestEntry requestEntry)
    {
        Remove(requestEntry.model.userId);
        OnRejectConfirmation?.Invoke(requestEntry);
    }

    private void OnEntryCancelButtonPressed(FriendRequestEntry requestEntry)
    {
        Remove(requestEntry.model.userId);
        OnCancelConfirmation?.Invoke(requestEntry);
    }

    private void OnEntryMenuToggle(FriendEntryBase friendEntry)
    {
        contextMenuPanel.Show(friendEntry.model.userId);
        friendEntry.Dock(contextMenuPanel);
    }

    private void UpdateCounterLabel()
    {
        receivedRequestsCountText.SetText("RECEIVED ({0})", receivedRequestsList.Count());
        sentRequestsCountText.SetText("SENT ({0})", sentRequestsList.Count());
    }

    private void HandleFriendBlockRequest(string userId, bool blockUser)
    {
        var friendEntryToBlock = Get(userId);
        if (friendEntryToBlock == null) return;
        // instantly refresh ui
        friendEntryToBlock.model.blocked = blockUser;
        Set(userId, friendEntryToBlock.model, friendEntryToBlock.isReceived);
    }

    [Serializable]
    private class Model
    {
        [Serializable]
        public struct UserIdAndEntry
        {
            public string userId;
            public bool received;
            public SerializableEntryModel model;
        }

        [Serializable]
        public class SerializableEntryModel : FriendEntryBase.Model
        {
        }

        public UserIdAndEntry[] friends;
        public bool isReceivedRequestsExpanded = true;
        public bool isSentRequestsExpanded = true;
    }
}