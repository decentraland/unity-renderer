using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    /// <summary>
    /// MonoBehaviour that represents the view of a SPIN-BOX type CONTROL.
    /// </summary>
    public class SpinBoxSettingsControlView : SettingsControlView
    {
        [SerializeField] private SpinBoxPresetted spinBox;

        public SpinBoxPresetted spinBoxControl { get => spinBox; }

        public override void Initialize(SettingsControlModel controlConfig, SettingsControlController settingsControlController)
        {
            SetLabels(((SpinBoxControlModel)controlConfig).spinBoxLabels);

            base.Initialize(controlConfig, settingsControlController);
            settingsControlController.OnControlChanged(spinBox.value);

            spinBox.onValueChanged.AddListener(spinBoxValue =>
            {
                ApplySetting(spinBoxValue);
            });
        }

        public override void RefreshControl()
        {
            int newValue = (int)settingsControlController.GetStoredValue();
            if (spinBox.value != newValue)
                spinBox.value = newValue;
            else
                skipPostApplySettings = false;
        }

        /// <summary>
        /// Overrides the current list of available options for the spin-box.
        /// </summary>
        /// <param name="labels">New list of available options.</param>
        public void SetLabels(string[] labels)
        {
            if (labels.Length == 0)
                return;

            spinBox.SetLabels(labels);
            spinBox.SetValue(0);
        }
    }
}