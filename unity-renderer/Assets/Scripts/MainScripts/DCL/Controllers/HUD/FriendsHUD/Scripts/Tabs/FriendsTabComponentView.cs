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
    
    [SerializeField] private GameObject emptyStateContainer;
    [SerializeField] private GameObject filledStateContainer;
    [SerializeField] private FriendEntry entryPrefab;
    [SerializeField] private CollapsableSortedFriendEntryList onlineFriendsList;
    [SerializeField] private CollapsableSortedFriendEntryList offlineFriendsList;
    [SerializeField] private TMP_Text onlineFriendsCountText;
    [SerializeField] private TMP_Text offlineFriendsCountText;
    [SerializeField] private Model model;

    private readonly Dictionary<string, FriendEntryBase.Model> creationQueue = new Dictionary<string, FriendEntryBase.Model>();
    private readonly Dictionary<string, PoolableObject> pooleableEntries = new Dictionary<string, PoolableObject>();
    private readonly Dictionary<string, FriendEntryBase> entries = new Dictionary<string, FriendEntryBase>();

    private Pool entryPool;
    private string lastProcessedFriend;
    
    public event Action<FriendEntry> OnWhisper;
    public event Action<string> OnDeleteConfirmation;

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
    }

    public bool Remove(string userId)
    {
        if (!entries.ContainsKey(userId)) return false;

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

        return true;
    }

    public bool Set(string userId, FriendEntryBase.Model model)
    {
        if (creationQueue.ContainsKey(userId))
        {
            creationQueue[userId] = model;
            return false;
        }
        
        if (!entries.ContainsKey(userId))
            CreateEntry(userId);

        var entry = entries[userId];
        entry.Populate(model);
        entry.userId = userId;

        if (model.status == PresenceStatus.ONLINE)
        {
            offlineFriendsList.Remove(userId);
            onlineFriendsList.Add(userId, entry);
            var removedTimestamp = offlineFriendsList.RemoveTimestamp(userId);
            onlineFriendsList.SetTimestamp(removedTimestamp);
        }
        else
        {
            onlineFriendsList.Remove(userId);
            offlineFriendsList.Add(userId, entry);
            var removedTimestamp = onlineFriendsList.RemoveTimestamp(userId);
            offlineFriendsList.SetTimestamp(removedTimestamp);
        }
        
        UpdateEmptyOrFilledState();
        UpdateCounterLabel();

        return true;
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

        // entry.OnMenuToggle += (x) =>
        // {
        //     contextMenuPanel.transform.position = entry.menuPositionReference.position;
        //     contextMenuPanel.Show(userId);
        // };
        //
        // entry.OnWhisperClick += (x) => OnWhisper?.Invoke(x);
        // entry.OnJumpInClick += (x) => this.owner.OnCloseButtonPressed();
        //
        // return true;
    }

    // private void ChatController_OnAddMessage(ChatMessage message)
    // {
    //     if (message.messageType != ChatMessage.Type.PRIVATE)
    //         return;
    //
    //     FriendEntryBase friend = GetEntry(message.sender != UserProfile.GetOwnUserProfile().userId
    //         ? message.sender
    //         : message.recipient);
    //
    //     if (friend == null)
    //         return;
    //
    //     bool reorderFriendEntries = false;
    //
    //     if (friend.userId != lastProcessedFriend)
    //     {
    //         lastProcessedFriend = friend.userId;
    //         reorderFriendEntries = true;
    //     }
    //
    //     FriendsTabViewBase.LastFriendTimestampModel timestampToUpdate = new FriendsTabViewBase.LastFriendTimestampModel
    //     {
    //         userId = friend.userId,
    //         lastMessageTimestamp = message.timestamp
    //     };
    //
    //     // Each time a private message is received (or sent by the player), we sort the online and offline lists by timestamp
    //     if (friend.model.status == PresenceStatus.ONLINE)
    //     {
    //         onlineFriendsList.SetTimestamp(timestampToUpdate, reorderFriendEntries);
    //     }
    //     else
    //     {
    //         offlineFriendsList.SetTimestamp(timestampToUpdate, reorderFriendEntries);
    //     }
    //
    //     lastProcessedFriend = friend.userId;
    // }

    public void CreateOrUpdateEntryDeferred(string userId, FriendEntryBase.Model model)
    {
        creationQueue[userId] = model;
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