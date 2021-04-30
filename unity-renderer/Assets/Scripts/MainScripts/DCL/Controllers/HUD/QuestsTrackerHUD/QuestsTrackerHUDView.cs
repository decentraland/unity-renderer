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
        void UpdateQuest(string questId, bool hasProgressed);
        void RemoveEntry(string questId);
        void PinQuest(string questId);
        void UnpinQuest(string questId);
        void ClearEntries();
        void SetVisibility(bool visibility);
        void AddReward(string questId, QuestReward reward);
        void Dispose();
    }

    public class QuestsTrackerHUDView : MonoBehaviour, IQuestsTrackerHUDView
    {
        internal static int ENTRIES_PER_FRAME { get; set; } = 5;
        private static BaseDictionary<string, QuestModel> quests => DataStore.i.Quests.quests;
        private static BaseCollection<string> pinnedQuests => DataStore.i.Quests.pinnedQuests;

        [SerializeField] internal RectTransform questsContainer;
        [SerializeField] internal GameObject questPrefab;
        [SerializeField] private DynamicScrollSensitivity dynamicScrollSensitivity;
        [SerializeField] internal QuestsNotificationsController notificationsController;

        internal readonly Dictionary<string, QuestsTrackerEntry> currentEntries = new Dictionary<string, QuestsTrackerEntry>();
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

        private void Awake() { StartCoroutine(RemoveEntriesRoutine()); }

        public void UpdateQuest(string questId, bool hasProgressed)
        {
            if(hasProgressed || currentEntries.ContainsKey(questId))
                AddOrUpdateQuest(questId, pinnedQuests.Contains(questId));
        }

        internal void AddOrUpdateQuest(string questId, bool isPinned)
        {
            if (!quests.TryGetValue(questId, out QuestModel quest))
                return;

            if (quest.isCompleted && !quest.justProgressed)
            {
                RemoveEntry(questId);
                return;
            }

            if (!currentEntries.TryGetValue(questId, out QuestsTrackerEntry questEntry))
            {
                questEntry = Instantiate(questPrefab, questsContainer).GetComponent<QuestsTrackerEntry>();
                questEntry.OnLayoutRebuildRequested += () => layoutRebuildRequested = true;
                questEntry.OnQuestCompleted += (x => notificationsController.ShowQuestCompleted(x));
                questEntry.OnRewardObtained += (x => notificationsController.ShowRewardObtained(x));
                currentEntries.Add(quest.id, questEntry);
            }

            questEntry.transform.SetSiblingIndex(0);

            questEntry.Populate(quest);
            questEntry.SetPinStatus(isPinned);
            layoutRebuildRequested = true;
        }

        public void RemoveEntry(string questId)
        {
            if (!currentEntries.TryGetValue(questId, out QuestsTrackerEntry entry))
                return;

            currentEntries.Remove(questId);
            entry.StartDestroy();
        }

        public void PinQuest(string questId)
        {
            if (currentEntries.TryGetValue(questId, out QuestsTrackerEntry entry))
            {
                entry.SetPinStatus(true);
            }
            else
            {
                AddOrUpdateQuest(questId, pinnedQuests.Contains(questId));
            }
        }

        public void UnpinQuest(string questId)
        {
            if (!currentEntries.TryGetValue(questId, out QuestsTrackerEntry entry))
                return;

            entry.SetPinStatus(false);
        }

        private void Update()
        {
            if (layoutRebuildRequested)
            {
                layoutRebuildRequested = false;
                questsContainer.ForceUpdateLayout( false);
                dynamicScrollSensitivity?.RecalculateSensitivity();
            }
        }

        public void ClearEntries()
        {
            foreach ((string key, QuestsTrackerEntry value) in currentEntries)
            {
                Destroy(value.gameObject);
            }
            currentEntries.Clear();
        }

        public void SetVisibility(bool visibility) { gameObject.SetActive(visibility); }
        public void AddReward(string questId, QuestReward reward)
        {
            if (!currentEntries.TryGetValue(questId, out QuestsTrackerEntry entry))
                return;
            entry.AddRewardToGive(reward);
        }

        public void Dispose()
        {
            if (!isDestroyed)
                Destroy(gameObject);
        }

        private void OnDestroy() { isDestroyed = true; }

        private IEnumerator RemoveEntriesRoutine()
        {
            while (true)
            {
                var entriesToRemove = currentEntries.Where(x => x.Value.isReadyForDisposal).Select(x => x.Key).ToArray();
                foreach (string questId in entriesToRemove)
                {
                    RemoveEntry(questId);
                }
                yield return WaitForSecondsCache.Get(0.25f);
            }
        }
    }
}