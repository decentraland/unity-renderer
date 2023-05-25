using DCL.Quests;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DCL.Quests
{
    public class QuestLogComponentView : BaseComponentView, IQuestLogComponentView
    {
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

        public override void Awake()
        {
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
        }

        public override void RefreshControl()
        {
            throw new NotImplementedException();
        }
    }
}
