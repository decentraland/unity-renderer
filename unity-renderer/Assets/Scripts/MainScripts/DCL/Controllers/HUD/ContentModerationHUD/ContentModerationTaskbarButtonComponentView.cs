using DCL.Controllers;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.ContentModeration
{
    public class ContentModerationTaskbarButtonComponentView : BaseComponentView
    {
        [SerializeField] internal Button contentModerationButton;
        [SerializeField] internal TMP_Text contentModerationRatingText;
        [SerializeField] internal Image contentModerationRatingBackground;
        [SerializeField] internal Color backgroundColorForTeen;
        [SerializeField] internal Color backgroundColorForAdult;
        [SerializeField] internal Color backgroundColorForRestricted;

        public event Action OnContentModerationPressed;

        public override void Awake()
        {
            base.Awake();
            contentModerationButton.onClick.AddListener(() => OnContentModerationPressed?.Invoke());
        }

        public override void Dispose()
        {
            contentModerationButton.onClick.RemoveAllListeners();
            base.Dispose();
        }

        public override void RefreshControl() { }

        public void SetContentCategory(SceneContentCategory contentCategory)
        {
            switch (contentCategory)
            {
                case SceneContentCategory.ADULT:
                    contentModerationRatingText.text = "A";
                    contentModerationRatingBackground.color = backgroundColorForAdult;
                    break;
                case SceneContentCategory.RESTRICTED:
                    contentModerationRatingText.text = "R";
                    contentModerationRatingBackground.color = backgroundColorForRestricted;
                    break;
                case SceneContentCategory.TEEN:
                default:
                    contentModerationRatingText.text = "T";
                    contentModerationRatingBackground.color = backgroundColorForTeen;
                    break;
            }
        }
    }
}
