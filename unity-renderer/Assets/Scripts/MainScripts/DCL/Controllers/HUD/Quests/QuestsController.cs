using Cysharp.Threading.Tasks;
using DCLServices.QuestsService;
using Decentraland.Quests;
using System;
using System.Collections.Generic;
using Task = System.Threading.Tasks.Task;

namespace DCL.Quests
{
    public class QuestsController : IDisposable
    {
        private readonly IQuestsService questsService;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly IQuestTrackerComponentView questTrackerComponentView;
        private readonly IQuestCompletedComponentView questCompletedComponentView;
        private readonly IQuestStartedPopupComponentView questStartedComponentView;
        private UserProfile ownUserProfile => userProfileBridge.GetOwn();

        public QuestsController(
            IQuestsService questsService,
            IUserProfileBridge userProfileBridge,
            IQuestTrackerComponentView questTrackerComponentView,
            IQuestCompletedComponentView questCompletedComponentView,
            IQuestStartedPopupComponentView questStartedComponentView)
        {
            this.questsService = questsService;
            this.userProfileBridge = userProfileBridge;
            this.questTrackerComponentView = questTrackerComponentView;
            this.questCompletedComponentView = questCompletedComponentView;
            this.questStartedComponentView = questStartedComponentView;

            questsService.SetUserId(ownUserProfile.userId);
            questsService.OnQuestUpdated += UpdateQuestTracker;
        }

        private void ShowQuestOffer(QuestStateUpdate quest)
        {
            questStartedComponentView.SetQuestName(quest.QuestData.Name);
            questStartedComponentView.SetVisible(true);
        }

        private void UpdateQuestTracker(QuestStateWithData questStateUpdate)
        {
            if (questStateUpdate.QuestState.RequiredSteps.Count == 0 && questStateUpdate.QuestState.StepsLeft == 0)
            {
                questCompletedComponentView.SetTitle(questStateUpdate.Name);
                questCompletedComponentView.SetIsGuest(ownUserProfile.isGuest);
                questCompletedComponentView.SetVisible(true);
            }

            List<QuestStepComponentModel> questSteps = new List<QuestStepComponentModel>();

            foreach (var step in questStateUpdate.QuestState.CurrentSteps)
            {
                foreach (Decentraland.Quests.Task task in step.Value.TasksCompleted)
                    questSteps.Add(new QuestStepComponentModel { isCompleted = true, text = task.Id });

                foreach (Decentraland.Quests.Task task in step.Value.ToDos)
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
