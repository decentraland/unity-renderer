using DCL.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL.Huds.QuestsTracker
{
    public interface IQuestsTrackerHUDView
    {
        void UpdateQuest(string questId);
        void RemoveEntry(string questId);
        void PinQuest(string questId);
        void UnpinQuest(string questId);
        void ClearEntries();
        void SetVisibility(bool visibility);
        void Dispose();
    }

    public class QuestsTrackerHUDView : MonoBehaviour, IQuestsTrackerHUDView
    {
        internal static int ENTRIES_PER_FRAME { get; set; } = 5;
        private static BaseDictionary<string, QuestModel> quests => DataStore.i.Quests.quests;
        private static BaseCollection<string> pinnedQuests => DataStore.i.Quests.pinnedQuests;

        [SerializeField] internal RectTransform questsContainer;
        [SerializeField] internal GameObject questPrefab;

        internal readonly Dictionary<string, QuestsTrackerEntry> currentEntries = new Dictionary<string, QuestsTrackerEntry>();
        internal  readonly Dictionary<string, DateTime> lastUpdateTimestamp = new Dictionary<string, DateTime>();
        internal  readonly List<string> questsToBeAdded = new List<string>();
        private bool layoutRebuildRequested;
        private bool isDestroyed = false;

        public static QuestsTrackerHUDView Create()
        {
            QuestsTrackerHUDView view = Instantiate(Resources.Load<GameObject>("QuestsTrackerHUD")).GetComponent<QuestsTrackerHUDView>();

#if UNITY_EDITOR
            view.gameObject.name = "_QuestsTrackerHUDView";
#endif
            return view;
        }

        private void Awake()
        {
            StartCoroutine(AddEntriesRoutine());
            StartCoroutine(RemoveEntriesRoutine());
        }

        public void UpdateQuest(string questId)
        {
            if (questsToBeAdded.Contains(questId))
                return;

            questsToBeAdded.Add(questId);
        }

        internal void AddOrUpdateQuest(string questId, bool isPinned)
        {
            if (!quests.TryGetValue(questId, out QuestModel quest) )
                return;

            if (quest.isCompleted)
            {
                RemoveEntry(questId);
                return;
            }

            if (!currentEntries.TryGetValue(questId, out QuestsTrackerEntry questEntry))
            {
                questEntry = Instantiate(questPrefab, questsContainer).GetComponent<QuestsTrackerEntry>();
                questEntry.OnLayoutRebuildRequested += () => layoutRebuildRequested = true;
                questEntry.Populate(quest);
                currentEntries.Add(quest.id, questEntry);
            }

            RefreshLastUpdateTime(quest.id, isPinned);
            questEntry.transform.SetSiblingIndex(0);

            questEntry.Populate(quest);
            questEntry.SetPinStatus(isPinned);
            layoutRebuildRequested = true;
        }

        public void RemoveEntry(string questId)
        {
            questsToBeAdded.Remove(questId);
            if (!currentEntries.TryGetValue(questId, out QuestsTrackerEntry entry))
                return;

            currentEntries.Remove(questId);
            lastUpdateTimestamp.Remove(questId);
            Destroy(entry.gameObject);
        }

        public void PinQuest(string questId)
        {
            if (currentEntries.TryGetValue(questId, out QuestsTrackerEntry entry))
            {
                entry.SetPinStatus(true);
                RefreshLastUpdateTime(questId, true);
            }
            else
            {
                if (questsToBeAdded.Contains(questId))
                    return;

                questsToBeAdded.Add(questId);
            }
        }

        public void UnpinQuest(string questId)
        {
            if (!currentEntries.TryGetValue(questId, out QuestsTrackerEntry entry))
                return;

            entry.SetPinStatus(false);
            RefreshLastUpdateTime(questId, false);
        }

        private void Update()
        {
            if (layoutRebuildRequested)
            {
                layoutRebuildRequested = false;
                Utils.ForceRebuildLayoutImmediate(questsContainer);
            }
        }

        internal void RefreshLastUpdateTime(string questId, bool isPinned)
        {
            DateTime dateToSet = isPinned ? DateTime.MaxValue : DateTime.Now;

            if (lastUpdateTimestamp.ContainsKey(questId))
                lastUpdateTimestamp[questId] = dateToSet;
            else
                lastUpdateTimestamp.Add(questId, dateToSet);
        }

        public void ClearEntries()
        {
            lastUpdateTimestamp.Clear();
            foreach ((string key, QuestsTrackerEntry value) in currentEntries)
            {
                Destroy(value.gameObject);
            }
            currentEntries.Clear();
        }

        public void SetVisibility(bool visibility) { gameObject.SetActive(visibility); }

        public void Dispose()
        {
            if (!isDestroyed)
                Destroy(gameObject);
        }

        private void OnDestroy() { isDestroyed = true; }

        private IEnumerator AddEntriesRoutine()
        {
            while (true)
            {
                for (int i = 0; i < ENTRIES_PER_FRAME && questsToBeAdded.Count > 0; i++)
                {
                    string questId = questsToBeAdded.First();
                    questsToBeAdded.RemoveAt(0);
                    AddOrUpdateQuest(questId, pinnedQuests.Contains(questId));
                }
                yield return null;
            }
        }

        private IEnumerator RemoveEntriesRoutine()
        {
            while (true)
            {
                var entriesToRemove = lastUpdateTimestamp.Where(x => (DateTime.Now - x.Value) > TimeSpan.FromSeconds(3)).Select(x => x.Key).ToArray();
                foreach (string questId in entriesToRemove)
                {
                    RemoveEntry(questId);
                }
                yield return WaitForSecondsCache.Get(0.25f);
            }
        }
    }
}