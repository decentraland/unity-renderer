using DCL.Helpers;
using DCL.QuestsController;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Huds.QuestsPanel
{
    public class QuestsPanelHUDController : IHUD
    {
        internal IQuestsPanelHUDView view;
        internal IQuestsController questsController;
        private static BaseDictionary<string, QuestModel> quests => DataStore.i.Quests.quests;

        internal InputAction_Trigger toggleQuestsPanel;

        public void Initialize(IQuestsController newQuestsController)
        {
            questsController = newQuestsController;
            view = CreateView();
            SetViewActive(false);

            toggleQuestsPanel = Resources.Load<InputAction_Trigger>("ToggleQuestsPanelHud");
            toggleQuestsPanel.OnTriggered += OnToggleActionTriggered;

            questsController.OnQuestProgressed += OnQuestProgressed;
            quests.OnAdded += OnQuestAdded;
            quests.OnRemoved += OnQuestRemoved;
            quests.OnSet += OnQuestSet;

            DataStore.i.HUDs.questsPanelVisible.OnChange -= OnQuestPanelVisibleChanged;
            DataStore.i.HUDs.questsPanelVisible.OnChange += OnQuestPanelVisibleChanged;

            OnQuestSet(quests.Get());
        }
        private void OnQuestPanelVisibleChanged(bool current, bool previous)
        {
            SetViewActive(current);
        }

        private void OnToggleActionTriggered(DCLAction_Trigger action)
        {
            SetVisibility(!DataStore.i.HUDs.questsPanelVisible.Get());
        }

        private void OnQuestProgressed(string questId)
        {
            if (!quests.TryGetValue(questId, out QuestModel model) || model.status == QuestLiterals.Status.BLOCKED)
            {
                view.RemoveQuest(questId);
                return;
            }

            view.RequestAddOrUpdateQuest(questId);
        }

        private void OnQuestAdded(string questId, QuestModel questModel)
        {
            if (questModel.status == QuestLiterals.Status.BLOCKED)
                return;
            view.RequestAddOrUpdateQuest(questId);
        }

        private void OnQuestRemoved(string questId, QuestModel questModel)
        {
            view.RemoveQuest(questId);
        }

        private void OnQuestSet(IEnumerable<KeyValuePair<string, QuestModel>> quests)
        {
            view.ClearQuests();
            foreach ((string key, QuestModel value) in quests)
            {
                OnQuestAdded(key, value);
            }
        }

        public void SetVisibility(bool visible)
        {
            if(visible)
                Utils.UnlockCursor();
            else
                Utils.LockCursor();

            DataStore.i.HUDs.questsPanelVisible.Set(visible);
        }

        private void SetViewActive(bool visible)
        {
            view?.SetVisibility(visible);
        }

        internal virtual IQuestsPanelHUDView CreateView() => QuestsPanelHUDView.Create();

        public void Dispose()
        {
            view.Dispose();
            toggleQuestsPanel.OnTriggered -= OnToggleActionTriggered;
            if (questsController != null)
                questsController.OnQuestProgressed -= OnQuestProgressed;
            quests.OnAdded -= OnQuestAdded;
            quests.OnRemoved -= OnQuestRemoved;
            quests.OnSet -= OnQuestSet;
            DataStore.i.HUDs.questsPanelVisible.OnChange -= OnQuestPanelVisibleChanged;
        }
    }
}