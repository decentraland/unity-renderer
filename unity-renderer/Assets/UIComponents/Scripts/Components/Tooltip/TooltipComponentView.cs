using System;
using TMPro;
using UIComponents.Scripts.Components.Tooltip;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIComponents.Scripts.Components
{
    [SelectionBase, DisallowMultipleComponent]
    public class TooltipComponentView : BaseComponentView<TooltipComponentModel>, IDeselectHandler
    {
        private const int OFFSET = 16;

        [SerializeField] private TMP_Text text;
        [SerializeField] private BaseComponentView activatorComponent;

        private bool mouseIsOver;

        private RectTransform rectTransform;
        private RectTransform rect => rectTransform ??= GetComponent<RectTransform>();

        private void Start()
        {
            if (activatorComponent == null)
                return;

            activatorComponent.onFocused += isFocused =>
            {
                if (isFocused)
                    Show();
                else
                    Hide();
            };
        }

        public override void OnEnable()
        {
            base.OnEnable();

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public override void OnFocus()
        {
            base.OnFocus();

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public override void OnLoseFocus()
        {
            base.OnLoseFocus();

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(gameObject);
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
            SetAutoAdaptModeActive(model.autoAdaptContainerSize);
            AdjustTooltipSizeToText();

            void AdjustTooltipSizeToText()
            {
                if (!model.autoAdaptContainerSize)
                    return;

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

        private void SetAutoAdaptModeActive(bool isActive) =>
            model.autoAdaptContainerSize = isActive;
    }
}
