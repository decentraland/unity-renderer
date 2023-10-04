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
        [SerializeField] private Color backgroundNormalColor;
        [SerializeField] private Color backgroundMarkedColor;
        [SerializeField] private List<TMP_Text> textsToColor;
        [SerializeField] private GameObject currentMark;
        [SerializeField] private Image backgroundImage;

        public Button RatingButton => ratingButton;
        public bool IsMarked { get; private set; }

        public override void RefreshControl() { }

        public void Select(bool isSelected)
        {
            selectedContainer.SetActive(isSelected || IsMarked);
            unselectedContainer.SetActive(!isSelected && !IsMarked);

            foreach (TMP_Text text in textsToColor)
                text.color = isSelected || IsMarked ? selectedColor : unselectedColor;

            backgroundImage.color = IsMarked ? backgroundMarkedColor : backgroundNormalColor;
        }

        public void SetCurrentMarkActive(bool isActive)
        {
            IsMarked = isActive;
            currentMark.SetActive(isActive);
        }
    }
}
