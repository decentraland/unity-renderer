using TMPro;
using UnityEngine;

namespace UIComponents.Scripts.Components.Tooltip
{
    public class TooltipComponentView : BaseComponentView<TooltipComponentModel>
    {
        private const int OFFSET = 16;

        [SerializeField] private TMP_Text text;

        private RectTransform rectTransform;
        private RectTransform rect => rectTransform ??= GetComponent<RectTransform>();

        public override void Show(bool instant = false)
        {
            gameObject.SetActive(true);
            base.Show(instant);
        }

        public override void RefreshControl()
        {
            SetText(model.message);
            AdjustTooltipSizeToText();

            void AdjustTooltipSizeToText()
            {
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, text.preferredWidth + (OFFSET * 2));
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, text.preferredHeight + (OFFSET * 2));
            }
        }

        private void SetText(string newText)
        {
            model.message = newText;

            if (text != null)
                text.text = newText;
        }
    }
}
