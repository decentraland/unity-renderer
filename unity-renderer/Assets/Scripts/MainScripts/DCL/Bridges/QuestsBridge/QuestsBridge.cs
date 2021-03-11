using DCL.QuestsController;
using System.Collections.Generic;
using UnityEngine;
using DCL.Helpers;

public class QuestsBridge : MonoBehaviour
{
    public void InitializeQuests(string jsonMessage)
    {
        var parsedQuests = Utils.ParseJsonArray<List<QuestModel>>(jsonMessage);
        QuestsController.i.InitializeQuests(parsedQuests);
    }

    public void UpdateQuestProgress(string jsonMessage)
    {
        var progressedQuest = JsonUtility.FromJson<QuestModel>(jsonMessage);
        QuestsController.i.UpdateQuestProgress(progressedQuest);
    }

    public void RemoveQuest(string jsonMessage)
    {
        var quest = JsonUtility.FromJson<QuestModel>(jsonMessage);
        QuestsController.i.RemoveQuest(quest);
    }

    private void OnDestroy()
    {
        QuestsController.i?.Dispose();
    }
}
