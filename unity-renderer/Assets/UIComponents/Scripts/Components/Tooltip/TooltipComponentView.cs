using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIComponents.Scripts.Components.Tooltip
{
    public class TooltipComponentView : BaseComponentView<TooltipComponentModel>, IDeselectHandler
    {
        private const int OFFSET = 16;

        [SerializeField] private TMP_Text text;

        private bool mouseIsOver;

        private RectTransform rectTransform;
        private RectTransform rect => rectTransform ??= GetComponent<RectTransform>();

        public override void OnEnable()
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
            base.OnEnable();
        }

        public override void OnFocus()
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
            base.OnFocus();
        }

        public override void OnLoseFocus()
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
            base.OnLoseFocus();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (!isFocused)
                Hide();
        }

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
