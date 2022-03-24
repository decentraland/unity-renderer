using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DCL;
using UIComponents.CollapsableSortedList;
using UnityEngine;

public class CollapsableDirectChatListComponentView : CollapsableSortedListComponentView<string, DirectChatEntry>
{
    private const string POOL_NAME_PREFIX = "DirectChatEntriesPool_";
    
    [SerializeField] private DirectChatEntry entryPrefab;
    [SerializeField] private UserContextMenu userContextMenu;

    private readonly Dictionary<string, PoolableObject> pooleableEntries = new Dictionary<string, PoolableObject>();
    private Pool entryPool;
    private IChatController chatController;

    public event Action<DirectChatEntry> OnOpenChat;

    public void Initialize(IChatController chatController)
    {
        this.chatController = chatController;
    }

    public void Filter(string search)
    {
        var regex = new Regex(search, RegexOptions.IgnoreCase);
        Filter(entry => regex.IsMatch(entry.Model.userName)
            || regex.IsMatch(entry.Model.lastMessage));
    }

    public void Set(string userId, DirectChatEntry.DirectChatEntryModel entryModel)
    {
        if (!Contains(entryModel.userId))
            CreateEntry(userId);
        var entry = Get(userId);
        entry.Set(entryModel);
    }
    
    private void CreateEntry(string userId)
    {
        entryPool = GetEntryPool();
        var newFriendEntry = entryPool.Get();
        pooleableEntries.Add(userId, newFriendEntry);
        var entry = newFriendEntry.gameObject.GetComponent<DirectChatEntry>();
        Add(userId, entry);
        entry.Initialize(chatController, userContextMenu);
        entry.OnOpenChat += () => OnOpenChat?.Invoke(entry);
    }
    
    private Pool GetEntryPool()
    {
        var entryPool = PoolManager.i.GetPool(POOL_NAME_PREFIX + name + GetInstanceID());
        if (entryPool != null) return entryPool;

        entryPool = PoolManager.i.AddPool(
            POOL_NAME_PREFIX + name + GetInstanceID(),
            Instantiate(entryPrefab).gameObject,
            maxPrewarmCount: 20,
            isPersistent: true);
        entryPool.ForcePrewarm();

        return entryPool;
    }
}