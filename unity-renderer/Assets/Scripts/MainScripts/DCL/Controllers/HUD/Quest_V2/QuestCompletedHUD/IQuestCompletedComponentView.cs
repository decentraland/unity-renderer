using System;
using System.Collections.Generic;

namespace DCL.Quest
{
    public interface IQuestCompletedComponentView
    {
        event Action OnConfirmed;

        void SetTitle(string title);
        void SetRewards(List<QuestRewardComponentModel> rewards);
    }
}
