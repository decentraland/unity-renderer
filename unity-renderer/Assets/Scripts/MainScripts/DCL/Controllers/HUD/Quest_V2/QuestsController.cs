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

        questOfferComponentView.OnQuestAccepted += AccceptQuest;
    }

    private void UpdateQuestTracker(QuestStateUpdate questStateUpdate)
    {
        List<QuestStepComponentModel> questSteps = new List<QuestStepComponentModel>();
        foreach (var step in questStateUpdate.QuestState.CurrentSteps)
        {
            foreach (Task task in step.Value.TasksCompleted)
                foreach (var action in task.ActionItems)
                    questSteps.Add(new QuestStepComponentModel {isCompleted = true, text = action.Type});

            foreach (Task task in step.Value.ToDos)
                foreach (var action in task.ActionItems)
                    questSteps.Add(new QuestStepComponentModel {isCompleted = false, text = action.Type});
        }
        questTrackerComponentView.SetQuestTitle(questStateUpdate.Name);
        questTrackerComponentView.SetQuestSteps(questSteps);
    }

    private void AccceptQuest(string questId)
    {
        questsService.StartQuest(questId);
    }

    public void Dispose()
    {
    }
}
