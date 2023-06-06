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
        private readonly IUserProfileBridge userProfileBridge;
        private readonly IQuestTrackerComponentView questTrackerComponentView;
        private readonly IQuestCompletedComponentView questCompletedComponentView;
        private readonly IQuestStartedPopupComponentView questStartedPopupComponentView;
        private UserProfile ownUserProfile => userProfileBridge.GetOwn();
        private CancellationTokenSource disposeCts = null;

        public QuestsController(
            IQuestsService questsService,
            IQuestTrackerComponentView questTrackerComponentView,
            IQuestCompletedComponentView questCompletedComponentView,
            IQuestStartedPopupComponentView questStartedPopupComponentView) : this(questsService, null, questTrackerComponentView, questCompletedComponentView, questStartedPopupComponentView)
        { }

        public QuestsController(
            IQuestsService questsService,
            IUserProfileBridge userProfileBridge,
            IQuestTrackerComponentView questTrackerComponentView,
            IQuestCompletedComponentView questCompletedComponentView,
            IQuestStartedPopupComponentView questStartedPopupComponentView)
        {
            this.questsService = questsService;
            this.userProfileBridge = userProfileBridge;
            this.questTrackerComponentView = questTrackerComponentView;
            this.questCompletedComponentView = questCompletedComponentView;
            this.questStartedPopupComponentView = questStartedPopupComponentView;

            if(userProfileBridge != null)
                questsService.SetUserId(ownUserProfile.userId);

            disposeCts = new CancellationTokenSource();
            StartTrackingQuests(disposeCts.Token).Forget();
            StartTrackingStartedQuests(disposeCts.Token).Forget();
        }

        private async UniTaskVoid StartTrackingQuests(CancellationToken ct)
        {
            await foreach (var questStateUpdate in questsService.QuestUpdated.WithCancellation(ct))
            {
                if (questStateUpdate.QuestInstanceId != "pinnedQuestId")
                {
                    AddOrUpdateQuestToLog(questStateUpdate);
                    continue;
                }
                List<QuestStepComponentModel> questSteps = new List<QuestStepComponentModel>();

                foreach (var step in questStateUpdate.QuestState.CurrentSteps)
                {
                    foreach (Task task in step.Value.TasksCompleted)
                        questSteps.Add(new QuestStepComponentModel { isCompleted = true, text = task.Id });

                    foreach (Task task in step.Value.ToDos)
                        questSteps.Add(new QuestStepComponentModel { isCompleted = false, text = task.Id });
                }
                questTrackerComponentView.SetQuestTitle(questStateUpdate.Name);
                questTrackerComponentView.SetQuestSteps(questSteps);
            }
        }

        private async UniTaskVoid StartTrackingStartedQuests(CancellationToken ct)
        {
            await foreach (var questStateUpdate in questsService.QuestStarted.WithCancellation(ct))
            {
                questStartedPopupComponentView.SetQuestName(questStateUpdate.Name);
                questStartedPopupComponentView.SetVisible(true);
                AddOrUpdateQuestToLog(questStateUpdate);
            }
        }

        private void AddOrUpdateQuestToLog(QuestStateWithData questStateWithData)
        {
            //Add or update quest in quest log as soon as merged
        }

        public void Dispose() =>
            disposeCts?.SafeCancelAndDispose();
    }
}
