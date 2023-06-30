using MainScripts.DCL.Helpers.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public event Action<Vector2Int> OnJumpIn;
        public event Action<string> OnQuestAbandon;

        private Dictionary<string, ActiveQuestComponentView> activeQuests;
        private Dictionary<string, ActiveQuestComponentView> completedQuests;
        private UnityObjectPool<ActiveQuestComponentView> questsPool;
        private string previouslyActiveSelectedQuest;
        private string previouslyCompletedSelectedQuest;

        public override void Awake()
        {
            activeQuests = new ();
            completedQuests = new ();
            questsPool = new UnityObjectPool<ActiveQuestComponentView>(activeQuestPrefab, activeQuestsContainer, actionOnDestroy: x => x.Hide());
            questsPool.Prewarm(MAX_QUESTS_COUNT);
            InitialiseSectionSelector();

            questDetailsComponentView.OnJumpIn += (coords) => OnJumpIn?.Invoke(coords);
            questDetailsComponentView.OnPinChange += (questId, isPinned) => OnPinChange?.Invoke(questId, isPinned);
            questDetailsComponentView.OnQuestAbandon += (questId) => OnQuestAbandon?.Invoke(questId);

            emptyState.SetActive(true);
        }

        private void InitialiseSectionSelector()
        {
            sectionSelector.Awake();
            sectionSelector.GetSection(IN_PROGRESS_SECTION_INDEX).onSelect.RemoveAllListeners();
            sectionSelector.GetSection(COMPLETED_SECTION_INDEX).onSelect.RemoveAllListeners();
            sectionSelector.GetSection(IN_PROGRESS_SECTION_INDEX).onSelect.AddListener(InProgressSectionOnSelect);
            sectionSelector.GetSection(COMPLETED_SECTION_INDEX).onSelect.AddListener(CompletedSectionOnSelect);
        }

        private void CompletedSectionOnSelect(bool isSelected)
        {
            if (!isSelected)
                return;

            headerText.text = COMPLETED_TITLE;
            ShowActiveOrCompletedQuests(false);

            if (completedQuests.Count == 0)
            {
                emptyState.SetActive(true);
                emptyActiveState.SetActive(false);
                emptyCompletedState.SetActive(true);
            }
            else
            {
                emptyState.SetActive(false);
                HandleCompletedQuestSelection(completedQuests.First().Key);
            }
        }

        private void InProgressSectionOnSelect(bool isSelected)
        {
            if (!isSelected)
                return;

            headerText.text = IN_PROGRESS_TITLE;
            ShowActiveOrCompletedQuests(true);

            if (activeQuests.Count == 0)
            {
                emptyState.SetActive(true);
                emptyActiveState.SetActive(true);
                emptyCompletedState.SetActive(false);
            }
            else
            {
                emptyState.SetActive(false);
                HandleActiveQuestSelection(activeQuests.First().Key);
            }
        }

        private void ShowActiveOrCompletedQuests(bool active)
        {
            foreach (var activeQuest in activeQuests.Values)
            {
                activeQuest.gameObject.SetActive(active);

            }
            foreach (var completedQuest in completedQuests.Values)
            {
                completedQuest.gameObject.SetActive(!active);
            }
        }

        public void AddActiveQuest(QuestDetailsComponentModel activeQuest, string creatorName)
        {
            emptyState.SetActive(false);

            if (activeQuests.ContainsKey(activeQuest.questId))
            {
                questsPool.Release(activeQuests[activeQuest.questId]);
                activeQuests.Remove(activeQuest.questId);
            }

            activeQuests.TryAdd(activeQuest.questId, questsPool.Get());

            ActiveQuestComponentView activeQuestComponentView = activeQuests[activeQuest.questId];
            activeQuestComponentView.OnActiveQuestSelected -= SelectedQuest;
            activeQuestComponentView.SetModel(new ActiveQuestComponentModel()
            {
                questId = activeQuest.questId,
                questCreator = creatorName,
                questName = activeQuest.questName,
                questImageUri = activeQuest.questImageUri,
                isPinned = activeQuest.isPinned,
                questModel = activeQuest
            });
            activeQuestComponentView.OnActiveQuestSelected += SelectedQuest;
            HandleActiveQuestSelection(activeQuest.questId);
            sectionSelector.GetSection(IN_PROGRESS_SECTION_INDEX).SelectToggle(true);
        }

        public void AddCompletedQuest(QuestDetailsComponentModel completedQuest, string creatorName)
        {
            emptyState.SetActive(false);

            if (activeQuests.ContainsKey(completedQuest.questId))
            {
                questsPool.Release(activeQuests[completedQuest.questId]);
                activeQuests.Remove(completedQuest.questId);
            }

            if (!completedQuests.ContainsKey(completedQuest.questId))
                completedQuests.Add(completedQuest.questId, questsPool.Get());

            ActiveQuestComponentView completedQuestComponentView = completedQuests[completedQuest.questId];
            completedQuestComponentView.OnActiveQuestSelected -= SelectedCompletedQuest;
            completedQuestComponentView.SetModel(new ActiveQuestComponentModel()
            {
                questId = completedQuest.questId,
                questCreator = creatorName,
                questName = completedQuest.questName,
                questImageUri = completedQuest.questImageUri,
                isPinned = false,
                questModel = completedQuest
            });
            completedQuestComponentView.OnActiveQuestSelected += SelectedCompletedQuest;
            HandleCompletedQuestSelection(completedQuest.questId);
            sectionSelector.GetSection(IN_PROGRESS_SECTION_INDEX).SelectToggle(true);
        }

        public void RemoveQuestIfExists(string questId)
        {
            if (!activeQuests.ContainsKey(questId)) return;

            questsPool.Release(activeQuests[questId]);
            activeQuests.Remove(questId);
            if (activeQuests.Count == 0)
            {
                emptyState.SetActive(true);
                emptyActiveState.SetActive(true);
                emptyCompletedState.SetActive(false);
            }
            else
            {
                HandleActiveQuestSelection(activeQuests.First().Key);
            }
        }

        public void SetAsFullScreenMenuMode(Transform parentTransform)
        {
            if (parentTransform == null)
                return;

            Transform panelTransform;
            (panelTransform = transform).SetParent(parentTransform);
            panelTransform.localScale = Vector3.one;

            RectTransform rectTransform = panelTransform as RectTransform;
            if (rectTransform == null) return;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.localPosition = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;
            gameObject.SetActive(true);
        }

        private void HandleActiveQuestSelection(string questId)
        {
            if(!string.IsNullOrEmpty(previouslyActiveSelectedQuest))
                activeQuests[previouslyActiveSelectedQuest].Deselect();
            activeQuests[questId].OnPointerClick(null);
            previouslyActiveSelectedQuest = questId;
        }

        private void HandleCompletedQuestSelection(string questId)
        {
            if(!string.IsNullOrEmpty(previouslyCompletedSelectedQuest))
                completedQuests[previouslyCompletedSelectedQuest].Deselect();
            completedQuests[questId].OnPointerClick(null);
            previouslyCompletedSelectedQuest = questId;
        }

        private void SelectedQuest(QuestDetailsComponentModel questModel)
        {
            if(!string.IsNullOrEmpty(previouslyActiveSelectedQuest) && activeQuests.TryGetValue(previouslyActiveSelectedQuest, out ActiveQuestComponentView quest))
                quest.Deselect();

            questDetailsComponentView.SetModel(questModel);
            questDetailsComponentView.SetFooter(true);
            previouslyActiveSelectedQuest = questModel.questId;
        }

        private void SelectedCompletedQuest(QuestDetailsComponentModel questModel)
        {
            if(!string.IsNullOrEmpty(previouslyCompletedSelectedQuest) && completedQuests.TryGetValue(previouslyCompletedSelectedQuest, out ActiveQuestComponentView quest))
                quest.Deselect();

            questDetailsComponentView.SetFooter(false);
            questDetailsComponentView.SetModel(questModel);
            previouslyCompletedSelectedQuest = questModel.questId;
        }

        public void SetIsGuest(bool isGuest) =>
            questDetailsComponentView.SetIsGuest(isGuest);

        public override void RefreshControl()
        {
        }
    }
}
