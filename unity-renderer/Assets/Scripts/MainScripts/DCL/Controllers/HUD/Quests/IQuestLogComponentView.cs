using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Quests
{
    public interface IQuestLogComponentView
    {
        event Action<string, bool> OnPinChange;

        void AddActiveQuest(QuestDetailsComponentModel activeQuest);
        void AddCompletedQuest(QuestDetailsComponentModel completedQuest);
        void SetAsFullScreenMenuMode(Transform parentTransform);
    }
}
