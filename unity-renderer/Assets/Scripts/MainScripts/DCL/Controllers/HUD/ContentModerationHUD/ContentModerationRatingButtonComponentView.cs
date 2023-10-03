using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.ContentModeration
{
    public class ContentModerationRatingButtonComponentView : BaseComponentView
    {
        [SerializeField] private Button ratingButton;
        [SerializeField] private GameObject selectedContainer;
        [SerializeField] private GameObject unselectedContainer;
        [SerializeField] private Color selectedColor;
        [SerializeField] private Color unselectedColor;
        [SerializeField] private List<TMP_Text> textsToColor;
        [SerializeField] private GameObject currentMark;

        public Button RatingButton => ratingButton;

        public override void RefreshControl() { }

        public void Select(bool isSelected)
        {
            selectedContainer.SetActive(isSelected);
            unselectedContainer.SetActive(!isSelected);

            foreach (TMP_Text text in textsToColor)
                text.color = isSelected ? selectedColor : unselectedColor;
        }

        public void SetCurrentMarkArctive(bool isActive) =>
            currentMark.SetActive(isActive);
    }
}
