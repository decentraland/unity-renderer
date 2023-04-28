using System;
using System.Collections.Generic;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace DCL.Quest
{
    public class QuestOfferComponentView : BaseComponentView<QuestOfferComponentModel>, IQuestOfferComponentView
    {

        [SerializeField] internal GameObject rewardsSection;
        [SerializeField] internal TMP_Text questTitle;
        [SerializeField] internal TMP_Text questDescription;
        [SerializeField] internal Button acceptButton;
        [SerializeField] internal Button cancelButton;
        [SerializeField] internal Transform rewardsContainer;

        [SerializeField] internal GameObject rewardPrefab;

        public event Action<string> OnQuestAccepted;

        public override void Awake()
        {
            acceptButton.onClick.RemoveAllListeners();
            acceptButton.onClick.AddListener(()=>OnQuestAccepted?.Invoke(model.questId));
        }

        public override void RefreshControl()
        {
            SetQuestId(model.questId);
            SetQuestTitle(model.title);
            SetQuestDescription(model.description);
            SetRewards(model.rewardsList);
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
            foreach (var rewardModel in rewardsList)
                Instantiate(rewardPrefab, rewardsContainer).GetComponent<QuestRewardComponentView>().SetModel(rewardModel);
        }
    }
}
