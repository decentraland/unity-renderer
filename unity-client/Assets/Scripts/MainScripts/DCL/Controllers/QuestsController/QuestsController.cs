using DCL.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL.QuestsController
{
    public delegate void QuestProgressed(string questId);
    public delegate void QuestCompleted(string questId);
    public delegate void SectionCompleted(string questId, string sectionId);
    public delegate void SectionUnlocked(string questId, string sectionId);
    public delegate void TaskProgressed(string questId, string sectionId, string taskId);

    public interface IQuestsController : IDisposable
    {
        event QuestProgressed OnQuestProgressed;
        event QuestCompleted OnQuestCompleted;
        event SectionCompleted OnSectionCompleted;
        event SectionUnlocked OnSectionUnlocked;
        event TaskProgressed OnTaskProgressed;

        void InitializeQuests(List<QuestModel> parsedQuests);
        void UpdateQuestProgress(QuestModel progressedQuest);
        void RemoveQuest(QuestModel quest);
    }

    public class QuestsController : IQuestsController
    {
        private const string PINNED_QUESTS_KEY = "PinnedQuests";

        public static IQuestsController i { get; internal set; }

        public event QuestProgressed OnQuestProgressed;
        public event QuestCompleted OnQuestCompleted;
        public event SectionCompleted OnSectionCompleted;
        public event SectionUnlocked OnSectionUnlocked;
        public event TaskProgressed OnTaskProgressed;

        private static BaseCollection<string> pinnedQuests => DataStore.i.Quests.pinnedQuests;
        private static BaseDictionary<string, QuestModel> quests => DataStore.i.Quests.quests;

        private bool pinnedQuestsIsDirty = false;

        static QuestsController() { i = new QuestsController(); }

        public QuestsController()
        {
            var savedPinnedQuests = PlayerPrefs.GetString(PINNED_QUESTS_KEY, null);
            if (!string.IsNullOrEmpty(savedPinnedQuests))
            {
                pinnedQuests.Set(Utils.ParseJsonArray<string[]>(savedPinnedQuests));
            }
            pinnedQuests.OnAdded += OnPinnedQuestUpdated;
            pinnedQuests.OnRemoved += OnPinnedQuestUpdated;
        }

        /// <summary>
        /// Bulk initialization of quests
        /// </summary>
        /// <param name="parsedQuests"></param>
        public void InitializeQuests(List<QuestModel> parsedQuests)
        {
            var questsToUnpin = parsedQuests.Where(x => !x.canBePinned).Select(x => x.id);
            foreach (string questId in questsToUnpin)
            {
                pinnedQuests.Remove(questId);
            }
            quests.Set(parsedQuests.Select(x => (x.id, x)));
        }

        /// <summary>
        /// Update progress in a quest
        /// </summary>
        /// <param name="progressedQuest"></param>
        public void UpdateQuestProgress(QuestModel progressedQuest)
        {
            if (!progressedQuest.canBePinned)
                pinnedQuests.Remove(progressedQuest.id);

            //Alex: Edge case. Progressed quest was not included in the initialization.
            // We invoke quests events but no sections or QuestCompleted one.
            if (!quests.TryGetValue(progressedQuest.id, out QuestModel oldQuest))
            {
                quests.Add(progressedQuest.id, progressedQuest);
                OnQuestProgressed?.Invoke(progressedQuest.id);

                return;
            }

            quests[progressedQuest.id] = progressedQuest;
            OnQuestProgressed?.Invoke(progressedQuest.id);

            for (int i = 0; i < progressedQuest.sections.Length; i++)
            {
                QuestSection newQuestSection = progressedQuest.sections[i];
                QuestSection nextQuestSection = (i + 1) < progressedQuest.sections.Length ? (progressedQuest.sections[i + 1]) : null;

                //Alex: Edge case. New quest reported contains a section that was previously not contained.
                // if it's completed, we call the SectionCompleted event and unlock the next one
                bool sectionCompleted = !oldQuest.TryGetSection(newQuestSection.id, out QuestSection oldQuestSection);

                sectionCompleted = sectionCompleted || Math.Abs(oldQuestSection.progress - newQuestSection.progress) > Mathf.Epsilon && newQuestSection.progress >= 1;

                if (sectionCompleted)
                {
                    OnSectionCompleted?.Invoke(progressedQuest.id, newQuestSection.id);
                    if (nextQuestSection != null)
                        OnSectionUnlocked?.Invoke(progressedQuest.id, nextQuestSection.id);
                }
            }

            if (!oldQuest.isCompleted && progressedQuest.isCompleted)
                OnQuestCompleted?.Invoke(progressedQuest.id);
        }

        public void RemoveQuest(QuestModel quest) { quests.Remove(quest.id); }

        private void OnPinnedQuestUpdated(string questId) { pinnedQuestsIsDirty = true; }

        private void Update()
        {
            if (pinnedQuestsIsDirty)
            {
                pinnedQuestsIsDirty = false;
                PlayerPrefs.SetString(PINNED_QUESTS_KEY, JsonConvert.SerializeObject(pinnedQuests.Get()));
            }
        }

        public void Dispose()
        {
            pinnedQuests.OnAdded -= OnPinnedQuestUpdated;
            pinnedQuests.OnRemoved -= OnPinnedQuestUpdated;
        }
    }
}