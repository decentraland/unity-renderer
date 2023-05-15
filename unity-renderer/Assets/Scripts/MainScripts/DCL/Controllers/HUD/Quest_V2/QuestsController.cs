using Cysharp.Threading.Tasks;
using DCL.Quests;
using DCLServices.QuestsService;
using Decentraland.Quests;
using System;
using System.Collections.Generic;

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

            questsService.SetUserId(ownUserProfile.userId);
            questsService.OnQuestUpdated += UpdateQuestTracker;
        }

        public QuestsController(
            IQuestsService questsService,
            IQuestTrackerComponentView questTrackerComponentView,
            IQuestCompletedComponentView questCompletedComponentView,
            IQuestOfferComponentView questOfferComponentView)
        {
            this.questsService = questsService;
            this.questTrackerComponentView = questTrackerComponentView;
            this.questCompletedComponentView = questCompletedComponentView;
            this.questOfferComponentView = questOfferComponentView;

            questsService.OnQuestUpdated += UpdateQuestTracker;
        }

        private void ShowQuestOffer(QuestStateUpdate quest)
        {
            questOfferComponentView.SetQuestTitle(quest.Name);
            questOfferComponentView.SetQuestDescription(quest.Description);
            questOfferComponentView.SetIsGuest(ownUserProfile.isGuest);
        }

        private void UpdateQuestTracker(QuestStateUpdate questStateUpdate)
        {
            //if(questStateUpdate.QuestState.StepsLeft == 0 && questStateUpdate.QuestState.CurrentSteps.Values)

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

        private void AccceptQuest(string questId)
        {
            questsService.StartQuest(questId).Forget();
        }

        private void AbortQuest(string questId)
        {
            questsService.AbortQuest(questId).Forget();
        }

        public void Dispose() { }
    }
}
