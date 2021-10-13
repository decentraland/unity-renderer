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
            int value = (int)newValue;
            if (value == Settings.i.qualitySettingsPresets.Length)
                return;
            
            QualitySettings preset = Settings.i.qualitySettingsPresets[value];
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
        
        private void SetupQualityPresetLabelsWithCustom()
        {
            List<string> presetNames = new List<string>();
            QualitySettings preset;
            for (int i = 0; i < Settings.i.qualitySettingsPresets.Length; i++)
            {
                preset = Settings.i.qualitySettingsPresets[i];
                presetNames.Add(preset.displayName);
            }
            presetNames.Add(TEXT_QUALITY_CUSTOM);
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
                    SetupQualityPresetLabels();
                    RaiseOnCurrentLabelChange(preset.displayName);
                    return i;
                }
            }

            SetupQualityPresetLabelsWithCustom();
            RaiseOnCurrentLabelChange(TEXT_QUALITY_CUSTOM);
            return Settings.i.qualitySettingsPresets.Length;
        }
    }
}