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
        [SerializeField] private List<TMP_Text> texts;
        [SerializeField] private List<Image> imagesToColor;
        [SerializeField] private GameObject currentMark;
        [SerializeField] private List<Image> backgroundImages;

        public Button RatingButton => ratingButton;
        public bool IsMarked { get; private set; }

        public override void Awake()
        {
            base.Awake();

            foreach (Image image in imagesToColor)
                image.color = selectedColor;
        }

        public override void RefreshControl() { }

        public void Select(bool isSelected)
        {
            selectedContainer.SetActive(isSelected);
            unselectedContainer.SetActive(!isSelected);

            foreach (TMP_Text text in texts)
                text.color = IsMarked ? selectedColor : unselectedColor;

            foreach (Image background in backgroundImages)
                background.color = IsMarked && isSelected ? backgroundMarkedColor : backgroundNormalColor;
        }

        public void SetCurrentMarkActive(bool isActive)
        {
            IsMarked = isActive;
            currentMark.SetActive(isActive);
        }
    }
}
