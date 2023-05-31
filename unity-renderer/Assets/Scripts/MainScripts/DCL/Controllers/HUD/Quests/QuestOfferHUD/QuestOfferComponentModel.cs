using System;
using System.Collections.Generic;

namespace DCL.Quests
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
