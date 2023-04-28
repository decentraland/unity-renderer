using System;

namespace DCL.Quest
{
    [Serializable]
    public record QuestOfferComponentModel
    {
        public string questId;
        public string title;
        public string description;
    }
}
