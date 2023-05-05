using System;

namespace DCL.Quests
{
    [Serializable]
    public record QuestStepComponentModel
    {
        public string text;
        public bool isCompleted;
    }
}
