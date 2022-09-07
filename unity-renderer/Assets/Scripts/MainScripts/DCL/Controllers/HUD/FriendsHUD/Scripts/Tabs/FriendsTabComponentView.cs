using System;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Helpers;
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

    [Header("Load More Entries")] [SerializeField]
    internal Button loadMoreEntriesButton;

    [SerializeField] internal GameObject loadMoreEntriesContainer;
    [SerializeField] internal TMP_Text loadMoreEntriesLabel;

    private readonly Dictionary<string, FriendEntryModel> creationQueue =
        new Dictionary<string, FriendEntryModel>();

    private readonly Dictionary<string, PoolableObject> pooleableEntries = new Dictionary<string, PoolableObject>();
    private readonly Dictionary<string, FriendEntry> entries = new Dictionary<string, FriendEntry>();
    private Pool entryPool;
    private int currentAvatarSnapshotIndex;
    private bool isLayoutDirty;
    private Dictionary<string, FriendEntryModel> filteredEntries;
    private IChatController chatController;
    private ILastReadMessagesService lastReadMessagesService;
    private IFriendsController friendsController;
    private ISocialAnalytics socialAnalytics;

    public Dictionary<string, FriendEntry> Entries => entries;
    public int Count => entries.Count + creationQueue.Keys.Count(s => !entries.ContainsKey(s));

    public bool DidDeferredCreationCompleted => creationQueue.Count == 0;

    public event Action<string> OnSearchRequested;

    public event Action<FriendEntryModel> OnWhisper;
    public event Action<string> OnDeleteConfirmation;
    public event Action OnRequireMoreFriends;
    
    public void Initialize(IChatController chatController,
        ILastReadMessagesService lastReadMessagesService,
        IFriendsController friendsController,
        ISocialAnalytics socialAnalytics)
    {
        this.chatController = chatController;
        this.lastReadMessagesService = lastReadMessagesService;
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
        loadMoreEntriesButton.onClick.AddListener(RequestMoreFriendEntries);

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
        loadMoreEntriesButton.onClick.RemoveListener(RequestMoreFriendEntries);
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

    public override void Update()
    {
        base.Update();

        if (isLayoutDirty)
            Utils.ForceRebuildLayoutImmediate((RectTransform) filledStateContainer.transform);
        isLayoutDirty = false;

        SortDirtyLists();
        FetchProfilePicturesForVisibleEntries();
        SetQueuedEntries();
    }

    public void Clear()
    {
        entries.ToList().ForEach(pair => Remove(pair.Key));

        onlineFriendsList.list.Clear();
        offlineFriendsList.list.Clear();

        searchResultsFriendList.list.Clear();
        UpdateEmptyOrFilledState();
        UpdateCounterLabel();
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

        offlineFriendsList.list.Remove(userId);
        onlineFriendsList.list.Remove(userId);

        searchResultsFriendList.list.Remove(userId);

        UpdateEmptyOrFilledState();
        UpdateCounterLabel();
        UpdateLayout();
    }

    public FriendEntry Get(string userId) => entries.ContainsKey(userId) ? entries[userId] : null;

    public void Populate(string userId, FriendEntryModel model)
    {
        if (!entries.ContainsKey(userId))
        {
            if (creationQueue.ContainsKey(userId))
                creationQueue[userId] = model;
            return;
        }

        var entry = entries[userId];
        entry.Populate(model);

        if (filteredEntries?.ContainsKey(userId) ?? false)
        {
            offlineFriendsList.list.Remove(userId);
            onlineFriendsList.list.Remove(userId);
            searchResultsFriendList.list.Add(userId, entry);
            searchResultsFriendList.FlagAsPendingToSort();
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

    public void Set(string userId, FriendEntryModel model)
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

    public void ClearFilter()
    {
        filteredEntries = null;

        if (searchResultsFriendList.list.gameObject.activeSelf)
        {
            foreach (var pair in entries)
            {
                searchResultsFriendList.list.Remove(pair.Key);
                Populate(pair.Key, pair.Value.Model);
            }
        }
        
        searchResultsFriendList.Hide();

        offlineFriendsList.Show();
        onlineFriendsList.Show();
        offlineFriendsList.Sort();
        onlineFriendsList.Sort();
        offlineFriendsList.list.Filter(entry => true);
        onlineFriendsList.list.Filter(entry => true);
    }

    public void Filter(Dictionary<string, FriendEntryModel> search)
    {
        filteredEntries = search;

        offlineFriendsList.Hide();
        onlineFriendsList.Hide();

        if (!searchResultsFriendList.list.gameObject.activeSelf)
        {
            foreach (var pair in entries)
            {
                searchResultsFriendList.list.Add(pair.Key, pair.Value);

                offlineFriendsList.list.Remove(pair.Key);
                onlineFriendsList.list.Remove(pair.Key);
            }
        }

        searchResultsFriendList.Show();
        searchResultsFriendList.Sort();
        searchResultsFriendList.list.Filter(entry => search.ContainsKey(entry.Model.userId));

        UpdateCounterLabel();
        HideMoreFriendsToLoadHint();
        UpdateLayout();
    }

    public void Enqueue(string userId, FriendEntryModel model) => creationQueue[userId] = model;

    public void ShowMoreFriendsToLoadHint(int pendingFriendsCount)
    {
        loadMoreEntriesLabel.SetText(
            $"{pendingFriendsCount} friends hidden. Use the search bar to find them or click below to show more.");
        ShowMoreFriendsToLoadHint();
    }

    public void HideMoreFriendsToLoadHint()
    {
        loadMoreEntriesContainer.SetActive(false);
        UpdateLayout();
    }
    
    private void ShowMoreFriendsToLoadHint()
    {
        loadMoreEntriesContainer.SetActive(true);
        UpdateLayout();
    }

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

    private void UpdateEmptyOrFilledState()
    {
        emptyStateContainer.SetActive(entries.Count == 0);
        filledStateContainer.SetActive(entries.Count > 0);
    }

    private void CreateEntry(string userId)
    {
        entryPool = GetEntryPool();
        var newFriendEntry = entryPool.Get();
        pooleableEntries.Add(userId, newFriendEntry);
        var entry = newFriendEntry.gameObject.GetComponent<FriendEntry>();
        entry.Initialize(chatController, lastReadMessagesService, friendsController, socialAnalytics);
        entries.Add(userId, entry);

        entry.OnMenuToggle -= OnEntryMenuToggle;
        entry.OnMenuToggle += OnEntryMenuToggle;
        entry.OnWhisperClick -= OnEntryWhisperClick;
        entry.OnWhisperClick += OnEntryWhisperClick;
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

    private void RequestMoreFriendEntries() => OnRequireMoreFriends?.Invoke();

    [Serializable]
    private struct FriendListComponents
    {
        public CollapsableSortedFriendEntryList list;
        public TMP_Text countText;
        public GameObject headerContainer;

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