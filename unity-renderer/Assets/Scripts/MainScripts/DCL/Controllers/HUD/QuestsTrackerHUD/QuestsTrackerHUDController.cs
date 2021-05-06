using DCL.QuestsController;
using System.Collections.Generic;

namespace DCL.Huds.QuestsTracker
{
    public class QuestsTrackerHUDController : IHUD
    {
        private static BaseDictionary<string, QuestModel> quests => DataStore.i.Quests.quests;
        private static BaseCollection<string> pinnedQuests => DataStore.i.Quests.pinnedQuests;

        internal IQuestsTrackerHUDView view;
        internal IQuestsController questsController;

        public void Initialize(IQuestsController controller)
        {
            questsController = controller;
            view = CreateView();

            questsController.OnQuestUpdated += OnQuestUpdated;
            questsController.OnRewardObtained += AddReward;
            pinnedQuests.OnAdded += OnPinnedQuest;
            pinnedQuests.OnRemoved += OnUnpinnedQuest;
            pinnedQuests.OnSet += OnPinnedQuestsSet;
            quests.OnSet += OnQuestsSet;
            quests.OnAdded += OnQuestsAdded;
            quests.OnRemoved += OnQuestRemoved;

            foreach (string questId in pinnedQuests.Get())
            {
                OnPinnedQuest(questId);
            }
        }

        private void AddReward(string questId, string rewardId)
        {
            if (!quests.TryGetValue(questId, out QuestModel model) || model.status == QuestsLiterals.Status.BLOCKED)
                return;

            if (!model.TryGetReward(rewardId, out QuestReward reward))
                return;
            view?.AddReward(questId, reward);
        }

        private void OnQuestsAdded(string questId, QuestModel quest)
        {
            if (pinnedQuests.Contains(questId))
                view?.PinQuest(questId);
        }

        private void OnQuestsSet(IEnumerable<KeyValuePair<string, QuestModel>> pairs) { OnPinnedQuestsSet(pinnedQuests.Get()); }

        private void OnPinnedQuestsSet(IEnumerable<string> pinnedQuests)
        {
            view?.ClearEntries();
            foreach (string questId in pinnedQuests)
            {
                OnPinnedQuest(questId);
            }
        }

        private void OnQuestUpdated(string questId, bool hasProgress)
        {
            if (!quests.TryGetValue(questId, out QuestModel model) || model.status == QuestsLiterals.Status.BLOCKED || (model.visibility == QuestsLiterals.Visibility.SECRET && model.status == QuestsLiterals.Status.NOT_STARTED))
            {
                view?.RemoveEntry(questId);
                return;
            }

            view?.UpdateQuest(questId, hasProgress);
        }

        private void OnPinnedQuest(string questId)
        {
            if (!quests.TryGetValue(questId, out QuestModel model) || model.status == QuestsLiterals.Status.BLOCKED || (model.visibility == QuestsLiterals.Visibility.SECRET && model.status == QuestsLiterals.Status.NOT_STARTED))
            {
                view?.RemoveEntry(questId);
                return;
            }
            view?.PinQuest(questId);
        }

        private void OnUnpinnedQuest(string questId) { view?.UnpinQuest(questId); }

        private void OnQuestRemoved(string questId, QuestModel quest) { view?.RemoveEntry(questId); }

        public void SetVisibility(bool visible) { view?.SetVisibility(visible); }

        public void Dispose()
        {
            view.Dispose();
            if (questsController != null)
            {
                questsController.OnQuestUpdated -= OnQuestUpdated;
                questsController.OnRewardObtained -= AddReward;
            }
            pinnedQuests.OnAdded -= OnPinnedQuest;
            pinnedQuests.OnRemoved -= OnUnpinnedQuest;
            pinnedQuests.OnSet -= OnPinnedQuestsSet;
            quests.OnSet -= OnQuestsSet;
            quests.OnRemoved -= OnQuestRemoved;
        }

        internal virtual IQuestsTrackerHUDView CreateView() => QuestsTrackerHUDView.Create();
    }
}