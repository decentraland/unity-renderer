using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Quests
{
    public interface IQuestLogComponentView
    {
        event Action<string, bool> OnPinChange;
        event Action<Vector2Int> OnJumpIn;
        event Action<string> OnQuestAbandon;

        void AddActiveQuest(QuestDetailsComponentModel activeQuest, string creatorName);
        void AddCompletedQuest(QuestDetailsComponentModel completedQuest, string creatorName);
        void RemoveQuestIfExists(string questId);
        void SetAsFullScreenMenuMode(Transform parentTransform);
        void SetIsGuest(bool isGuest);
    }
}
