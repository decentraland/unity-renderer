using System.Collections.Generic;
using System.Linq;
using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using TMPro;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    public class DropdownControlView : SettingsControlView
    {
        [SerializeField] private TMP_Dropdown dropdown;
        
        private SpinBoxSettingsControlController spinBoxController;
        private PointerClickEventInterceptor pointerClick;
        
        private void Awake()
        {
            pointerClick = dropdown.GetComponent<PointerClickEventInterceptor>();
        }

        public override void Initialize(SettingsControlModel model, SettingsControlController controller)
        {
            // we use spinbox control model and control controller for compatibility
            SetLabels(((SpinBoxControlModel)model).spinBoxLabels);

            spinBoxController = (SpinBoxSettingsControlController)controller;
            spinBoxController.OnSetLabels += SetLabels;
            spinBoxController.OnCurrentLabelChange += SetOption;

            base.Initialize(model, spinBoxController);
            spinBoxController.UpdateSetting(dropdown.value);

            dropdown.onValueChanged.AddListener(spinBoxValue =>
            {
                ApplySetting(spinBoxValue);
            });
            
            pointerClick.PointerClicked += spinBoxController.OnPointerClicked;
        }

        private void SetOption(string option)
        {
            dropdown.captionText.text = option;
        }
        private void SetLabels(string[] labels)
        {
            if (labels.Length == 0)
                return;

            dropdown.ClearOptions();
            List<TMP_Dropdown.OptionData> dropdownOptions = labels.Select(l => new TMP_Dropdown.OptionData(l)).ToList();
            foreach (TMP_Dropdown.OptionData data in dropdownOptions)
            {
                dropdown.options.Add(data);
            }
            
            dropdown.Hide();
            dropdown.RefreshShownValue();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            pointerClick.PointerClicked -= spinBoxController.OnPointerClicked;

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