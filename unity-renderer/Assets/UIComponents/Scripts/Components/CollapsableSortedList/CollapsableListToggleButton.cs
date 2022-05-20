using DCL.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace UIComponents.CollapsableSortedList
{
    public class CollapsableListToggleButton : BaseComponentView
    {
        [SerializeField] private bool toggleOnAwake;
        [SerializeField] private Button toggleButton;
        [SerializeField] private Transform toggleButtonIcon;
        [SerializeField] private RectTransform containerRectTransform;
        [SerializeField] private CollapsableListToggleButtonModel model;

        public override void Awake()
        {
            base.Awake();
            
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
        }

        private void Toggle()
        {
            Toggle(!containerRectTransform.gameObject.activeSelf);
        }
    }
}