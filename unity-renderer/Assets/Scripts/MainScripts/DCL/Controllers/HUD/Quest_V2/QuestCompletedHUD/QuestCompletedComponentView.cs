using MainScripts.DCL.Helpers.Utils;
using System.Collections.Generic;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;

namespace DCL.Quest
{
    public class QuestCompletedComponentView : BaseComponentView<QuestCompletedComponentModel>, IQuestCompletedComponentView
    {
        private const int MAX_REWARDS_COUNT = 3;

        [SerializeField] internal GameObject rewardsSection;
        [SerializeField] private TMP_Text questTitle;
        [SerializeField] internal Transform rewardsContainer;

        [SerializeField] internal QuestRewardComponentView rewardPrefab;

        private UnityObjectPool<QuestRewardComponentView> rewardsPool;
        private List<QuestRewardComponentView> usedRewards = new ();

        public override void Awake()
        {
            rewardsPool = new UnityObjectPool<QuestRewardComponentView>(rewardPrefab, rewardsContainer);
            rewardsPool.Prewarm(MAX_REWARDS_COUNT);
        }

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

        public void SetRewards(List<QuestRewardComponentModel> rewardsList)
        {
            model.rewards = rewardsList;

            for (var i = 0; i < rewardsContainer.childCount; i++)
                Destroy(rewardsContainer.GetChild(i));

            if (rewardsList == null || rewardsList.Count == 0)
            {
                rewardsSection.SetActive(false);
                return;
            }

            rewardsSection.SetActive(true);

            foreach (var pooledReward in usedRewards)
                rewardsPool.Release(pooledReward);

            foreach (var rewardModel in rewardsList)
            {
                QuestRewardComponentView pooledReward = rewardsPool.Get();
                pooledReward.SetModel(rewardModel);
                usedRewards.Add(pooledReward);
            }
        }
    }
}
