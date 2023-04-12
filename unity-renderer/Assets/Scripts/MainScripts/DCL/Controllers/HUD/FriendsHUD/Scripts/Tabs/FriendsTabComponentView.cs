using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Helpers;
using DCl.Social.Friends;
using DCL.Social.Friends;
using SocialFeaturesAnalytics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendsTabComponentView : BaseComponentView
{
    private const int CREATION_AMOUNT_PER_FRAME = 5;
    private const int AVATAR_SNAPSHOTS_PER_FRAME = 5;
    private const int PRE_INSTANTIATED_ENTRIES = 25;
    private const string FRIEND_ENTRIES_POOL_NAME_PREFIX = "FriendEntriesPool_";
    private const float REQUEST_MORE_ENTRIES_SCROLL_THRESHOLD = 0.005f;
    private const float MIN_TIME_TO_REQUIRE_MORE_ENTRIES = 2f;

    [SerializeField] private GameObject enabledHeader;
    [SerializeField] private GameObject disabledHeader;
    [SerializeField] private GameObject emptyStateContainer;
    [SerializeField] private GameObject filledStateContainer;
    [SerializeField] private FriendEntry entryPrefab;
    [SerializeField] private FriendListComponents onlineFriendsList;
    [SerializeField] private FriendListComponents offlineFriendsList;
    [SerializeField] private FriendListComponents allFriendsList;
    [SerializeField] private FriendListComponents searchResultsFriendList;
    [SerializeField] private SearchBarComponentView searchBar;
    [SerializeField] private UserContextMenu contextMenuPanel;
    [SerializeField] private Model model;
    [SerializeField] private RectTransform viewport;
    [SerializeField] internal ScrollRect scroll;

    [Header("Load More Entries")] [SerializeField]
    internal GameObject loadMoreEntriesContainer;

    [SerializeField] internal TMP_Text loadMoreEntriesLabel;
    [SerializeField] internal GameObject loadMoreEntriesSpinner;

    private readonly Dictionary<string, FriendEntryModel> creationQueue =
        new Dictionary<string, FriendEntryModel>();

    private readonly Dictionary<string, PoolableObject> pooledEntries = new Dictionary<string, PoolableObject>();
    private readonly Dictionary<string, PoolableObject> searchPooledEntries = new Dictionary<string, PoolableObject>();
    private Pool entryPool;
    private bool isLayoutDirty;
    private IChatController chatController;
    private IFriendsController friendsController;
    private ISocialAnalytics socialAnalytics;
    private bool isSearchMode;
    private Vector2 lastScrollPosition = Vector2.one;
    private Coroutine requireMoreEntriesRoutine;
    private float loadMoreEntriesRestrictionTime;

    public int Count => onlineFriendsList.list.Count()
                        + offlineFriendsList.list.Count()
                        + creationQueue.Keys.Count(s =>
                            !onlineFriendsList.list.Contains(s) && !offlineFriendsList.list.Contains(s));

    public event Action<string> OnSearchRequested;

    public event Action<FriendEntryModel> OnWhisper;
    public event Action<string> OnDeleteConfirmation;
    public event Action OnRequireMoreEntries;

    public void Initialize(IChatController chatController,
        IFriendsController friendsController,
        ISocialAnalytics socialAnalytics)
    {
        this.chatController = chatController;
        this.friendsController = friendsController;
        this.socialAnalytics = socialAnalytics;
    }

    public override void OnEnable()
    {
        base.OnEnable();

        searchBar.Configure(new SearchBarComponentModel {placeHolderText = "Search friend"});
        searchBar.OnSearchText += HandleSearchInputChanged;
        contextMenuPanel.OnBlock += HandleFriendBlockRequest;
        contextMenuPanel.OnUnfriend += HandleUnfriendRequest;
        scroll.onValueChanged.AddListener(RequestMoreEntries);

        int SortByAlphabeticalOrder(FriendEntryBase u1, FriendEntryBase u2)
        {
            return string.Compare(u1.Model.userName, u2.Model.userName, StringComparison.InvariantCultureIgnoreCase);
        }

        onlineFriendsList.list.SortingMethod = SortByAlphabeticalOrder;
        offlineFriendsList.list.SortingMethod = SortByAlphabeticalOrder;
        searchResultsFriendList.list.SortingMethod = SortByAlphabeticalOrder;
        UpdateLayout();
        UpdateEmptyOrFilledState();

        //TODO temporary, remove this and allFriendsList gameobjects later
        allFriendsList.list.gameObject.SetActive(false);
        allFriendsList.headerContainer.gameObject.SetActive(false);
    }

    public override void OnDisable()
    {
        base.OnDisable();

        searchBar.OnSearchText -= HandleSearchInputChanged;
        contextMenuPanel.OnBlock -= HandleFriendBlockRequest;
        contextMenuPanel.OnUnfriend -= HandleUnfriendRequest;
        scroll.onValueChanged.RemoveListener(RequestMoreEntries);
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

    public void Update()
    {
        if (isLayoutDirty)
            Utils.ForceRebuildLayoutImmediate((RectTransform) filledStateContainer.transform);
        isLayoutDirty = false;

        SortDirtyLists();
        FetchProfilePicturesForVisibleEntries();
        SetQueuedEntries();
    }

    public void Clear()
    {
        HideMoreFriendsLoadingSpinner();
        loadMoreEntriesRestrictionTime = Time.realtimeSinceStartup;
        scroll.verticalNormalizedPosition = 1f;
        creationQueue.Clear();

        searchResultsFriendList.list.Clear();

        ClearSearchResults();
        ClearOnlineAndOfflineFriends();

        UpdateEmptyOrFilledState();
        UpdateCounterLabel();
    }

    public void Remove(string userId)
    {
        if (creationQueue.ContainsKey(userId))
            creationQueue.Remove(userId);

        if (pooledEntries.TryGetValue(userId, out var pooledObject))
        {
            entryPool.Release(pooledObject);
            pooledEntries.Remove(userId);
        }

        if (searchPooledEntries.TryGetValue(userId, out var searchPooledObject))
        {
            entryPool.Release(searchPooledObject);
            searchPooledEntries.Remove(userId);
        }

        offlineFriendsList.list.Remove(userId);
        onlineFriendsList.list.Remove(userId);
        searchResultsFriendList.list.Remove(userId);

        UpdateEmptyOrFilledState();
        UpdateCounterLabel();
        UpdateLayout();
    }

    public FriendEntry Get(string userId)
    {
        return (onlineFriendsList.list.Get(userId)
                ?? offlineFriendsList.list.Get(userId)
                ?? searchResultsFriendList.list.Get(userId)) as FriendEntry;
    }

    private void Populate(string userId, FriendEntryModel model, FriendEntryBase entry)
    {
        entry.Populate(model);

        if (isSearchMode)
        {
            searchResultsFriendList.list.Remove(userId);
            searchResultsFriendList.list.Add(userId, entry);
            searchResultsFriendList.FlagAsPendingToSort();
            searchResultsFriendList.list.Filter(searchBar.Text);
        }
        else
        {
            if (model.status == PresenceStatus.ONLINE)
            {
                offlineFriendsList.list.Remove(userId);
                onlineFriendsList.list.Add(userId, entry);
                onlineFriendsList.FlagAsPendingToSort();
            }
            else
            {
                onlineFriendsList.list.Remove(userId);
                offlineFriendsList.list.Add(userId, entry);
                offlineFriendsList.FlagAsPendingToSort();
            }
        }

        UpdateLayout();
        UpdateEmptyOrFilledState();
        UpdateCounterLabel();
    }

    private void Set(string userId, FriendEntryModel model)
    {
        if (creationQueue.ContainsKey(userId))
        {
            creationQueue[userId] = model;
            return;
        }

        FriendEntryBase entry;

        if (isSearchMode)
        {
            if (!searchResultsFriendList.list.Contains(userId))
                entry = CreateEntry(userId, searchPooledEntries);
            else
                entry = searchResultsFriendList.list.Get(userId);
        }
        else
        {
            if (!onlineFriendsList.list.Contains(userId)
                && !offlineFriendsList.list.Contains(userId))
                entry = CreateEntry(userId, pooledEntries);
            else
                entry = onlineFriendsList.list.Get(userId) ?? offlineFriendsList.list.Get(userId);
        }

        Populate(userId, model, entry);
    }

    public override void RefreshControl()
    {
        onlineFriendsList.Show();
        offlineFriendsList.Show();

        if (model.isOnlineFriendsExpanded)
            onlineFriendsList.list.Expand();
        else
            onlineFriendsList.list.Collapse();

        if (model.isOfflineFriendsExpanded)
            offlineFriendsList.list.Expand();
        else
            offlineFriendsList.list.Collapse();
    }

    public void DisableSearchMode()
    {
        isSearchMode = false;

        ClearSearchResults();

        searchBar.ClearSearch(false);
        searchResultsFriendList.list.Clear();
        searchResultsFriendList.Hide();
        offlineFriendsList.Show();
        onlineFriendsList.Show();
    }

    public void EnableSearchMode()
    {
        isSearchMode = true;

        ClearSearchResults();

        offlineFriendsList.Hide();
        onlineFriendsList.Hide();
        searchResultsFriendList.Show();

        UpdateCounterLabel();
        HideMoreFriendsToLoadHint();
        UpdateLayout();
    }

    public void Enqueue(string userId, FriendEntryModel model) => creationQueue[userId] = model;

    public void HideMoreFriendsToLoadHint()
    {
        loadMoreEntriesContainer.SetActive(false);
        UpdateLayout();
    }

    public void ShowMoreFriendsToLoadHint(int hiddenCount)
    {
        loadMoreEntriesLabel.text =
            $"{hiddenCount} friends hidden. Use the search bar to find them or scroll down to show more.";
        loadMoreEntriesContainer.SetActive(true);
        UpdateLayout();
    }

    private void ShowMoreFriendsLoadingSpinner() => loadMoreEntriesSpinner.SetActive(true);

    private void HideMoreFriendsLoadingSpinner() => loadMoreEntriesSpinner.SetActive(false);

    private void HandleSearchInputChanged(string search) => OnSearchRequested?.Invoke(search);

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

    private void FetchProfilePicturesForVisibleEntries()
    {
        FetchProfilePicturesForVisibleEntries(onlineFriendsList);
        FetchProfilePicturesForVisibleEntries(offlineFriendsList);
        FetchProfilePicturesForVisibleEntries(searchResultsFriendList);
    }

    private void FetchProfilePicturesForVisibleEntries(FriendListComponents friendListComponents)
    {
        foreach (var entry in friendListComponents.list.Entries.Values
                     .Skip(friendListComponents.currentAvatarSnapshotIndex)
                     .Take(AVATAR_SNAPSHOTS_PER_FRAME))
        {
            if (entry.IsVisible(viewport))
                entry.EnableAvatarSnapshotFetching();
            else
                entry.DisableAvatarSnapshotFetching();
        }

        friendListComponents.currentAvatarSnapshotIndex += AVATAR_SNAPSHOTS_PER_FRAME;

        if (friendListComponents.currentAvatarSnapshotIndex >= friendListComponents.list.Count())
            friendListComponents.currentAvatarSnapshotIndex = 0;
    }

    private void UpdateEmptyOrFilledState()
    {
        var totalFriends = onlineFriendsList.list.Count() + offlineFriendsList.list.Count();
        emptyStateContainer.SetActive(totalFriends == 0);
        filledStateContainer.SetActive(totalFriends > 0);
    }

    private FriendEntry CreateEntry(string userId, Dictionary<string, PoolableObject> poolableObjects)
    {
        entryPool = GetEntryPool();
        var newFriendEntry = entryPool.Get();
        poolableObjects.Add(userId, newFriendEntry);
        var entry = newFriendEntry.gameObject.GetComponent<FriendEntry>();
        entry.Initialize(chatController, friendsController, socialAnalytics);

        entry.OnMenuToggle -= OnEntryMenuToggle;
        entry.OnMenuToggle += OnEntryMenuToggle;
        entry.OnWhisperClick -= OnEntryWhisperClick;
        entry.OnWhisperClick += OnEntryWhisperClick;

        return entry;
    }

    private void OnEntryWhisperClick(FriendEntry friendEntry) => OnWhisper?.Invoke(friendEntry.Model);

    private void OnEntryMenuToggle(FriendEntryBase friendEntry)
    {
        contextMenuPanel.Show(friendEntry.Model.userId);
        friendEntry.Dock(contextMenuPanel);
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

    private void UpdateCounterLabel()
    {
        onlineFriendsList.countText.SetText("ONLINE ({0})", onlineFriendsList.list.Count());
        offlineFriendsList.countText.SetText("OFFLINE ({0})", offlineFriendsList.list.Count());
        searchResultsFriendList.countText.SetText("Results ({0})", searchResultsFriendList.list.Count());
    }

    private void HandleFriendBlockRequest(string userId, bool blockUser)
    {
        var friendEntryToBlock = Get(userId);
        if (friendEntryToBlock == null) return;
        // instantly refresh ui
        friendEntryToBlock.Model.blocked = blockUser;
        Set(userId, friendEntryToBlock.Model);
    }

    private void HandleUnfriendRequest(string userId)
    {
        var entry = Get(userId);
        if (entry == null) return;
        Remove(userId);
        OnDeleteConfirmation?.Invoke(userId);
    }

    private void UpdateLayout() => isLayoutDirty = true;

    private void SortDirtyLists()
    {
        if (offlineFriendsList.IsSortingDirty)
            offlineFriendsList.Sort();
        if (onlineFriendsList.IsSortingDirty)
            onlineFriendsList.Sort();

        if (searchResultsFriendList.IsSortingDirty)
            searchResultsFriendList.Sort();
    }

    private void RequestMoreEntries(Vector2 position)
    {
        if (!loadMoreEntriesContainer.activeInHierarchy ||
            loadMoreEntriesSpinner.activeInHierarchy ||
            Time.realtimeSinceStartup - loadMoreEntriesRestrictionTime < MIN_TIME_TO_REQUIRE_MORE_ENTRIES) return;

        if (position.y < REQUEST_MORE_ENTRIES_SCROLL_THRESHOLD &&
            lastScrollPosition.y >= REQUEST_MORE_ENTRIES_SCROLL_THRESHOLD)
        {
            if (requireMoreEntriesRoutine != null)
                StopCoroutine(requireMoreEntriesRoutine);

            ShowMoreFriendsLoadingSpinner();
            requireMoreEntriesRoutine = StartCoroutine(WaitThenRequireMoreEntries());

            loadMoreEntriesRestrictionTime = Time.realtimeSinceStartup;
        }

        lastScrollPosition = position;
    }

    private void ClearSearchResults()
    {
        foreach (var pooledObj in searchPooledEntries.Values)
            entryPool.Release(pooledObj);
        searchPooledEntries.Clear();
        searchResultsFriendList.list.Clear();
    }

    private void ClearOnlineAndOfflineFriends()
    {
        foreach (var pooledObj in pooledEntries.Values)
            entryPool.Release(pooledObj);

        pooledEntries.Clear();
        onlineFriendsList.list.Clear();
        offlineFriendsList.list.Clear();
    }

    private IEnumerator WaitThenRequireMoreEntries()
    {
        yield return new WaitForSeconds(1f);
        OnRequireMoreEntries?.Invoke();
    }

    [Serializable]
    private class FriendListComponents
    {
        public CollapsableSortedFriendEntryList list;
        public TMP_Text countText;
        public GameObject headerContainer;
        public int currentAvatarSnapshotIndex;

        public bool IsSortingDirty { get; private set; }

        public void Show()
        {
            list.Show();
            headerContainer.SetActive(true);
        }

        public void Hide()
        {
            list.Hide();
            headerContainer.SetActive(false);
        }

        public void FlagAsPendingToSort() => IsSortingDirty = true;

        public void Sort()
        {
            list.Sort();
            IsSortingDirty = false;
        }
    }

    [Serializable]
    private class Model
    {
        public bool isOnlineFriendsExpanded;
        public bool isOfflineFriendsExpanded;
    }
}
