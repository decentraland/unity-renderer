using System.Linq;
using DCL.SettingsControls;
using TMPro;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    public class DropdownControlView : SettingsControlView
    {
        [SerializeField] private TMP_Dropdown dropdown;
        
        private SpinBoxSettingsControlController spinBoxController;

        public override void Initialize(SettingsControlModel controlConfig, SettingsControlController settingsControlController)
        {
            //we use spinbox control model and control controller for compatibility
            SetLabels(((SpinBoxControlModel)controlConfig).spinBoxLabels);

            spinBoxController = (SpinBoxSettingsControlController)settingsControlController;
            spinBoxController.OnSetLabels += SetLabels;
            spinBoxController.OnCurrentLabelChange += SetOption;

            base.Initialize(controlConfig, spinBoxController);
            spinBoxController.UpdateSetting(dropdown.value);

            dropdown.onValueChanged.AddListener(spinBoxValue =>
            {
                ApplySetting(spinBoxValue);
            });
        }
        private void SetOption(string option)
        {
            var index = dropdown.options.FindIndex(o => o.text == option);
            if (index >= 0)
            {
                dropdown.value = index;
            }
        }
        private void SetLabels(string[] labels)
        {
            if (labels.Length == 0)
                return;

            dropdown.options = labels.Select(l => new TMP_Dropdown.OptionData(l)).ToList();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (spinBoxController != null)
            {
                spinBoxController.OnSetLabels -= SetLabels;
                spinBoxController.OnCurrentLabelChange -= SetOption;
            }
        }

        public override void RefreshControl()
        {
            base.RefreshControl();

            int newValue = (int)spinBoxController.GetStoredValue();
            if (dropdown.value != newValue)
                dropdown.value = newValue;
        }

    }
}