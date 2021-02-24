using DCL.QuestsController;

namespace DCL.Huds.QuestsNotifications
{
    public class QuestsNotificationsHUDController : IHUD
    {
        internal IQuestsController questsController;
        internal IQuestsNotificationsHUDView view;
        private static BaseDictionary<string, QuestModel> quests => DataStore.i.Quests.quests;

        public void Initialize(IQuestsController newQuestsController)
        {
            view = CreateView();

            questsController = newQuestsController;

            questsController.OnSectionCompleted += OnSectionCompleted;
            questsController.OnSectionUnlocked += OnSectionUnlocked;
            questsController.OnQuestCompleted += OnQuestCompleted;
        }

        private void OnQuestCompleted(string questId)
        {
            if (!quests.TryGetValue(questId, out QuestModel quest) || quest.status == QuestLiterals.Status.BLOCKED)
                return;

            view?.ShowQuestCompleted(quest);
        }

        private void OnSectionCompleted(string questId, string sectionId)
        {
            if (!quests.TryGetValue(questId, out QuestModel quest) || quest.status == QuestLiterals.Status.BLOCKED)
                return;

            if (!quest.TryGetSection(sectionId, out QuestSection section))
                return;

            view?.ShowSectionCompleted(section);
        }

        private void OnSectionUnlocked(string questId, string sectionId)
        {
            if (!quests.TryGetValue(questId, out QuestModel quest) || quest.status == QuestLiterals.Status.BLOCKED)
                return;

            if (!quest.TryGetSection(sectionId, out QuestSection section))
                return;

            view?.ShowSectionUnlocked(section);
        }

        public void SetVisibility(bool visible)
        {
            view?.SetVisibility(visible);
        }

        public void Dispose()
        {
            if (questsController != null)
            {
                view?.Dispose();
                questsController.OnSectionCompleted -= OnSectionCompleted;
                questsController.OnSectionUnlocked -= OnSectionUnlocked;
                questsController.OnQuestCompleted -= OnQuestCompleted;
            }
        }

        internal virtual IQuestsNotificationsHUDView CreateView() => QuestsNotificationsHUDView.Create();
    }
}