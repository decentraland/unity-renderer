using MainScripts.DCL.Helpers.Utils;
using System;
using System.Collections.Generic;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Quests
{
    public class QuestDetailsComponentView : BaseComponentView<QuestDetailsComponentModel>, IQuestDetailsComponentView
    {
        private const int MAX_REWARDS_COUNT = 5;
        private const int MAX_STEPS_COUNT = 10;

        [SerializeField] internal TMP_Text questName;
        [SerializeField] internal TMP_Text questCreator;
        [SerializeField] internal TMP_Text questDescription;
        [SerializeField] internal Transform stepsParent;
        [SerializeField] internal Transform rewardsParent;
        [SerializeField] internal Button jumpInButton;
        [SerializeField] internal Button pinButton;
        [SerializeField] internal Button abandonButton;
        [SerializeField] internal GameObject rewardsSection;
        [SerializeField] internal GameObject guestSection;

        [SerializeField] internal QuestRewardComponentView rewardPrefab;
        [SerializeField] internal QuestStepComponentView stepPrefab;

        public event Action<Vector2Int> OnJumpIn;
        public event Action<string, bool> OnPinChange;
        public event Action<string> OnQuestAbandon;

        private UnityObjectPool<QuestStepComponentView> stepsPool;
        private UnityObjectPool<QuestRewardComponentView> rewardsPool;

        private readonly List<QuestRewardComponentView> usedRewards = new ();
        private readonly List<QuestStepComponentView> usedSteps = new ();

        public override void Awake()
        {
            jumpInButton.onClick.RemoveAllListeners();
            jumpInButton.onClick.AddListener(() => InvokeJumpIn(model.coordinates));
            pinButton.onClick.RemoveAllListeners();
            pinButton.onClick.AddListener(() =>
            {
                model.isPinned = !model.isPinned;
                OnPinChange?.Invoke(model.questId, model.isPinned);
            });
            abandonButton.onClick.RemoveAllListeners();
            abandonButton.onClick.AddListener(() => OnQuestAbandon?.Invoke(model.questId));
            InitializePools();
        }

        public override void Dispose()
        {
            foreach (var pooledReward in usedRewards)
                rewardsPool.Release(pooledReward);
            rewardsPool.Clear();

            foreach (var pooledSteps in usedSteps)
                stepsPool.Release(pooledSteps);
            stepsPool.Clear();
        }

        public override void RefreshControl()
        {
            SetQuestId(model.questId);
            SetQuestName(model.questName);
            SetQuestCreator(model.questCreator);
            SetQuestDescription(model.questDescription);
            SetCoordinates(model.coordinates);
            SetQuestSteps(model.questSteps);
            SetQuestRewards(model.questRewards);
        }

        public void SetQuestName(string nameText)
        {
            model.questName = nameText;
            questName.text = nameText;
        }

        public void SetQuestCreator(string creatorText)
        {
            model.questCreator = creatorText;
            questCreator.text = creatorText;
        }

        public void SetQuestDescription(string description)
        {
            model.questDescription = description;
            questDescription.text = description;
        }

        public void SetQuestId(string questId) =>
            model.questId = questId;

        public void SetCoordinates(Vector2Int coordinates) =>
            model.coordinates = coordinates;

        public void SetIsPinned(bool isPinned) =>
            model.isPinned = isPinned;

        public void SetQuestSteps(List<QuestStepComponentModel> questSteps)
        {
            model.questSteps = questSteps;

            for (var i = 0; i < stepsParent.childCount; i++)
                Destroy(stepsParent.GetChild(i));

            foreach (var pooledStep in usedSteps)
                stepsPool.Release(pooledStep);

            foreach (var stepModel in questSteps)
            {
                QuestStepComponentView pooledStep = stepsPool.Get();
                pooledStep.OnJumpIn -= InvokeJumpIn;
                pooledStep.OnJumpIn += InvokeJumpIn;
                pooledStep.SetModel(stepModel);
                usedSteps.Add(pooledStep);
            }
        }

        public void SetQuestRewards(List<QuestRewardComponentModel> questRewards)
        {
            model.questRewards = questRewards;

            for (var i = 0; i < rewardsParent.childCount; i++)
                Destroy(rewardsParent.GetChild(i));

            if (questRewards == null || questRewards.Count == 0)
            {
                rewardsSection.SetActive(false);
                return;
            }

            rewardsSection.SetActive(true);

            foreach (var pooledReward in usedRewards)
                rewardsPool.Release(pooledReward);

            foreach (var rewardModel in questRewards)
            {
                QuestRewardComponentView pooledReward = rewardsPool.Get();
                pooledReward.SetModel(rewardModel);
                usedRewards.Add(pooledReward);
            }
        }

        public void SetIsGuest(bool isGuest) =>
            guestSection.SetActive(isGuest);

        private void InvokeJumpIn(Vector2Int coordinates) =>
            OnJumpIn?.Invoke(coordinates);

        private void InitializePools()
        {
            rewardsPool = new UnityObjectPool<QuestRewardComponentView>(rewardPrefab, rewardsParent);
            rewardsPool.Prewarm(MAX_REWARDS_COUNT);
            stepsPool = new UnityObjectPool<QuestStepComponentView>(stepPrefab, stepsParent);
            stepsPool.Prewarm(MAX_STEPS_COUNT);
        }
    }
}
