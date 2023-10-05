using DCL.Controllers;
using DCL.Helpers;
using System;
using TMPro;
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
        [SerializeField] private TMP_Text tooltipText;

        private SceneContentCategory currentCategory;

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
                    flagImage.sprite = adultFlagIcon;
                    tooltipText.text = $"This Scene is rated as PR 18+";
                    break;
                case SceneContentCategory.RESTRICTED:
                    flagImage.sprite = restrictedFlagIcon;
                    tooltipText.text = $"This Scene is rated as RESTRICTED";
                    break;
                case SceneContentCategory.TEEN:
                default:
                    flagImage.sprite = teenFlagIcon;
                    tooltipText.text = $"This Scene is rated as PR 13+";
                    break;
            }

            if (currentCategory != contentCategory)
            {
                currentCategory = contentCategory;
                Utils.ForceRebuildLayoutImmediate(tooltipText.transform.parent as RectTransform);
            }
        }
    }
}
