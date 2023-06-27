using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Quests
{
    [Serializable]
    public record QuestDetailsComponentModel
    {
        public string questName;
        public string questCreator;
        public string questDescription;
        public string questId = "";
        public bool isPinned = false;
        public Vector2Int coordinates;
        public List<QuestStepComponentModel> questSteps;
        public List<QuestRewardComponentModel> questRewards;
    }
}
