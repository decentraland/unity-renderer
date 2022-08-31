using System.Collections.Generic;
using System.Text.RegularExpressions;
using DCL;
using DCL.Chat.HUD;
using UIComponents.CollapsableSortedList;
using UnityEngine;

public class CollapsableChannelMemberListComponentView : CollapsableSortedListComponentView<string, ChannelMemberEntry>
{
    private const string POOL_NAME_PREFIX = "ChannelMembersPool_";

    [SerializeField] private ChannelMemberEntry entryPrefab;

    private readonly Dictionary<string, PoolableObject> pooleableEntries = new Dictionary<string, PoolableObject>();
    private Pool entryPool;
    private bool releaseEntriesFromPool = true;

    public void Filter(string search)
    {
        if (!gameObject.activeInHierarchy) return;
        var regex = new Regex(search, RegexOptions.IgnoreCase);
        Filter(entry => regex.IsMatch(entry.Model.userName));
    }

    public void Clear(bool releaseEntriesFromPool)
    {
        // avoids releasing instances from pool just for this clear
        this.releaseEntriesFromPool = releaseEntriesFromPool;
        base.Clear();
        this.releaseEntriesFromPool = true;
        pooleableEntries.Clear();
    }

    public override ChannelMemberEntry Remove(string key)
    {
        if (releaseEntriesFromPool)
        {
            if (pooleableEntries.ContainsKey(key))
                pooleableEntries[key].Release();
            pooleableEntries.Remove(key);
        }

        return base.Remove(key);
    }

    public void Set(string userId, ChannelMemberEntryModel entryModel)
    {
        if (!Contains(entryModel.userId))
            CreateEntry(userId);

        var entry = Get(userId);
        entry.Configure(entryModel);
    }

    private void CreateEntry(string userId)
    {
        entryPool = GetEntryPool();
        var newFriendEntry = entryPool.Get();
        pooleableEntries.Add(userId, newFriendEntry);
        var entry = newFriendEntry.gameObject.GetComponent<ChannelMemberEntry>();
        Add(userId, entry);
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
