using DCL.Helpers;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIComponents.CollapsableSortedList
{
    public class CollapsableListToggleButton : BaseComponentView
    {
        [SerializeField] private bool toggleOnAwake;
        [SerializeField] private Button toggleButton;
        [SerializeField] private Transform toggleButtonIcon;
        [SerializeField] private Image toggleImage;
        [SerializeField] private RectTransform containerRectTransform;
        [SerializeField] private CollapsableListToggleButtonModel model;
        [SerializeField] private Color disabledColor;

        public event Action<bool> OnToggled;

        private Color originalColor;

        public override void Awake()
        {
            base.Awake();

            originalColor = toggleImage?.color ?? Color.black;
            toggleButton.onClick.AddListener(Toggle);

            if (toggleOnAwake)
                Toggle();
        }

        public override void RefreshControl()
        {
            Toggle(model.isToggled);
        }

        public void Toggle(bool toggled)
        {
            containerRectTransform.gameObject.SetActive(toggled);
            var absScale = Mathf.Abs(toggleButtonIcon.localScale.y);
            var scale = toggled ? absScale : -absScale;
            toggleButtonIcon.localScale = new Vector3(toggleButtonIcon.localScale.x, scale, 1f);
            Utils.ForceRebuildLayoutImmediate(containerRectTransform);
            model.isToggled = toggled;
            OnToggled?.Invoke(toggled);
        }

        public void SetInteractability(bool interactable)
        {
            toggleButton.interactable = interactable;
            toggleImage.color = interactable ? originalColor : disabledColor;
        }

        private void Toggle()
        {
            Toggle(!containerRectTransform.gameObject.activeSelf);
        }
    }
}
