using System;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Quests
{
    public class QuestStepComponentView : BaseComponentView<QuestStepComponentModel>, IQuestStepComponentView
    {
        [SerializeField] internal GameObject completedQuestToggle;
        [SerializeField] internal GameObject nonCompletedQuestToggle;
        [SerializeField] internal TMP_Text questStepText;
        [SerializeField] internal Button jumpInButton;

        [SerializeField] internal Color normalTextColor = Color.white;
        [SerializeField] internal Color completedTextColor = Color.gray;

        public event Action<Vector2Int> OnJumpIn;

        public override void Awake()
        {
            jumpInButton.onClick.RemoveAllListeners();
            jumpInButton.onClick.AddListener(() => OnJumpIn?.Invoke(model.coordinates));
        }

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

        public void SetCoordinates(Vector2Int coordinates) =>
            model.coordinates = coordinates;

        public void SetSupportsJumpIn(bool supportsJumpIn)
        {
            model.supportsJumpIn = supportsJumpIn;
            jumpInButton.gameObject.SetActive(supportsJumpIn);
        }
    }
}
