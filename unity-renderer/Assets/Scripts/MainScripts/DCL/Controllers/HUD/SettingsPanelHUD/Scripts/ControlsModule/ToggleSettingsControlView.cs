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

            base.Initialize(model, toggleController);
            toggleController.UpdateSetting(toggle.isOn);

            toggle.onValueChanged.AddListener(isOn =>
            {
                ApplySetting(isOn);
            });
        }

        public override void RefreshControl()
        {
            base.RefreshControl();

            bool newValue = (bool)toggleController.GetStoredValue();
            if (toggle.isOn != newValue)
                toggle.isOn = newValue;
        }
    }
}