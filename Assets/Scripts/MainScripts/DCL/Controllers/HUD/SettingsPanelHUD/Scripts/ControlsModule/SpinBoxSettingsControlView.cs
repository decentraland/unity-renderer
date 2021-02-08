using DCL.SettingsControls;
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

        private SpinBoxSettingsControlController spinBoxController;

        public override void Initialize(SettingsControlModel controlConfig, SettingsControlController settingsControlController)
        {
            SetLabels(((SpinBoxControlModel)controlConfig).spinBoxLabels);

            spinBoxController = (SpinBoxSettingsControlController)settingsControlController;
            spinBoxController.OnSetLabels += SetLabels;
            spinBoxController.OnCurrentLabelChange += spinBox.OverrideCurrentLabel;

            base.Initialize(controlConfig, spinBoxController);
            spinBoxController.UpdateSetting(spinBox.value);

            spinBox.onValueChanged.AddListener(spinBoxValue =>
            {
                ApplySetting(spinBoxValue);
            });
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (spinBoxController != null)
            {
                spinBoxController.OnSetLabels -= SetLabels;
                spinBoxController.OnCurrentLabelChange -= spinBox.OverrideCurrentLabel;
            }
        }

        public override void RefreshControl()
        {
            base.RefreshControl();

            int newValue = (int)spinBoxController.GetStoredValue();
            if (spinBox.value != newValue)
                spinBox.value = newValue;
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