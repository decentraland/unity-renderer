using System;
using UnityEngine;

namespace DCL.Quests
{
    [Serializable]
    public record QuestStepComponentModel
    {
        public string text;
        public Vector2Int coordinates;
        public bool isCompleted;
        public bool supportsJumpIn;
    }
}
