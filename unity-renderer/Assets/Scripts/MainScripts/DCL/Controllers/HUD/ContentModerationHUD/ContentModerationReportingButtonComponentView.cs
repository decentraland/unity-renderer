using DCL.Controllers;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.ContentModeration
{
    public class ContentModerationReportingButtonComponentView : BaseComponentView, IContentModerationReportingButtonComponentView
    {
        [SerializeField] private Button contentModerationButton;
        [SerializeField] private Image flagImage;
        [SerializeField] private Sprite teenFlagIcon;
        [SerializeField] private Sprite adultFlagIcon;
        [SerializeField] private Sprite restrictedFlagIcon;

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

        public void Show() =>
            base.Show();

        public void Hide() =>
            base.Hide();

        public void SetContentCategory(SceneContentCategory contentCategory)
        {
            switch (contentCategory)
            {
                case SceneContentCategory.ADULT:
                    flagImage.sprite = adultFlagIcon;
                    break;
                case SceneContentCategory.RESTRICTED:
                    flagImage.sprite = restrictedFlagIcon;
                    break;
                case SceneContentCategory.TEEN:
                default:
                    flagImage.sprite = teenFlagIcon;
                    break;
            }
        }
    }
}
