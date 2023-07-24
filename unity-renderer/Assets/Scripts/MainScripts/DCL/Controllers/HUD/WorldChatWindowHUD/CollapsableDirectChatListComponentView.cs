using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DCL.Social.Chat;
using UIComponents.CollapsableSortedList;
using UnityEngine;

namespace DCL.Social.Chat
{
    public class CollapsableDirectChatListComponentView : CollapsableSortedListComponentView<string, PrivateChatEntry>
    {
        private const string POOL_NAME_PREFIX = "DirectChatEntriesPool_";

        [SerializeField] private PrivateChatEntry entryPrefab;
        [SerializeField] private UserContextMenu userContextMenu;

        private readonly Dictionary<string, PoolableObject> pooleableEntries = new Dictionary<string, PoolableObject>();
        private Pool entryPool;
        private IChatController chatController;
        private bool releaseEntriesFromPool = true;
        private DataStore_Mentions mentionsDataStore;

        public event Action<PrivateChatEntry> OnOpenChat;
        public event Action<string> OnUnfriend
        {
            add => userContextMenu.OnUnfriend += value;
            remove => userContextMenu.OnUnfriend -= value;
        }

        public void Initialize(
            IChatController chatController,
            DataStore_Mentions mentionsDataStore)
        {
            this.chatController = chatController;
            this.mentionsDataStore = mentionsDataStore;
        }

        public void Filter(string search)
        {
            if (!gameObject.activeInHierarchy) return;
            var regex = new Regex(search, RegexOptions.IgnoreCase);
            Filter(entry => regex.IsMatch(entry.Model.userName));
            // throttling may intruduce race conditions & artifacts into the ui
            // StartCoroutine(FilterAsync(entry => regex.IsMatch(entry.Model.userName)));
        }

        public void Clear(bool releaseEntriesFromPool)
        {
            // avoids releasing instances from pool just for this clear
            this.releaseEntriesFromPool = releaseEntriesFromPool;
            base.Clear();
            this.releaseEntriesFromPool = true;
            pooleableEntries.Clear();
        }

        public override PrivateChatEntry Remove(string key)
        {
            if (releaseEntriesFromPool)
            {
                if (pooleableEntries.ContainsKey(key))
                    pooleableEntries[key].Release();
                pooleableEntries.Remove(key);
            }

            return base.Remove(key);
        }

        public void Set(string userId, PrivateChatEntryModel entryModel)
        {
            if (!Contains(entryModel.userId))
                CreateEntry(userId);

            var entry = Get(userId);
            entry.Configure(entryModel);
        }

        public void RefreshBlockedEntries(List<string> blockedUsers)
        {
            foreach (var pair in Entries)
            {
                pair.Value.SetBlockStatus(blockedUsers.Contains(pair.Key));
            }
        }

        public void RefreshPresence(string userId, bool isOnline)
        {
            if (Entries.TryGetValue(userId, out PrivateChatEntry directMessage))
                directMessage.SetPresence(isOnline);
        }

        private void CreateEntry(string userId)
        {
            entryPool = GetEntryPool();
            var newFriendEntry = entryPool.Get();
            pooleableEntries.Add(userId, newFriendEntry);
            var entry = newFriendEntry.gameObject.GetComponent<PrivateChatEntry>();
            Add(userId, entry);
            entry.Initialize(chatController, userContextMenu, mentionsDataStore);
            entry.OnOpenChat -= OnEntryOpenChat;
            entry.OnOpenChat += OnEntryOpenChat;
        }

        private void OnEntryOpenChat(PrivateChatEntry entry) { OnOpenChat?.Invoke(entry); }

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
