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
        private readonly IQuestOfferComponentView questOfferComponentView;
        private UserProfile ownUserProfile => userProfileBridge.GetOwn();
        private CancellationTokenSource trackQuestCts = null;

        public QuestsController(
            IQuestsService questsService,
            IQuestTrackerComponentView questTrackerComponentView,
            IQuestCompletedComponentView questCompletedComponentView,
            IQuestOfferComponentView questOfferComponentView) : this(questsService, null, questTrackerComponentView, questCompletedComponentView, questOfferComponentView)
        { }

        public QuestsController(
            IQuestsService questsService,
            IUserProfileBridge userProfileBridge,
            IQuestTrackerComponentView questTrackerComponentView,
            IQuestCompletedComponentView questCompletedComponentView,
            IQuestOfferComponentView questOfferComponentView)
        {
            this.questsService = questsService;
            this.userProfileBridge = userProfileBridge;
            this.questTrackerComponentView = questTrackerComponentView;
            this.questCompletedComponentView = questCompletedComponentView;
            this.questOfferComponentView = questOfferComponentView;

            if(userProfileBridge != null)
                questsService.SetUserId(ownUserProfile.userId);

            //todo subscribe to quest pinned changes in OnQuestPinned
        }

        private void ShowQuestOffer(QuestStateUpdate quest)
        {
            questOfferComponentView.SetQuestTitle(quest.QuestData.Name);
            questOfferComponentView.SetQuestDescription(quest.QuestData.Description);
            questOfferComponentView.SetIsGuest(ownUserProfile.isGuest);
        }

        private void OnQuestPinned(string current, string previous)
        {
            trackQuestCts?.SafeCancelAndDispose();
            trackQuestCts = null;

            if (!string.IsNullOrEmpty(current))
            {
                trackQuestCts = new CancellationTokenSource();
                StartTrackingPinnedQuest(current, trackQuestCts.Token).Forget();
            }
            else
            {
                // todo: Hide tracker logic
            }
        }

        private async UniTaskVoid StartTrackingPinnedQuest(string questInstanceId, CancellationToken ct)
        {
            await foreach (var questStateUpdate in questsService.QuestUpdated.WithCancellation(ct))
            {
                // Ignore updates from other quests
                if(questStateUpdate.QuestInstanceId != questInstanceId)
                    continue;

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

        private void AbortQuest(string questInstanceId)
        {
            questsService.AbortQuest(questInstanceId).Forget();
        }

        public void Dispose()
        {
            trackQuestCts?.SafeCancelAndDispose();
        }
    }
}
