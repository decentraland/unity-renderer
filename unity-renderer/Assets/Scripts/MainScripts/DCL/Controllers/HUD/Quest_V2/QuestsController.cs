using Cysharp.Threading.Tasks;
using DCL.Quests;
using Decentraland.Quests;
using System;
using System.Collections.Generic;
using System.Threading;

public class QuestsController : IDisposable
{
    private readonly IQuestsService questsService;
    private readonly IUserProfileBridge userProfileBridge;
    private readonly IQuestTrackerComponentView questTrackerComponentView;
    private readonly IQuestCompletedComponentView questCompletedComponentView;
    private readonly IQuestOfferComponentView questOfferComponentView;
    private UserProfile ownUserProfile => userProfileBridge.GetOwn();

    private CancellationTokenSource cts;

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
        questsService.OnQuestPopup += ShowQuestOffer;
        //UpdateQuestTracker(questsService.CurrentState);

        questOfferComponentView.OnQuestAccepted += AccceptQuest;
        questOfferComponentView.OnQuestRefused += AbortQuest; //??? abort is like cancel?
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
                questSteps.Add(new QuestStepComponentModel {isCompleted = true, text = task.Description});

            foreach (Task task in step.Value.ToDos)
                questSteps.Add(new QuestStepComponentModel {isCompleted = false, text = task.Description});
        }
        questTrackerComponentView.SetQuestTitle(questStateUpdate.Name);
        questTrackerComponentView.SetQuestSteps(questSteps);
    }

    private void AccceptQuest(string questId)
    {
        ResetCts();
        questsService.StartQuest(questId, cts.Token).Forget();
    }

    private void AbortQuest(string questId)
    {
        ResetCts();
        questsService.AbortQuest(questId, cts.Token).Forget();
    }

    public void Dispose()
    {
        ResetCts();
    }

    private void ResetCts()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();
    }
}
