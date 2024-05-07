using System;
using System.Collections.Generic;

namespace DCL.Quests
{
    [Serializable]
    public record QuestCompletedComponentModel
    {
        public string title;
        public List<QuestRewardComponentModel> rewards;
    }
}
