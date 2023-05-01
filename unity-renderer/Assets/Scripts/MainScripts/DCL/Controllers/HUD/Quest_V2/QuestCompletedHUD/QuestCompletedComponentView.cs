using System.Collections.Generic;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;

namespace DCL.Quest
{
    public class QuestCompletedComponentView : BaseComponentView<QuestCompletedComponentModel>, IQuestCompletedComponentView
    {
        [SerializeField] private TMP_Text questTitle;

        public override void RefreshControl()
        {
            SetTitle(model.title);
            SetRewards(model.rewards);
        }

        public void SetTitle(string title)
        {
            model.title = title;
            questTitle.text = title;
        }

        public void SetRewards(List<QuestRewardComponentModel> rewards)
        {
            model.rewards = rewards;

        }
    }
}
