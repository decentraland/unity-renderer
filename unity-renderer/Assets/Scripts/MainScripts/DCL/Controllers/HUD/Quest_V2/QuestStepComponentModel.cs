using System;

namespace DCL.Quest
{
    [Serializable]
    public record QuestStepComponentModel
    {
        public string text;
        public bool isCompleted;
    }
}
