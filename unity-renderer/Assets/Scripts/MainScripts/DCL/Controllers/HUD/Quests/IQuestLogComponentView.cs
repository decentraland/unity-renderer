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

        void AddActiveQuest(QuestDetailsComponentModel activeQuest);
        void AddCompletedQuest(QuestDetailsComponentModel completedQuest);
        void SetAsFullScreenMenuMode(Transform parentTransform);
        void SetIsGuest(bool isGuest);
    }
}
