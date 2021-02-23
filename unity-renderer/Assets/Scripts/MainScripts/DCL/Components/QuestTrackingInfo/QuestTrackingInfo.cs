using DCL.Components;
using DCL.QuestsController;
using System.Collections;
using UnityEngine;

public class QuestTrackingInfo : BaseComponent
{
    private QuestModel model;
    private static IQuestsController questsController => QuestsController.i;

    public override object GetModel()
    {
        return model;
    }

    public override void SetModel(object newModel)
    {
        if(!(newModel is QuestModel quest))
            return;

        if (model != null)
            questsController.RemoveQuest(model);

        model = quest;
        if(model != null)
            questsController.UpdateQuestProgress(quest);
    }

    public override IEnumerator ApplyChanges(string newJson)
    {
        if (newJson == "{}")
        {
            ApplyChanges(null);
            yield break;
        }
        SetModel(JsonUtility.FromJson<QuestModel>(newJson));
    }

    private void OnDestroy()
    {
        if (model != null)
            questsController.RemoveQuest(model);
    }
}
