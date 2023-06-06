using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Quests
{
    public interface IQuestLogComponentView
    {
        void AddActiveQuest(QuestDetailsComponentModel activeQuest);
        void AddCompletedQuest(QuestDetailsComponentModel completedQuest);
    }
}
