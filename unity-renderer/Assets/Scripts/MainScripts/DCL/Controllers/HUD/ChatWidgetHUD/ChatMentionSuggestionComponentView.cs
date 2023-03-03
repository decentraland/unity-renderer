using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Chat.HUD
{
    public class ChatMentionSuggestionComponentView : MonoBehaviour
    {
        private const string POOL_NAME_PREFIX = "ChatMentionSuggestions";

        [SerializeField] private ChatMentionSuggestionEntryComponentView mentionSuggestionPrefab;
        [SerializeField] private RectTransform entryContainer;

        private readonly Dictionary<ChatMentionSuggestionEntryComponentView, PoolableObject> pooledObjects = new ();

        public event Action<ChatMentionSuggestionModel> OnEntryClicked;

        public void Clear()
        {
            foreach ((ChatMentionSuggestionEntryComponentView view, PoolableObject poolObj) in pooledObjects)
                poolObj.Release();
            pooledObjects.Clear();
        }

        public void Show() =>
            gameObject.SetActive(true);

        public void Hide() =>
            gameObject.SetActive(false);

        public void Set(List<ChatMentionSuggestionModel> suggestions)
        {
            foreach (ChatMentionSuggestionModel suggestion in suggestions)
            {
                ChatMentionSuggestionEntryComponentView entry = GetEntryFromPool();
                entry.transform.SetParent(entryContainer, false);
                entry.SetModel(suggestion);
                entry.Show(true);
                entry.OnClicked -= HandleEntryClick;
                entry.OnClicked += HandleEntryClick;
            }
        }

        private void HandleEntryClick(ChatMentionSuggestionModel model) =>
            OnEntryClicked?.Invoke(model);

        private ChatMentionSuggestionEntryComponentView GetEntryFromPool()
        {
            var pool = GetPool(mentionSuggestionPrefab);
            var pooledObj = pool.Get();
            var entry = pooledObj.gameObject.GetComponent<ChatMentionSuggestionEntryComponentView>();
            pooledObjects[entry] = pooledObj;
            return entry;
        }

        private Pool GetPool(ChatMentionSuggestionEntryComponentView prefab)
        {
            string poolId = POOL_NAME_PREFIX + GetInstanceID();
            Pool entryPool = PoolManager.i.GetPool(poolId);
            if (entryPool != null) return entryPool;

            entryPool = PoolManager.i.AddPool(
                poolId,
                Instantiate(prefab).gameObject,
                maxPrewarmCount: 0,
                isPersistent: true);
            entryPool.ForcePrewarm();

            return entryPool;
        }
    }
}
