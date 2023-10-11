using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.SettingsPanelHUD.Controls
{
    /// <summary>
    /// MonoBehaviour that represents the view of a TOGGLE type CONTROL.
    /// </summary>
    public class ToggleSettingsControlView : SettingsControlView
    {
        [SerializeField] private Toggle toggle;

        public Toggle toggleControl { get => toggle; }

        private ToggleSettingsControlController toggleController;

        public override void Initialize(SettingsControlModel model, SettingsControlController controller)
        {
            toggleController = (ToggleSettingsControlController)controller;
            toggleController.ToggleValueChanged += OverrideToggleValue;

            base.Initialize(model, toggleController);
            toggleController.UpdateSetting(toggle.isOn);

            toggle.onValueChanged.AddListener(isOn =>
            {
                ApplySetting(isOn);
            });
        }

        protected override void OnDestroy()
        {
            if (toggleController != null)
                toggleController.ToggleValueChanged -= OverrideToggleValue;

            base.OnDestroy();
        }

        public override void RefreshControl()
        {
            base.RefreshControl();

            bool newValue = (bool)toggleController.GetStoredValue();
            if (toggle.isOn != newValue)
                toggle.isOn = newValue;
        }

        private void OverrideToggleValue(bool newValue) =>
            toggle.isOn = newValue;
    }
}
