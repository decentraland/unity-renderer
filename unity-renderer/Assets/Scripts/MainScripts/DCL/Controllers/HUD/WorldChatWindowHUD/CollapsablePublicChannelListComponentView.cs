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

    public event Action<PublicChannelEntry> OnOpenChat;

    public void Filter(string search)
    {
        var regex = new Regex(search, RegexOptions.IgnoreCase);
        Filter(entry => regex.IsMatch(entry.Model.name));
    }

    public override PublicChannelEntry Remove(string key)
    {
        if (pooleableEntries.ContainsKey(key))
            pooleableEntries[key].Release();
        pooleableEntries.Remove(key);
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