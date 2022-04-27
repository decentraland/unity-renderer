using System;
using System.Collections.Generic;
using System.Linq;
using DCL;
using TMPro;
using UnityEngine;

public class FriendsTabComponentView : BaseComponentView
{
    private const int CREATION_AMOUNT_PER_FRAME = 5;
    private const int PRE_INSTANTIATED_ENTRIES = 25;
    private const string FRIEND_ENTRIES_POOL_NAME_PREFIX = "FriendEntriesPool_";

    [SerializeField] private GameObject enabledHeader;
    [SerializeField] private GameObject disabledHeader;
    [SerializeField] private GameObject emptyStateContainer;
    [SerializeField] private GameObject filledStateContainer;
    [SerializeField] private FriendEntry entryPrefab;
    [SerializeField] private FriendListComponents onlineFriendsList;
    [SerializeField] private FriendListComponents offlineFriendsList;
    [SerializeField] private FriendListComponents searchResultsFriendList;
    [SerializeField] private SearchBarComponentView searchBar;
    [SerializeField] private UserContextMenu contextMenuPanel;
    [SerializeField] private Model model;

    private readonly Dictionary<string, FriendEntryBase.Model> creationQueue =
        new Dictionary<string, FriendEntryBase.Model>();

    private readonly Dictionary<string, PoolableObject> pooleableEntries = new Dictionary<string, PoolableObject>();
    private readonly Dictionary<string, FriendEntry> entries = new Dictionary<string, FriendEntry>();
    private readonly Dictionary<string, ulong> timestamps = new Dictionary<string, ulong>();
    private Pool entryPool;
    private string lastSearch;

    public Dictionary<string, FriendEntry> Entries => entries;

    public bool DidDeferredCreationCompleted => creationQueue.Count == 0;

    public event Action<FriendEntry> OnWhisper;
    public event Action<string> OnDeleteConfirmation;

    public override void OnEnable()
    {
        base.OnEnable();

        searchBar.Configure(new SearchBarComponentModel {placeHolderText = "Search friend"});
        searchBar.OnSearchText += Filter;
        contextMenuPanel.OnBlock += HandleFriendBlockRequest;
        contextMenuPanel.OnUnfriend += HandleUnfriendRequest;

        int SortByTimestamp(FriendEntryBase u1, FriendEntryBase u2)
        {
            var t1 = timestamps.ContainsKey(u1.model.userId) ? timestamps[u1.model.userId] : 0;
            var t2 = timestamps.ContainsKey(u2.model.userId) ? timestamps[u2.model.userId] : 0;
            return t2.CompareTo(t1);
        }

        int SortByAlphabeticalOrder(FriendEntryBase u1, FriendEntryBase u2)
        {
            return string.Compare(u2.model.userName, u1.model.userName, StringComparison.Ordinal);
        }

        onlineFriendsList.list.SortingMethod = SortByTimestamp;
        offlineFriendsList.list.SortingMethod = SortByTimestamp;
        searchResultsFriendList.list.SortingMethod = SortByAlphabeticalOrder;
    }

    public override void OnDisable()
    {
        base.OnDisable();

        searchBar.OnSearchText -= Filter;
        contextMenuPanel.OnBlock -= HandleFriendBlockRequest;
        contextMenuPanel.OnUnfriend -= HandleUnfriendRequest;
    }

    public void Expand()
    {
        onlineFriendsList.list.Expand();
        offlineFriendsList.list.Expand();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        enabledHeader.SetActive(true);
        disabledHeader.SetActive(false);
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

        if (creationQueue.Count == 0) return;

        for (var i = 0; i < CREATION_AMOUNT_PER_FRAME && creationQueue.Count != 0; i++)
        {
            var pair = creationQueue.FirstOrDefault();
            creationQueue.Remove(pair.Key);
            Set(pair.Key, pair.Value);
        }
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
        timestamps.Remove(userId);

        UpdateEmptyOrFilledState();
        UpdateCounterLabel();
    }

    public FriendEntry Get(string userId) => entries.ContainsKey(userId) ? entries[userId] : null;

