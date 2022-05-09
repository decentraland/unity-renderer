﻿using System;
using DCL.Components;
using DCL.QuestsController;
using System.Collections;
using DCL.Models;
using UnityEngine;

public class QuestTrackingInfo : BaseComponent
{
    private static IQuestsController questsController => QuestsController.i;

    private QuestModel cachedModel;

    public override string componentName => "questTrackingComponent";

    private void Awake() { model = new QuestModel(); }

    new public QuestModel GetModel() { return cachedModel; }

    public override void UpdateFromModel(BaseModel newModel)
    {
        if (newModel == null)
            return;

        base.UpdateFromModel(newModel);
        if (!(newModel is QuestModel quest))
            return;

        if (string.IsNullOrEmpty(quest.id))
            return;

        bool isDifferentQuest = cachedModel == null || quest.id != cachedModel.id;
        if (isDifferentQuest && cachedModel != null)
            questsController.RemoveQuest(cachedModel);

        cachedModel = quest;
        if (cachedModel != null)
            questsController.UpdateQuestProgress(cachedModel);
    }

    public override int GetClassId() { return (int) CLASS_ID_COMPONENT.QUEST_TRACKING_INFORMATION; }

    public override IEnumerator ApplyChanges(BaseModel newJson) { yield break; }

    private void OnDestroy()
    {
        if (cachedModel != null)
            questsController.RemoveQuest(cachedModel);
    }
}