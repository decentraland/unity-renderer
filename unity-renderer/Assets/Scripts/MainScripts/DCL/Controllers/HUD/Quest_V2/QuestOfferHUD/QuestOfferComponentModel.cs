using System;
using System.Collections.Generic;

namespace DCL.Quest
{
    [Serializable]
    public record QuestOfferComponentModel
    {
        public string questId;
        public string title;
        public string description;
        public List<QuestRewardComponentModel> rewardsList;
    }
}
