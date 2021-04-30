using DCL.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL.QuestsController
{
    public delegate void NewQuest(string questId);
    public delegate void QuestUpdated(string questId, bool hasProgress);
    public delegate void QuestCompleted(string questId);
    public delegate void RewardObtained(string questId, string rewardId);

    public interface IQuestsController : IDisposable
    {
        event NewQuest OnNewQuest;
        event QuestUpdated OnQuestUpdated;
        event QuestCompleted OnQuestCompleted;
        event RewardObtained OnRewardObtained;

        void InitializeQuests(List<QuestModel> parsedQuests);
        void UpdateQuestProgress(QuestModel progressedQuest);
        void RemoveQuest(QuestModel quest);
    }

    public class QuestsController : IQuestsController
    {
        private const string PINNED_QUESTS_KEY = "PinnedQuests";

        public static IQuestsController i { get; internal set; }

        public event NewQuest OnNewQuest;
        public event QuestUpdated OnQuestUpdated;
        public event QuestCompleted OnQuestCompleted;
        public event RewardObtained OnRewardObtained;

        private static BaseCollection<string> pinnedQuests => DataStore.i.Quests.pinnedQuests;
        private static BaseDictionary<string, QuestModel> quests => DataStore.i.Quests.quests;

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

            parsedQuests.ForEach(RestoreProgressFlags);
            //We ignore quests without sections/tasks
            quests.Set(parsedQuests.Where(x => x.sections != null && x.sections.Length > 0).Select(x => (x.id, x)));
        }

        /// <summary>
        /// Update progress in a quest
        /// </summary>
        /// <param name="progressedQuest"></param>
        public void UpdateQuestProgress(QuestModel progressedQuest)
        {
            if (!progressedQuest.canBePinned)
                pinnedQuests.Remove(progressedQuest.id);

            //Alex: Edge case. Quests has no sections/tasks, we ignore the UpdateQuestProgress and remove the cached one.
            if (progressedQuest.sections == null || progressedQuest.sections.Length == 0)
            {
                quests.Remove(progressedQuest.id);
                return;
            }

            //Alex: Edge case. Progressed quest was not included in the initialization. We dont invoke quests events
            if (!quests.TryGetValue(progressedQuest.id, out QuestModel oldQuest))
            {
                RestoreProgressFlags(progressedQuest);
                quests.Add(progressedQuest.id, progressedQuest);
                if (!progressedQuest.isCompleted)
                    OnNewQuest?.Invoke(progressedQuest.id);
                return;
            }

            quests[progressedQuest.id] = progressedQuest;
            progressedQuest.oldProgress = oldQuest.progress;

            for (int index = 0; index < progressedQuest.sections.Length; index++)
            {
                QuestSection newQuestSection = progressedQuest.sections[index];

                bool oldQuestSectionFound = oldQuest.TryGetSection(newQuestSection.id, out QuestSection oldQuestSection);

                for (int index2 = 0; index2 < newQuestSection.tasks.Length; index2++)
                {
                    QuestTask currentTask = newQuestSection.tasks[index2];
                    if (oldQuestSectionFound)
                    {
                        bool oldTaskFound = oldQuestSection.TryGetTask(currentTask.id, out QuestTask oldTask);
                        currentTask.justProgressed = !oldTaskFound || currentTask.progress != oldTask.progress;
                        currentTask.justUnlocked = !oldTaskFound || (currentTask.status != QuestsLiterals.Status.BLOCKED && oldTask.status == QuestsLiterals.Status.BLOCKED);
                        currentTask.oldProgress = oldTaskFound ? oldTask.progress : 0;
                    }
                    else
                    {
                        currentTask.justProgressed = false;
                        currentTask.justUnlocked = false;
                        currentTask.oldProgress = 0;
                    }
                }
            }


            // If quest is not blocked anymore or being secret has been just started, we call NewQuest event.
            if (!progressedQuest.isCompleted &&
                ((oldQuest.status == QuestsLiterals.Status.BLOCKED && progressedQuest.status != QuestsLiterals.Status.BLOCKED) ||
                 (progressedQuest.visibility == QuestsLiterals.Visibility.SECRET && oldQuest.status == QuestsLiterals.Status.NOT_STARTED && progressedQuest.status != QuestsLiterals.Status.NOT_STARTED )))
                OnNewQuest?.Invoke(progressedQuest.id);

            OnQuestUpdated?.Invoke(progressedQuest.id, HasProgressed(progressedQuest, oldQuest));
            if (!oldQuest.isCompleted && progressedQuest.isCompleted)
                OnQuestCompleted?.Invoke(progressedQuest.id);

            if (progressedQuest.rewards == null)
                progressedQuest.rewards = new QuestReward[0];

            for (int index = 0; index < progressedQuest.rewards.Length; index++)
            {
                QuestReward newReward = progressedQuest.rewards[index];

                //Alex: Edge case. New quest reported contains a reward that was previously not contained.
                // If it's completed, we call the RewardObtained event
                bool oldRewardFound = oldQuest.TryGetReward(newReward.id, out QuestReward oldReward);
                bool rewardObtained = (!oldRewardFound && newReward.status == QuestsLiterals.RewardStatus.OK) || ( newReward.status != oldReward.status && newReward.status == QuestsLiterals.RewardStatus.OK);
                if (rewardObtained)
                {
                    OnRewardObtained?.Invoke(progressedQuest.id, newReward.id);
                }
            }

            RestoreProgressFlags(progressedQuest);
        }

        private void RestoreProgressFlags(QuestModel progressedQuest)
        {
            progressedQuest.oldProgress = progressedQuest.progress;
            for (int index = 0; index < progressedQuest.sections.Length; index++)
            {
                QuestSection section = progressedQuest.sections[index];
                for (var index2 = 0; index2 < section.tasks.Length; index2++)
                {
                    section.tasks[index2].justProgressed = false;
                    section.tasks[index2].justUnlocked = false;
                    section.tasks[index2].oldProgress = section.tasks[index2].progress;
                }
            }
        }

        public void RemoveQuest(QuestModel quest) { quests.Remove(quest.id); }

        private void OnPinnedQuestUpdated(string questId) { PlayerPrefs.SetString(PINNED_QUESTS_KEY, JsonConvert.SerializeObject(pinnedQuests.Get())); }

        public void Dispose()
        {
            pinnedQuests.OnAdded -= OnPinnedQuestUpdated;
            pinnedQuests.OnRemoved -= OnPinnedQuestUpdated;
        }

        private bool HasProgressed(QuestModel newQuest, QuestModel oldQuest)
        {
            if (newQuest.rewards != null)
            {
                foreach (QuestReward newQuestReward in newQuest.rewards)
                {
                    if (!oldQuest.TryGetReward(newQuestReward.id, out var oldReward))
                        continue;

                    if (newQuestReward.status != oldReward.status)
                        return true;
                }
            }

            foreach (QuestSection newQuestSection in newQuest.sections)
            {
                if (!oldQuest.TryGetSection(newQuestSection.id, out var oldSection))
                    continue;

                if (Math.Abs(newQuestSection.progress - oldSection.progress) > Mathf.Epsilon)
                    return true;
            }
            return false;
        }
    }
}