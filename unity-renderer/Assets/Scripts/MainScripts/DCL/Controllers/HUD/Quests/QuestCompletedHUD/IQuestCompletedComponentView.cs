using System;
using System.Collections.Generic;

namespace DCL.Quests
{
    public interface IQuestCompletedComponentView
    {
        event Action OnConfirmed;

        void SetTitle(string title);
        void SetIsGuest(bool isGuest);
        void SetRewards(List<QuestRewardComponentModel> rewards);
        void SetVisible(bool isVisible);
    }
}
