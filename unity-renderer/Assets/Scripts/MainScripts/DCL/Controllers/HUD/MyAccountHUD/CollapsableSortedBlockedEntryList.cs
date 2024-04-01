using System;
using System.Collections.Generic;
using UIComponents.CollapsableSortedList;
using UnityEngine;

namespace DCL.MyAccount
{
    public class CollapsableSortedBlockedEntryList : CollapsableSortedListComponentView<string, BlockedUserEntry>
    {
        private const string POOL_NAME_PREFIX = "BlockedEntryPool_";
        [SerializeField] private BlockedUserEntry entryPrefab;

        private Pool entryPool;
        private readonly Dictionary<string, PoolableObject> pooleableEntries = new Dictionary<string, PoolableObject>();

        public Action<string> OnUnblockUser { get; set; }

        public void Clear()
        {
            base.Clear();
            pooleableEntries.Clear();
        }

        public override BlockedUserEntry Remove(string key)
        {
            if (pooleableEntries.ContainsKey(key))
                pooleableEntries[key].Release();
            pooleableEntries.Remove(key);

            return base.Remove(key);
        }

        public void Set(string userId, BlockedUserEntryModel entryModel)
        {
            if (!Contains(entryModel.userId))
                CreateEntry(userId);

            var entry = Get(userId);
            entry.Configure(entryModel);
            entry.SetButtonAction(OnUnblockUser);
        }

        private void CreateEntry(string userId)
        {
            entryPool = GetEntryPool();
            var newFriendEntry = entryPool.Get();
            pooleableEntries.Add(userId, newFriendEntry);
            var entry = newFriendEntry.gameObject.GetComponent<BlockedUserEntry>();
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
}
