using DCL.QuestsController;
using System.Collections.Generic;

namespace DCL.Huds.QuestsTracker
{
    public class QuestsTrackerHUDController : IHUD
    {
        private static BaseDictionary<string, QuestModel> quests =>DataStore.i.Quests.quests;
        private static BaseCollection<string> pinnedQuests => DataStore.i.Quests.pinnedQuests;

        internal IQuestsTrackerHUDView view;
        internal IQuestsController questsController;


        public void Initialize(IQuestsController controller)
        {
            questsController = controller;
            view = CreateView();

            questsController.OnQuestProgressed += OnQuestProgressed;
            pinnedQuests.OnAdded += OnPinnedQuest;
            pinnedQuests.OnRemoved += OnUnpinnedQuest;
            pinnedQuests.OnSet += OnPinnedQuestsSet;
            quests.OnSet += OnQuestsSet;
            quests.OnAdded += OnQuestsAdded;
            quests.OnRemoved += OnQuestRemoved;

            foreach (string questId in pinnedQuests.Get())
            {
                view?.PinQuest(questId);
            }
        }

        private void OnQuestsAdded(string questId, QuestModel quest)
        {
            view?.PinQuest(questId);
        }

        private void OnQuestsSet(IEnumerable<KeyValuePair<string, QuestModel>> pairs)
        {
            OnPinnedQuestsSet(pinnedQuests.Get());
        }

        private void OnPinnedQuestsSet(IEnumerable<string> pinnedQuests)
        {
            view?.ClearEntries();
            foreach (string questId in pinnedQuests)
            {
                view?.PinQuest(questId);
            }
        }

        private void OnQuestProgressed(string questId)
        {
            if (!quests.TryGetValue(questId, out QuestModel model) || model.status == QuestLiterals.Status.BLOCKED)
            {
                view?.RemoveEntry(questId);
                return;
            }

            view?.UpdateQuest(questId);
        }

        private void OnPinnedQuest(string questId)
        {
            view?.PinQuest(questId);
        }

        private void OnUnpinnedQuest(string questId)
        {
            view?.UnpinQuest(questId);
        }

        private void OnQuestRemoved(string questId, QuestModel quest)
        {
            view?.RemoveEntry(questId);
        }

        public void SetVisibility(bool visible)
        {
            view?.SetVisibility(visible);
        }

        public void Dispose()
        {
            questsController.OnQuestProgressed -= OnQuestProgressed;
            pinnedQuests.OnAdded -= OnPinnedQuest;
            pinnedQuests.OnRemoved -= OnUnpinnedQuest;
            pinnedQuests.OnSet -= OnPinnedQuestsSet;
            quests.OnSet -= OnQuestsSet;
            quests.OnRemoved -= OnQuestRemoved;
        }

        internal virtual IQuestsTrackerHUDView CreateView() => QuestsTrackerHUDView.Create();
    }
}