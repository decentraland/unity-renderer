using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Quest
{
    [Serializable]
    public record QuestTrackerComponentModel
    {
        public string questTitle;
        public Vector2Int coordinates;
        public List<QuestStepComponentModel> questSteps;
    }
}
