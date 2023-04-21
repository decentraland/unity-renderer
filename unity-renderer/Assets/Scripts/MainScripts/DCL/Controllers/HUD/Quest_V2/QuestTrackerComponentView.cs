using System;
using System.Collections.Generic;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Quest
{
    public class QuestTrackerComponentView : BaseComponentView<QuestTrackerComponentModel>, IQuestTrackerComponentView
    {
        [SerializeField] internal TMP_Text questTitleText;
        [SerializeField] internal Transform stepsContainer;
        [SerializeField] internal QuestStepComponentView stepPrefab;
        [SerializeField] internal Button jumpInButton;

        public event Action<Vector2Int> OnJumpIn;
        internal List<GameObject> currentQuestSteps = new();

        public override void Awake()
        {
            jumpInButton.onClick.RemoveAllListeners();
            jumpInButton.onClick.AddListener(() => OnJumpIn?.Invoke(model.coordinates));
        }

        public override void RefreshControl()
        {
            if (model == null)
                return;

            SetQuestTitle(model.questTitle);
            SetQuestCoordinates(model.coordinates);
            SetQuestSteps(model.questSteps);
        }

        public void SetQuestTitle(string questTitle)
        {
            model.questTitle = questTitle;
            questTitleText.text = questTitle;
        }

        public void SetQuestCoordinates(Vector2Int coordinates)
        {
            model.coordinates = coordinates;
        }

        public void SetQuestSteps(List<QuestStepComponentModel> questSteps)
        {
            foreach (var step in currentQuestSteps)
                Destroy(step);

            foreach (var step in questSteps)
            {
                QuestStepComponentView questStep = Instantiate(stepPrefab, stepsContainer);
                questStep.model = step;
                questStep.RefreshControl();
                currentQuestSteps.Add(questStep.gameObject);
            }
        }
    }

}
