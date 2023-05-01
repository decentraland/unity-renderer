using System.Collections.Generic;

namespace DCL.Quest
{
    public interface IQuestCompletedComponentView
    {
        void SetTitle(string title);
        void SetRewards(List<QuestRewardComponentModel> rewards);
    }
}
