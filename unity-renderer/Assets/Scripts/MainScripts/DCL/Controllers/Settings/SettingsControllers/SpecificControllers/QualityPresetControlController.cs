using System.Collections.Generic;
using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Quality Preset", fileName = "QualityPresetControlController")]
    public class QualityPresetControlController : SpinBoxSettingsControlController
    {
        public const string TEXT_QUALITY_CUSTOM = "Custom";

        public override void Initialize()
        {
            base.Initialize();

            SetupQualityPresetLabels();
        }

        public override object GetStoredValue() { return GetCurrentStoredValue(); }

        public override void UpdateSetting(object newValue)
        {
            QualitySettings preset = Settings.i.qualitySettingsPresets[(int)newValue];
            currentQualitySetting = preset;
        }

        private void SetupQualityPresetLabels()
        {
            List<string> presetNames = new List<string>();
            QualitySettings preset;
            for (int i = 0; i < Settings.i.qualitySettingsPresets.Length; i++)
            {
                preset = Settings.i.qualitySettingsPresets[i];
                presetNames.Add(preset.displayName);
            }

            RaiseOnOverrideIndicatorLabel(presetNames.ToArray());
        }

        private int GetCurrentStoredValue()
        {
            QualitySettings preset;
            for (int i = 0; i < Settings.i.qualitySettingsPresets.Length; i++)
            {
                preset = Settings.i.qualitySettingsPresets[i];
                if (preset.Equals(currentQualitySetting))
                {
                    RaiseOnCurrentLabelChange(preset.displayName);
                    return i;
                }
            }

            RaiseOnCurrentLabelChange(TEXT_QUALITY_CUSTOM);
            return 0;
        }
    }
}