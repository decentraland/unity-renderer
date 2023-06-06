using Cysharp.Threading.Tasks;
using DCL.Tasks;
using DCLServices.QuestsService;
using Decentraland.Quests;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DCL.Quests
{
    public class QuestsController : IDisposable
    {
        private readonly IQuestsService questsService;
        private readonly IQuestTrackerComponentView questTrackerComponentView;
        private readonly IQuestCompletedComponentView questCompletedComponentView;
        private readonly IQuestStartedPopupComponentView questStartedPopupComponentView;
        private CancellationTokenSource disposeCts = null;

        public QuestsController(
            IQuestsService questsService,
            IQuestTrackerComponentView questTrackerComponentView,
            IQuestCompletedComponentView questCompletedComponentView,
            IQuestStartedPopupComponentView questStartedPopupComponentView)
        {
            this.questsService = questsService;
            this.questTrackerComponentView = questTrackerComponentView;
            this.questCompletedComponentView = questCompletedComponentView;
            this.questStartedPopupComponentView = questStartedPopupComponentView;

            disposeCts = new CancellationTokenSource();
            StartTrackingQuests(disposeCts.Token).Forget();
            StartTrackingStartedQuests(disposeCts.Token).Forget();
        }

        private async UniTaskVoid StartTrackingQuests(CancellationToken ct)
        {
            await foreach (var questInstance in questsService.QuestUpdated.WithCancellation(ct))
            {
                AddOrUpdateQuestToLog(questInstance);
                if (questInstance.Id != "pinnedQuestId")
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
            //Add or update quest in quest log as soon as merged
        }

        public void Dispose() =>
            disposeCts?.SafeCancelAndDispose();
    }
}
