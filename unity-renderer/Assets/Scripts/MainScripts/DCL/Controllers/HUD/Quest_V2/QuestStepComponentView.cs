using System;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;

namespace DCL.Quest
{
    public class QuestStepComponentView : BaseComponentView<QuestStepComponentModel>, IQuestStepComponentView
    {
        [Header("Configuration")]
        [SerializeField] internal QuestStepComponentModel model;

        [SerializeField] internal GameObject completedQuestToggle;
        [SerializeField] internal GameObject nonCompletedQuestToggle;
        [SerializeField] internal TMP_Text questStepText;

        [SerializeField] internal Color normalTextColor = Color.white;
        [SerializeField] internal Color completedTextColor = Color.gray;

        public override void RefreshControl()
        {
            if (model == null)
                return;

            SetIsCompleted(model.isCompleted);
            SetQuestStepText(model.text);
        }

        public void SetIsCompleted(bool isCompleted)
        {
            model.isCompleted = isCompleted;

            completedQuestToggle.SetActive(isCompleted);
            nonCompletedQuestToggle.SetActive(!isCompleted);
            questStepText.color = isCompleted ? completedTextColor : normalTextColor;
        }

        public void SetQuestStepText(string stepText)
        {
            model.text = stepText;

            questStepText.text = stepText;
        }
    }
}
