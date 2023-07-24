using DCL.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL.Social.Chat
{
    public class ChatMentionSuggestionComponentView : MonoBehaviour
    {
        private const string POOL_NAME_PREFIX = "ChatMentionSuggestions";

        [SerializeField] internal ChatMentionSuggestionEntryComponentView mentionSuggestionPrefab;
        [SerializeField] internal RectTransform entryContainer;
        [SerializeField] internal RectTransform layout;

        private readonly Dictionary<ChatMentionSuggestionEntryComponentView, PoolableObject> pooledObjects = new ();

        public event Action<ChatMentionSuggestionModel> OnEntrySubmit;

        public bool IsVisible => gameObject.activeInHierarchy;

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
                entry.OnSubmitted -= HandleEntrySubmit;
                entry.OnSubmitted += HandleEntrySubmit;
            }

            layout.ForceUpdateLayout();
        }

        public void SelectFirstEntry() =>
            pooledObjects.Keys.FirstOrDefault()?.Select();

        public void SelectNextEntry()
        {
            var selectionFound = false;
            ChatMentionSuggestionEntryComponentView[] views = pooledObjects.Keys.ToArray();

            for (var i = 0; i < views.Length; i++)
            {
                ChatMentionSuggestionEntryComponentView view = views[i];
                if (!view.IsSelected) continue;
                int nextIndex = (i + 1) % views.Length;
                view.Deselect();
                views[nextIndex].Select();
                selectionFound = true;
                break;
            }

            if (!selectionFound)
                SelectFirstEntry();
        }

        public void SelectPreviousEntry()
        {
            var selectionFound = false;
            ChatMentionSuggestionEntryComponentView[] views = pooledObjects.Keys.ToArray();

            for (var i = 0; i < views.Length; i++)
            {
                ChatMentionSuggestionEntryComponentView view = views[i];
                if (!view.IsSelected) continue;
                int nextIndex = i - 1 < 0 ? views.Length - 1 : i - 1;
                view.Deselect();
                views[nextIndex].Select();
                selectionFound = true;
                break;
            }

            if (!selectionFound)
                SelectFirstEntry();
        }

        private void HandleEntrySubmit(ChatMentionSuggestionModel model) =>
            OnEntrySubmit?.Invoke(model);

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

        public void SubmitSelectedEntry()
        {
            ChatMentionSuggestionEntryComponentView view = pooledObjects.Keys.FirstOrDefault(view => view.IsSelected);
            view?.OnSubmit(null);
        }
    }
}
