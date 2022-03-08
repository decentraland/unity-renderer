using System;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Interface;
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
    [SerializeField] private CollapsableSortedFriendEntryList onlineFriendsList;
    [SerializeField] private CollapsableSortedFriendEntryList offlineFriendsList;
    [SerializeField] private TMP_Text onlineFriendsCountText;
    [SerializeField] private TMP_Text offlineFriendsCountText;
    [SerializeField] private SearchBarComponentView searchBar;
    [SerializeField] private UserContextMenu contextMenuPanel;
    [SerializeField] private Model model;

    private readonly Dictionary<string, FriendEntryBase.Model> creationQueue = new Dictionary<string, FriendEntryBase.Model>();
    private readonly Dictionary<string, PoolableObject> pooleableEntries = new Dictionary<string, PoolableObject>();
    private readonly Dictionary<string, FriendEntry> entries = new Dictionary<string, FriendEntry>();
    private Pool entryPool;

    public Dictionary<string, FriendEntry> Entries => entries;

    public bool DidDeferredCreationCompleted => creationQueue.Count == 0;
    
    public event Action<FriendEntry> OnWhisper;
    public event Action<string> OnDeleteConfirmation;

    public override void Start()
    {
        base.Start();
        
        if (ChatController.i != null)
            ChatController.i.OnAddMessage += HandleChatMessageAdded;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        
        searchBar.OnSearchText += Filter;
        contextMenuPanel.OnBlock += HandleFriendBlockRequest;
        contextMenuPanel.OnUnfriend += HandleUnfriendRequest;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        
        searchBar.OnSearchText -= Filter;
        contextMenuPanel.OnBlock -= HandleFriendBlockRequest;
        contextMenuPanel.OnUnfriend -= HandleUnfriendRequest;
    }

    public override void Dispose()
    {
        base.Dispose();
        
        if (ChatController.i != null)
            ChatController.i.OnAddMessage -= HandleChatMessageAdded;
    }

    public void Expand()
    {
        onlineFriendsList.Expand();
        offlineFriendsList.Expand();
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
        onlineFriendsList.Clear();
        offlineFriendsList.Clear();
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
        
        offlineFriendsList.Remove(userId);
        onlineFriendsList.Remove(userId);
        offlineFriendsList.RemoveTimestamp(userId);
        onlineFriendsList.RemoveTimestamp(userId);
        
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
        entry.userId = userId;
        
        if (model.status == PresenceStatus.ONLINE)
        {
            offlineFriendsList.Remove(userId);
            onlineFriendsList.Add(userId, entry);
            var timestamp = offlineFriendsList.RemoveTimestamp(userId);
            onlineFriendsList.SetTimestamp(userId, timestamp);
        }
        else
        {
            onlineFriendsList.Remove(userId);
            offlineFriendsList.Add(userId, entry);
            var timestamp = onlineFriendsList.RemoveTimestamp(userId);
            offlineFriendsList.SetTimestamp(userId, timestamp);
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
            onlineFriendsList.Expand();
        else
            onlineFriendsList.Collapse();
        
        if (model.isOfflineFriendsExpanded)
            offlineFriendsList.Expand();
        else
            offlineFriendsList.Collapse();
    }
    
    public void Filter(string search)
    {
        offlineFriendsList.Filter(search);
        onlineFriendsList.Filter(search);
    }
    
    public void Enqueue(string userId, FriendEntryBase.Model model)
    {
        creationQueue[userId] = model;
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
            contextMenuPanel.transform.position = entry.menuPositionReference.position;
            contextMenuPanel.Show(userId);
        };
        
        entry.OnWhisperClick += x => OnWhisper?.Invoke(x);
    }

    private void HandleChatMessageAdded(ChatMessage message)
    {
        if (message.messageType != ChatMessage.Type.PRIVATE)
            return;
    
        FriendEntryBase friend = Get(message.sender != UserProfile.GetOwnUserProfile().userId
            ? message.sender
            : message.recipient);
    
        if (friend == null) return;
    
        // Each time a private message is received (or sent by the player),
        // we sort the online and offline lists by timestamp
        if (friend.model.status == PresenceStatus.ONLINE)
            onlineFriendsList.SetTimestamp(friend.userId, message.timestamp);
        else
            offlineFriendsList.SetTimestamp(friend.userId, message.timestamp);
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
        onlineFriendsCountText.SetText("ONLINE ({0})", onlineFriendsList.Count());
        offlineFriendsCountText.SetText("OFFLINE ({0})", offlineFriendsList.Count());
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