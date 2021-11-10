using System;
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

        public void Initialize(IQuestsController newQuestsController)
        {
            questsController = newQuestsController;
            view = CreateView();
            SetViewActive(false);

            questsController.OnQuestUpdated += OnQuestUpdated;
            quests.OnAdded += OnQuestAdded;
            quests.OnRemoved += OnQuestRemoved;
            quests.OnSet += OnQuestSet;

            DataStore.i.HUDs.questsPanelVisible.OnChange -= OnQuestPanelVisibleChanged;
            DataStore.i.HUDs.questsPanelVisible.OnChange += OnQuestPanelVisibleChanged;

            OnQuestSet(quests.Get());
        }

        private void OnQuestPanelVisibleChanged(bool current, bool previous) { SetViewActive(current); }

        private void OnQuestUpdated(string questId, bool hasProgress)
        {
            if (!quests.TryGetValue(questId, out QuestModel model) || model.status == QuestsLiterals.Status.BLOCKED || (model.visibility == QuestsLiterals.Visibility.SECRET && model.status == QuestsLiterals.Status.NOT_STARTED))
            {
                view.RemoveQuest(questId);
                return;
            }

            view.RequestAddOrUpdateQuest(questId);
        }

        private void OnQuestAdded(string questId, QuestModel questModel)
        {
            if (questModel.status == QuestsLiterals.Status.BLOCKED || (questModel.visibility == QuestsLiterals.Visibility.SECRET && questModel.status == QuestsLiterals.Status.NOT_STARTED))
                return;
            view.RequestAddOrUpdateQuest(questId);
        }

        private void OnQuestRemoved(string questId, QuestModel questModel) { view.RemoveQuest(questId); }

        private void OnQuestSet(IEnumerable<KeyValuePair<string, QuestModel>> quests)
        {
            view.ClearQuests();
            foreach ((string key, QuestModel value) in quests)
            {
                OnQuestAdded(key, value);
            }
        }

        public void SetVisibility(bool visible) { DataStore.i.HUDs.questsPanelVisible.Set(visible); }

        private void SetViewActive(bool visible)
        {
            if ( CommonScriptableObjects.rendererState.Get() )
            {
                if (visible)
                    Utils.UnlockCursor();
                else
                    Utils.LockCursor();
            }
            view?.SetVisibility(visible);
        }

        internal virtual IQuestsPanelHUDView CreateView() => QuestsPanelHUDView.Create();

        public void Dispose()
        {
            view.Dispose();
            if (questsController != null)
                questsController.OnQuestUpdated -= OnQuestUpdated;
            quests.OnAdded -= OnQuestAdded;
            quests.OnRemoved -= OnQuestRemoved;
            quests.OnSet -= OnQuestSet;
            DataStore.i.HUDs.questsPanelVisible.OnChange -= OnQuestPanelVisibleChanged;
        }
    }
}