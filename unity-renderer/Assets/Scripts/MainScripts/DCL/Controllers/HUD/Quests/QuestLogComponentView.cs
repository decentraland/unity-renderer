using DCL.Quests;
using MainScripts.DCL.Helpers.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DCL.Quests
{
    public class QuestLogComponentView : BaseComponentView, IQuestLogComponentView
    {
        private const int MAX_QUESTS_COUNT = 10;
        private const string IN_PROGRESS_TITLE = "Active quests";
        private const string COMPLETED_TITLE = "Completed quests";
        private const int IN_PROGRESS_SECTION_INDEX = 0;
        private const int COMPLETED_SECTION_INDEX = 1;

        [SerializeField] internal SectionSelectorComponentView sectionSelector;
        [SerializeField] internal QuestDetailsComponentView questDetailsComponentView;
        [SerializeField] internal TMP_Text headerText;
        [SerializeField] internal GameObject emptyState;
        [SerializeField] internal GameObject emptyActiveState;
        [SerializeField] internal GameObject emptyCompletedState;
        [SerializeField] internal Transform activeQuestsContainer;
        [SerializeField] internal ActiveQuestComponentView activeQuestPrefab;

        public event Action<string, bool> OnPinChange;

        private Dictionary<string, ActiveQuestComponentView> activeQuests = new Dictionary<string, ActiveQuestComponentView>();
        private Dictionary<string, ActiveQuestComponentView> completedQuests = new Dictionary<string, ActiveQuestComponentView>();
        private UnityObjectPool<ActiveQuestComponentView> questsPool;
        private UnityObjectPool<ActiveQuestComponentView> completedQuestsPool;

        public void Start()
        {
            questsPool = new UnityObjectPool<ActiveQuestComponentView>(activeQuestPrefab, activeQuestsContainer);
            questsPool.Prewarm(MAX_QUESTS_COUNT);
            sectionSelector.GetSection(IN_PROGRESS_SECTION_INDEX).onSelect.RemoveAllListeners();
            sectionSelector.GetSection(COMPLETED_SECTION_INDEX).onSelect.RemoveAllListeners();
            sectionSelector.GetSection(IN_PROGRESS_SECTION_INDEX).onSelect.AddListener((isSelected) =>
            {
                if (isSelected)
                {
                    headerText.text = IN_PROGRESS_TITLE;
                }
            });
            sectionSelector.GetSection(COMPLETED_SECTION_INDEX).onSelect.AddListener((isSelected) =>
            {
                if (isSelected)
                {
                    headerText.text = COMPLETED_TITLE;
                }
            });

            questDetailsComponentView.OnPinChange += (questId, isPinned) => OnPinChange?.Invoke(questId, isPinned);
        }

        public void AddActiveQuest(QuestDetailsComponentModel activeQuest)
        {
            if (!activeQuests.ContainsKey(activeQuest.questId))
                activeQuests[activeQuest.questId] = questsPool.Get();

            activeQuests[activeQuest.questId].SetModel(new ActiveQuestComponentModel()
            {
                questId = activeQuest.questId,
                questCreator = activeQuest.questCreator,
                questName = activeQuest.questName,
                questImageUri = "",
                isPinned = false,
                questModel = activeQuest
            });
        }

        public void AddCompletedQuest(QuestDetailsComponentModel completedQuest)
        {
            if (!completedQuests.ContainsKey(completedQuest.questId))
                completedQuests[completedQuest.questId] = questsPool.Get();

            completedQuests[completedQuest.questId].SetModel(new ActiveQuestComponentModel()
            {
                questId = completedQuest.questId,
                questCreator = completedQuest.questCreator,
                questName = completedQuest.questName,
                questImageUri = "",
                isPinned = false,
                questModel = completedQuest
            });
        }

        public void SetAsFullScreenMenuMode(Transform parentTransform)
        {
            if (parentTransform == null)
                return;

            transform.SetParent(parentTransform);
            transform.localScale = Vector3.one;

            RectTransform rectTransform = transform as RectTransform;
            if (rectTransform == null) return;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.localPosition = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;
            gameObject.SetActive(true);
        }

        public override void RefreshControl()
        {
        }
    }
}
