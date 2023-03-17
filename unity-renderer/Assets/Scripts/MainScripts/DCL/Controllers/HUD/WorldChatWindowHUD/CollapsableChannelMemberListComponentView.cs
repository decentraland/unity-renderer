using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DCL;
using DCL.Chat.HUD;
using DCL.Tasks;
using System;
using System.Threading;
using UIComponents.CollapsableSortedList;
using UnityEngine;

public class CollapsableChannelMemberListComponentView : CollapsableSortedListComponentView<string, ChannelMemberEntry>
{
    private const string POOL_NAME_PREFIX = "ChannelMembersPool_";

    [SerializeField] private ChannelMemberEntry entryPrefab;
    [SerializeField] private UserContextMenu userContextMenu;

    private readonly Dictionary<string, PoolableObject> pooleableEntries = new ();
    private Pool entryPool;
    private CancellationTokenSource preloadProfilesCancellationToken = new ();

    public Func<string, CancellationToken, UniTask<UserProfile>> LoadFullProfileStrategy { private get; set; }

    public override void Dispose()
    {
        base.Dispose();
        preloadProfilesCancellationToken.SafeCancelAndDispose();
    }

    public void Filter(string search)
    {
        if (!gameObject.activeInHierarchy) return;
        var regex = new Regex(search, RegexOptions.IgnoreCase);
        Filter(entry => regex.IsMatch(entry.Model.userName));
    }

    public void Clear()
    {
        base.Clear();
        pooleableEntries.Clear();
    }

    public override ChannelMemberEntry Remove(string key)
    {
        if (pooleableEntries.ContainsKey(key))
            pooleableEntries[key].Release();
        pooleableEntries.Remove(key);

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
        entry.SetUserContextMenu(userContextMenu);
        Add(userId, entry);

        entry.OnProfileOptionsClicked -= HandleProfileOptionsClicked;
        entry.OnProfileOptionsClicked += HandleProfileOptionsClicked;
    }

    private void HandleProfileOptionsClicked(ChannelMemberEntryModel model)
    {
        // Need to preload the full profile since kernel sends a minimal profile for every channel member
        // This allows to see the profile information when inspecting it in the UI
        // It may be a race condition if you press the profile button before the profile is fetched, resulting in an incomplete profile in the passport UI
        // TODO: remove when we have a better way of managing profiles.. should be transparent for the UI
        preloadProfilesCancellationToken = preloadProfilesCancellationToken.SafeRestart();
        LoadFullProfileStrategy?.Invoke(model.userId, preloadProfilesCancellationToken.Token).Forget();
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
