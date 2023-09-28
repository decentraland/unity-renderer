using DCL.Controllers;
using System;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.ContentModeration
{
    public class ContentModerationReportingComponentView : BaseComponentView<ContentModerationReportingModel>, IContentModerationReportingComponentView
    {
        [SerializeField] internal Button backgroundButton;
        [SerializeField] internal ButtonComponentView cancelButton;
        [SerializeField] internal Slider ratingSlider;
        [SerializeField] internal TMP_Text optionsSectionTitle;
        [SerializeField] internal TMP_Text optionsSectionSubtitle;

        public event Action OnPanelClosed;

        public override void Awake()
        {
            base.Awake();
            backgroundButton.onClick.AddListener(HidePanel);
            cancelButton.onClick.AddListener(HidePanel);
            ratingSlider.onValueChanged.AddListener(OnRatingSliderChanged);
        }

        public override void Dispose()
        {
            backgroundButton.onClick.RemoveAllListeners();
            cancelButton.onClick.RemoveAllListeners();
            ratingSlider.onValueChanged.RemoveAllListeners();
            base.Dispose();
        }

        public override void RefreshControl()
        {
            SetRating(model.currentRating);
        }

        public void ShowPanel() =>
            Show();

        public void HidePanel()
        {
            Hide();
            OnPanelClosed?.Invoke();
        }

        public void SetRating(SceneContentCategory contentCategory)
        {
            switch (contentCategory)
            {
                default:
                case SceneContentCategory.TEEN:
                    ratingSlider.value = 0;
                    break;
                case SceneContentCategory.ADULT:
                    ratingSlider.value = 1;
                    break;
                case SceneContentCategory.RESTRICTED:
                    ratingSlider.value = 2;
                    break;
            }
        }

        private void OnRatingSliderChanged(float value)
        {
            switch ((int)value)
            {
                case 0:
                    optionsSectionTitle.text = model.teenOptionTitle;
                    optionsSectionSubtitle.text = model.teenOptionSubtitle;
                    break;
                case 1:
                    optionsSectionTitle.text = model.adultOptionTitle;
                    optionsSectionSubtitle.text = model.adultOptionSubtitle;
                    break;
                case 2:
                    optionsSectionTitle.text = model.restrictedOptionTitle;
                    optionsSectionSubtitle.text = model.restrictedOptionSubtitle;
                    break;
            }
        }
    }
}
