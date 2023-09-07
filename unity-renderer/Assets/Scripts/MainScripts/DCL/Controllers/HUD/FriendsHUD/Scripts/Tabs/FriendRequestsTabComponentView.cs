using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendRequestsTabComponentView : BaseComponentView
{
    private const string FRIEND_ENTRIES_POOL_NAME_PREFIX = "FriendRequestEntriesPool_";
    private const string NOTIFICATIONS_ID = "Friends";
    private const int PRE_INSTANTIATED_ENTRIES = 25;
    private const float NOTIFICATIONS_DURATION = 3;
    private const int AVATAR_SNAPSHOTS_PER_FRAME = 5;
    private const int CREATION_AMOUNT_PER_FRAME = 5;
    private const float REQUEST_MORE_ENTRIES_SCROLL_THRESHOLD = 0.005f;
    private const float MIN_TIME_TO_REQUIRE_MORE_ENTRIES = 0.5f;

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
    [SerializeField] private RectTransform viewport;
    [SerializeField] internal ScrollRect scroll;

    [Header("Notifications")] [SerializeField]
    private Notification requestSentNotification;

    [SerializeField] private Notification friendSearchFailedNotification;
    [SerializeField] private Notification acceptedFriendNotification;
    [SerializeField] private Notification alreadyFriendsNotification;
    [SerializeField] private Model model;

    [Header("Load More Entries")]
    [SerializeField] internal GameObject loadMoreEntriesContainer;
    [SerializeField] internal TMP_Text loadMoreEntriesLabel;
    [SerializeField] internal GameObject loadMoreEntriesSpinner;

    private readonly Dictionary<string, PoolableObject> pooleableEntries = new Dictionary<string, PoolableObject>();
    private readonly Dictionary<string, FriendRequestEntry> entries = new Dictionary<string, FriendRequestEntry>();
    private readonly Dictionary<string, FriendRequestEntryModel> creationQueue =
        new Dictionary<string, FriendRequestEntryModel>();
    private Pool entryPool;
    private string lastRequestSentUserName;
    private int currentAvatarSnapshotIndex;
    private bool isLayoutDirty;
    private Vector2 lastScrollPosition = Vector2.one;
    private Coroutine requireMoreEntriesRoutine;
    private float loadMoreEntriesRestrictionTime;

    public Dictionary<string, FriendRequestEntry> Entries => entries;

    public int Count => Entries.Count + creationQueue.Keys.Count(s => !Entries.ContainsKey(s));

    public int ReceivedCount => receivedRequestsList.Count() +
                                creationQueue.Count(pair => pair.Value.isReceived && !Entries.ContainsKey(pair.Key));
    public int SentCount => sentRequestsList.Count() +
                                creationQueue.Count(pair => !pair.Value.isReceived && !Entries.ContainsKey(pair.Key));

    public event Action<FriendRequestEntryModel> OnCancelConfirmation;
    public event Action<FriendRequestEntryModel> OnRejectConfirmation;
    public event Action<FriendRequestEntryModel> OnFriendRequestApproved;
    public event Action<string> OnFriendRequestSent;
    public event Action<string> OnFriendRequestOpened;
    public event Action OnRequireMoreEntries;

    public override void OnEnable()
    {
        base.OnEnable();
        searchBar.Configure(new SearchBarComponentModel {placeHolderText = "Search a friend you want to add"});
        searchBar.OnSubmit += SendFriendRequest;
        searchBar.OnSearchText += OnSearchInputValueChanged;
        contextMenuPanel.OnBlock += HandleFriendBlockRequest;
        scroll.onValueChanged.AddListener(RequestMoreEntries);
        UpdateLayout();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        searchBar.OnSubmit -= SendFriendRequest;
        searchBar.OnSearchText -= OnSearchInputValueChanged;
        contextMenuPanel.OnBlock -= HandleFriendBlockRequest;
        scroll.onValueChanged.RemoveListener(RequestMoreEntries);
        NotificationsController.i?.DismissAllNotifications(NOTIFICATIONS_ID);
    }

    public void Update()
    {
        if (isLayoutDirty)
            Utils.ForceRebuildLayoutImmediate((RectTransform) filledStateContainer.transform);
        isLayoutDirty = false;

        SetQueuedEntries();
        FetchProfilePicturesForVisibleEntries();
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
            Set(friend.userId, friend.model);

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
        if (creationQueue.ContainsKey(userId))
            creationQueue.Remove(userId);

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
        HideMoreFriendsLoadingSpinner();
        loadMoreEntriesRestrictionTime = Time.realtimeSinceStartup;
        scroll.verticalNormalizedPosition = 1f;
        creationQueue.Clear();
        entries.ToList().ForEach(pair => Remove(pair.Key));
        receivedRequestsList.Clear();
        sentRequestsList.Clear();
        UpdateEmptyOrFilledState();
        UpdateCounterLabel();
    }

    public FriendRequestEntry Get(string userId) => entries.ContainsKey(userId) ? entries[userId] : null;

    public void Enqueue(string userId, FriendRequestEntryModel model) => creationQueue[userId] = model;

    public void Set(string userId, FriendRequestEntryModel model)
    {
        if (creationQueue.ContainsKey(userId))
        {
            creationQueue[userId] = model;
            return;
        }

        if (!entries.ContainsKey(userId))
        {
            CreateEntry(userId);
            UpdateLayout();
        }

        Populate(userId, model);
        UpdateEmptyOrFilledState();
        UpdateCounterLabel();
    }

    public void Populate(string userId, FriendRequestEntryModel model)
    {
        if (!entries.ContainsKey(userId))
        {
            if (creationQueue.ContainsKey(userId))
                creationQueue[userId] = model;
            return;
        }

        var entry = entries[userId];
        entry.Populate(model);

        if (model.isReceived)
            receivedRequestsList.Add(userId, entry);
        else
            sentRequestsList.Add(userId, entry);
    }

    public void ShowUserNotFoundNotification()
    {
        friendSearchFailedNotification.model.timer = NOTIFICATIONS_DURATION;
        friendSearchFailedNotification.model.groupID = NOTIFICATIONS_ID;
        NotificationsController.i?.ShowNotification(friendSearchFailedNotification);
    }

    private void UpdateLayout() => isLayoutDirty = true;

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
        entry.OnOpened -= OpenFriendRequestDetails;
        entry.OnOpened += OpenFriendRequestDetails;
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
        friendUserName = friendUserName.Trim()
            .Replace("\n", "")
            .Replace("\r", "");
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

    public void HideMoreFriendsToLoadHint()
    {
        loadMoreEntriesContainer.SetActive(false);
        UpdateLayout();
    }

    public void ShowMoreEntriesToLoadHint(int hiddenCount)
    {
        loadMoreEntriesLabel.text = $"{hiddenCount} requests hidden. Scroll down to show more.";
        loadMoreEntriesContainer.SetActive(true);
        UpdateLayout();
    }

    private void ShowMoreFriendsLoadingSpinner() => loadMoreEntriesSpinner.SetActive(true);

    private void HideMoreFriendsLoadingSpinner() => loadMoreEntriesSpinner.SetActive(false);

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

    private void OpenFriendRequestDetails(FriendRequestEntry entry) =>
        OnFriendRequestOpened?.Invoke(entry.Model.userId);

    private void OnFriendRequestReceivedAccepted(FriendRequestEntry requestEntry)
    {
        ShowFriendAcceptedNotification(requestEntry);
        Remove(requestEntry.Model.userId);
        OnFriendRequestApproved?.Invoke((FriendRequestEntryModel) requestEntry.Model);
    }

    private void ShowFriendAcceptedNotification(FriendRequestEntry requestEntry)
    {
        acceptedFriendNotification.model.timer = NOTIFICATIONS_DURATION;
        acceptedFriendNotification.model.groupID = NOTIFICATIONS_ID;
        acceptedFriendNotification.model.message = $"You and {requestEntry.Model.userName} are now friends!";
        NotificationsController.i?.ShowNotification(acceptedFriendNotification);
    }

    private void OnEntryRejectButtonPressed(FriendRequestEntry requestEntry)
    {
        Remove(requestEntry.Model.userId);
        OnRejectConfirmation?.Invoke((FriendRequestEntryModel) requestEntry.Model);
    }

    private void OnEntryCancelButtonPressed(FriendRequestEntry requestEntry)
    {
        Remove(requestEntry.Model.userId);
        OnCancelConfirmation?.Invoke((FriendRequestEntryModel) requestEntry.Model);
    }

    private void OnEntryMenuToggle(FriendEntryBase friendEntry)
    {
        friendEntry.Dock(contextMenuPanel);
        contextMenuPanel.Show(friendEntry.Model.userId);
        contextMenuPanel.SetFriendshipContentActive(false);
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
        friendEntryToBlock.Model.blocked = blockUser;
        Set(userId, (FriendRequestEntryModel) friendEntryToBlock.Model);
    }

    private void FetchProfilePicturesForVisibleEntries()
    {
        foreach (var entry in entries.Values.Skip(currentAvatarSnapshotIndex).Take(AVATAR_SNAPSHOTS_PER_FRAME))
        {
            if (entry.IsVisible(viewport))
                entry.EnableAvatarSnapshotFetching();
            else
                entry.DisableAvatarSnapshotFetching();
        }

        currentAvatarSnapshotIndex += AVATAR_SNAPSHOTS_PER_FRAME;

        if (currentAvatarSnapshotIndex >= entries.Count)
            currentAvatarSnapshotIndex = 0;
    }

    private void SetQueuedEntries()
    {
        if (creationQueue.Count == 0) return;

        for (var i = 0; i < CREATION_AMOUNT_PER_FRAME && creationQueue.Count != 0; i++)
        {
            var pair = creationQueue.FirstOrDefault();
            creationQueue.Remove(pair.Key);
            Set(pair.Key, pair.Value);
        }

        HideMoreFriendsLoadingSpinner();
    }

    private void RequestMoreEntries(Vector2 position)
    {
        if (!loadMoreEntriesContainer.activeInHierarchy ||
            loadMoreEntriesSpinner.activeInHierarchy ||
            (Time.realtimeSinceStartup - loadMoreEntriesRestrictionTime) < MIN_TIME_TO_REQUIRE_MORE_ENTRIES) return;

        if (position.y < REQUEST_MORE_ENTRIES_SCROLL_THRESHOLD && lastScrollPosition.y >= REQUEST_MORE_ENTRIES_SCROLL_THRESHOLD)
        {
            if (requireMoreEntriesRoutine != null)
                StopCoroutine(requireMoreEntriesRoutine);

            ShowMoreFriendsLoadingSpinner();
            requireMoreEntriesRoutine = StartCoroutine(WaitThenRequireMoreEntries());

            loadMoreEntriesRestrictionTime = Time.realtimeSinceStartup;
        }

        lastScrollPosition = position;
    }

    private IEnumerator WaitThenRequireMoreEntries()
    {
        yield return new WaitForSeconds(1f);
        OnRequireMoreEntries?.Invoke();
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
        public class SerializableEntryModel : FriendRequestEntryModel
        {
        }

        public UserIdAndEntry[] friends;
        public bool isReceivedRequestsExpanded = true;
        public bool isSentRequestsExpanded = true;
    }
}
