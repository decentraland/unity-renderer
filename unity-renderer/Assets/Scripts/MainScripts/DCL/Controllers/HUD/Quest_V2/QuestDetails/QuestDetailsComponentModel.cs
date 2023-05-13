using System;

namespace DCL.Quests
{
    [Serializable]
    public record QuestDetailsComponentModel
    {
        public string questName;
        public string questDescription;
    }
}
