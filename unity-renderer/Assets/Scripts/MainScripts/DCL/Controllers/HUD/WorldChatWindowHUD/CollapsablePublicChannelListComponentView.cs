using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DCL;
using UIComponents.CollapsableSortedList;
using UnityEngine;

public class CollapsablePublicChannelListComponentView : CollapsableSortedListComponentView<string, PublicChannelEntry>
{
    private const string POOL_NAME_PREFIX = "PublicChannelEntriesPool_";
    
    [SerializeField] private PublicChannelEntry entryPrefab;

    private readonly Dictionary<string, PoolableObject> pooleableEntries = new Dictionary<string, PoolableObject>();
    private Pool entryPool;
    private bool releaseEntriesFromPool = true;
    private IChatController chatController;
    private ILastReadMessagesService lastReadMessagesService;

    public event Action<PublicChannelEntry> OnOpenChat;
    
    public void Initialize(IChatController chatController, ILastReadMessagesService lastReadMessagesService)
    {
        this.chatController = chatController;
        this.lastReadMessagesService = lastReadMessagesService;
    }

    public void Filter(string search)
    {
        var regex = new Regex(search, RegexOptions.IgnoreCase);
        Filter(entry => regex.IsMatch(entry.Model.name));
    }

    public void Clear(bool releaseEntriesFromPool)
    {
        // avoids releasing instances from pool just for this clear
        this.releaseEntriesFromPool = releaseEntriesFromPool;
        base.Clear();
        this.releaseEntriesFromPool = true;
    }

    public override PublicChannelEntry Remove(string key)
    {
        if (releaseEntriesFromPool)
        {
            if (pooleableEntries.ContainsKey(key))
                pooleableEntries[key].Release();
            pooleableEntries.Remove(key);    
        }
        
        return base.Remove(key);
    }

    public void Set(string channelId, PublicChannelEntry.PublicChannelEntryModel entryModel)
    {
        if (!Contains(entryModel.channelId))
            CreateEntry(channelId);
        var entry = Get(channelId);
        entry.Set(entryModel);
    }
    
    private void CreateEntry(string channelId)
    {
        entryPool = GetEntryPool();
        var newFriendEntry = entryPool.Get();
        pooleableEntries.Add(channelId, newFriendEntry);
        var entry = newFriendEntry.gameObject.GetComponent<PublicChannelEntry>();
        Add(channelId, entry);
        entry.Initialize(chatController, lastReadMessagesService);
        entry.OnOpenChat -= OnEntryOpenChat;
        entry.OnOpenChat += OnEntryOpenChat;
    }

    private void OnEntryOpenChat(PublicChannelEntry entry) { OnOpenChat?.Invoke(entry); }

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