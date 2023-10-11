using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.ContentModeration
{
    public class ContentModerationReportingOptionComponentView : BaseComponentView
    {
        [SerializeField] private Button optionButton;
        [SerializeField] private TMP_Text optionText;
        [SerializeField] private Image optionBackground;
        [SerializeField] internal Color backgroundUnselectedColor;
        [SerializeField] internal Color textUnselectedColor;
        [SerializeField] internal Color backgroundSelectedColor;
        [SerializeField] internal Color textSelectedColor;

        public Button OptionButton => optionButton;

        public override void RefreshControl() { }

        public void SetOptionText(string text) =>
            optionText.text = text;

        public void SelectOption()
        {
            optionBackground.color = backgroundSelectedColor;
            optionText.color = textSelectedColor;
        }

        public void UnselectOption()
        {
            optionBackground.color = backgroundUnselectedColor;
            optionText.color = textUnselectedColor;
        }

        public void SetActive(bool isActive) =>
            gameObject.SetActive(isActive);
    }
}
