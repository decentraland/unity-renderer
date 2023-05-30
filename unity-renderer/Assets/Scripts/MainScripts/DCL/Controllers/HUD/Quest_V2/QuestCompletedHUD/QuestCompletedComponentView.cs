using MainScripts.DCL.Helpers.Utils;
using System;
using System.Collections.Generic;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Quests
{
    public class QuestCompletedComponentView : BaseComponentView<QuestCompletedComponentModel>, IQuestCompletedComponentView
    {
        private const int MAX_REWARDS_COUNT = 3;

        [SerializeField] internal GameObject rewardsSection;
        [SerializeField] internal TMP_Text questTitle;
        [SerializeField] internal Transform rewardsContainer;
        [SerializeField] private Button confirmButton;
        [SerializeField] internal GameObject guestSection;
        [SerializeField] internal GameObject container;

        [SerializeField] internal QuestRewardComponentView rewardPrefab;

        public event Action OnConfirmed;

        private UnityObjectPool<QuestRewardComponentView> rewardsPool;
        private List<QuestRewardComponentView> usedRewards = new ();

        public override void Awake()
        {
            rewardsPool = new UnityObjectPool<QuestRewardComponentView>(rewardPrefab, rewardsContainer);
            rewardsPool.Prewarm(MAX_REWARDS_COUNT);
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(()=>
            {
                SetVisible(false);
                OnConfirmed?.Invoke();
            });
            SetVisible(false);
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

        public void SetIsGuest(bool isGuest)
        {
            guestSection.SetActive(isGuest);
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

        public void SetVisible(bool isVisible) =>
            container.SetActive(isVisible);
    }
}