    public void Populate(string userId, FriendEntryBase.Model model)
    {
        if (!entries.ContainsKey(userId))
        {
            if (creationQueue.ContainsKey(userId))
                creationQueue[userId] = model;
            return;
        }

        var entry = entries[userId];
        entry.Populate(model);

        if (model.status == PresenceStatus.ONLINE)
        {
            offlineFriendsList.list.Remove(userId);
            onlineFriendsList.list.Add(userId, entry);
        }
        else
        {
            onlineFriendsList.list.Remove(userId);
            offlineFriendsList.list.Add(userId, entry);
        }

        UpdateEmptyOrFilledState();
        UpdateCounterLabel();
    }

    public void Set(string userId, FriendEntryBase.Model model)
    {
        if (creationQueue.ContainsKey(userId))
        {
            creationQueue[userId] = model;
            return;
        }

        if (!entries.ContainsKey(userId))
            CreateEntry(userId);

        Populate(userId, model);
    }

    public override void RefreshControl()
    {
        Clear();

        foreach (var friend in model.friends)
            Set(friend.userId, friend.model);

        if (model.isOnlineFriendsExpanded)
            onlineFriendsList.list.Expand();
        else
            onlineFriendsList.list.Collapse();

        if (model.isOfflineFriendsExpanded)
            offlineFriendsList.list.Expand();
        else
            offlineFriendsList.list.Collapse();
    }

    public void Filter(string search)
    {
        if (string.IsNullOrEmpty(search) && !string.IsNullOrEmpty(lastSearch))
        {
            searchResultsFriendList.Hide();

            foreach (var pair in entries)
            {
                searchResultsFriendList.list.Remove(pair.Key);
                Populate(pair.Key, pair.Value.model);
            }

            offlineFriendsList.Show();
            onlineFriendsList.Show();
        }

        if (!string.IsNullOrEmpty(search) && string.IsNullOrEmpty(lastSearch))
        {
            offlineFriendsList.Hide();
            onlineFriendsList.Hide();

            foreach (var pair in entries)
            {
                searchResultsFriendList.list.Add(pair.Key, pair.Value);
                offlineFriendsList.list.Remove(pair.Key);
                onlineFriendsList.list.Remove(pair.Key);
            }

            searchResultsFriendList.list.Sort();
            searchResultsFriendList.Show();
        }

        searchResultsFriendList.list.Filter(search);
        offlineFriendsList.list.Filter(search);
        onlineFriendsList.list.Filter(search);
        lastSearch = search;
        UpdateCounterLabel();
    }

    public void Enqueue(string userId, FriendEntryBase.Model model)
    {
        creationQueue[userId] = model;
    }

    public void SortEntriesByTimestamp(FriendEntryBase.Model user, ulong timestamp)
    {
        timestamps[user.userId] = timestamp;

        if (user.status == PresenceStatus.ONLINE)
            onlineFriendsList.list.Sort();
        else
            offlineFriendsList.list.Sort();
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
        entries.Add(userId, entry);

        entry.OnMenuToggle += x =>
        {
            contextMenuPanel.Show(userId);
            entry.Dock(contextMenuPanel);
        };

        entry.OnWhisperClick += x => OnWhisper?.Invoke(x);
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
        friendEntryToBlock.model.blocked = blockUser;
        Set(userId, friendEntryToBlock.model);
    }

    private void HandleUnfriendRequest(string userId)
    {
        var entry = Get(userId);
        if (entry == null) return;
        Remove(userId);
        OnDeleteConfirmation?.Invoke(userId);
    }

    [Serializable]
    private struct FriendListComponents
    {
        public CollapsableSortedFriendEntryList list;
        public TMP_Text countText;
        public GameObject headerContainer;

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
    }

    [Serializable]
    private class Model
    {
        [Serializable]
        public struct UserIdAndEntry
        {
            public string userId;
            public SerializableEntryModel model;
        }

        [Serializable]
        public class SerializableEntryModel : FriendEntryBase.Model
        {
        }

        public UserIdAndEntry[] friends;
        public bool isOnlineFriendsExpanded = true;
        public bool isOfflineFriendsExpanded = true;
    }
}