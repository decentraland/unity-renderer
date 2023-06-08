using MainScripts.DCL.Helpers.Utils;
using System;
using System.Collections.Generic;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace DCL.Quests
{
    public class QuestOfferComponentView : BaseComponentView<QuestOfferComponentModel>, IQuestOfferComponentView
    {
        private const int MAX_REWARDS_COUNT = 3;

        [SerializeField] internal GameObject rewardsSection;
        [SerializeField] internal GameObject guestSection;
        [SerializeField] internal TMP_Text questTitle;
        [SerializeField] internal TMP_Text questDescription;
        [SerializeField] internal Button acceptButton;
        [SerializeField] internal Button cancelButton;
        [SerializeField] internal Transform rewardsContainer;

        [SerializeField] internal QuestRewardComponentView rewardPrefab;
        public event Action<string> OnQuestAccepted;

        private UnityObjectPool<QuestRewardComponentView> rewardsPool;
        private List<QuestRewardComponentView> usedRewards = new ();

        public override void Awake()
        {
            acceptButton.onClick.RemoveAllListeners();
            acceptButton.onClick.AddListener(()=>OnQuestAccepted?.Invoke(model.questId));
            rewardsPool = new UnityObjectPool<QuestRewardComponentView>(rewardPrefab, rewardsContainer);
            rewardsPool.Prewarm(MAX_REWARDS_COUNT);
        }

        public override void Dispose()
        {
            base.Dispose();

            foreach (var pooledReward in usedRewards)
                rewardsPool.Release(pooledReward);
            rewardsPool.Clear();
        }

        public override void RefreshControl()
        {
            SetQuestId(model.questId);
            SetQuestTitle(model.title);
            SetQuestDescription(model.description);
            SetRewards(model.rewardsList);
        }

        public void SetIsGuest(bool isGuest)
        {
            guestSection.SetActive(isGuest);
        }

        public void SetQuestId(string questId) =>
            model.questId = questId;

        public void SetQuestTitle(string title)
        {
            model.title = title;
            questTitle.text = title;
        }

        public void SetQuestDescription(string description)
        {
            model.description = description;
            questDescription.text = description;
        }

        public void SetRewards(List<QuestRewardComponentModel> rewardsList)
        {
            model.rewardsList = rewardsList;

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
