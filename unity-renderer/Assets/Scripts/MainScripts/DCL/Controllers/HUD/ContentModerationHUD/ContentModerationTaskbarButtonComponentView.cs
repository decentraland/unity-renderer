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

        public void SetContentModerationRating(string rating) =>
            contentModerationRatingText.text = rating;
    }
}
