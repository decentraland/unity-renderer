using DCL.Quests;
using Decentraland.Quests;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Action = Decentraland.Quests.Action;

public class QuestsController : IDisposable
{
    private readonly IQuestsService questsService;
    private readonly IQuestTrackerComponentView questTrackerComponentView;
    private readonly IQuestCompletedComponentView questCompletedComponentView;
    private readonly IQuestOfferComponentView questOfferComponentView;

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
        //UpdateQuestTracker(questsService.CurrentState);

        questOfferComponentView.OnQuestAccepted += AccceptQuest;
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
        questsService.StartQuest(questId);
    }

    private void AbortQuest(string questId)
    {
        questsService.AbortQuest(questId);
    }

    public void Dispose()
    {
    }
}
