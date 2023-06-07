using Cysharp.Threading.Tasks;
using DCL.Tasks;
using DCLServices.QuestsService;
using Decentraland.Quests;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCL.Quests
{
    public class QuestsController : IDisposable
    {
        private readonly IQuestsService questsService;
        private readonly IQuestTrackerComponentView questTrackerComponentView;
        private readonly IQuestCompletedComponentView questCompletedComponentView;
        private readonly IQuestStartedPopupComponentView questStartedPopupComponentView;
        private readonly IQuestLogComponentView questLogComponentView;
        private readonly DataStore dataStore;

        private CancellationTokenSource disposeCts = null;
        private BaseVariable<string> pinnedQuestId => dataStore.Quests.pinnedQuest;

        public QuestsController(
            IQuestsService questsService,
            IQuestTrackerComponentView questTrackerComponentView,
            IQuestCompletedComponentView questCompletedComponentView,
            IQuestStartedPopupComponentView questStartedPopupComponentView,
            IQuestLogComponentView questLogComponentView,
            DataStore dataStore)
        {
            this.questsService = questsService;
            this.questTrackerComponentView = questTrackerComponentView;
            this.questCompletedComponentView = questCompletedComponentView;
            this.questStartedPopupComponentView = questStartedPopupComponentView;
            this.questLogComponentView = questLogComponentView;
            disposeCts = new CancellationTokenSource();

            if (questsService != null)
            {
                StartTrackingQuests(disposeCts.Token).Forget();
                StartTrackingStartedQuests(disposeCts.Token).Forget();
            }

            questStartedPopupComponentView.OnOpenQuestLog += () => { dataStore.HUDs.questsPanelVisible.Set(true); };
            dataStore.exploreV2.configureQuestInFullscreenMenu.OnChange += ConfigureQuestLogInFullscreenMenuChanged;
            ConfigureQuestLogInFullscreenMenuChanged(dataStore.exploreV2.configureQuestInFullscreenMenu.Get(), null);
            questLogComponentView.OnPinChange += ChangePinnedQuest;
        }

        private void ChangePinnedQuest(string questId, bool isPinned) =>
            pinnedQuestId.Set(isPinned ? questId : "");

        private void ConfigureQuestLogInFullscreenMenuChanged(Transform current, Transform previous) =>
            questLogComponentView.SetAsFullScreenMenuMode(current);

        private async UniTaskVoid StartTrackingQuests(CancellationToken ct)
        {
            await foreach (var questInstance in questsService.QuestUpdated.WithCancellation(ct))
            {
                AddOrUpdateQuestToLog(questInstance);
                if (questInstance.Id != pinnedQuestId.Get())
                    continue;

                List<QuestStepComponentModel> questSteps = new List<QuestStepComponentModel>();
                foreach (var step in questInstance.State.CurrentSteps)
                {
                    foreach (Task task in step.Value.TasksCompleted)
                        questSteps.Add(new QuestStepComponentModel { isCompleted = true, text = task.Id });

                    foreach (Task task in step.Value.ToDos)
                        questSteps.Add(new QuestStepComponentModel { isCompleted = false, text = task.Id });
                }
                questTrackerComponentView.SetQuestTitle(questInstance.Quest.Name);
                questTrackerComponentView.SetQuestSteps(questSteps);
            }
        }

        private async UniTaskVoid StartTrackingStartedQuests(CancellationToken ct)
        {
            await foreach (var questStateUpdate in questsService.QuestStarted.WithCancellation(ct))
            {
                questStartedPopupComponentView.SetQuestName(questStateUpdate.Quest.Name);
                questStartedPopupComponentView.SetVisible(true);
                AddOrUpdateQuestToLog(questStateUpdate);
            }
        }

        private void AddOrUpdateQuestToLog(QuestInstance questInstance)
        {
            QuestDetailsComponentModel quest = new QuestDetailsComponentModel()
            {
                questName = questInstance.Quest.Name,
                //questCreator = questInstance.Quest.,
                questDescription = questInstance.Quest.Description,
                questId = questInstance.Id,
                isPinned = questInstance.Id == pinnedQuestId.Get(),
                //coordinates = questInstance.Quest.Coordinates,
                questSteps = new List<QuestStepComponentModel>(),
                questRewards = new List<QuestRewardComponentModel>()
            };
            questLogComponentView.AddActiveQuest(quest);
        }

        public void Dispose() =>
            disposeCts?.SafeCancelAndDispose();
    }
}
