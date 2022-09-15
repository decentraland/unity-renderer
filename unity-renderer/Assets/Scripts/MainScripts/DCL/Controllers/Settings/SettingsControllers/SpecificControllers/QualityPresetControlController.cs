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
            if (value != Settings.i.qualitySettingsPresets.Length)
                currentQualitySetting = Settings.i.qualitySettingsPresets[value];
        }

        private void SetupQualityPresetLabels(params string[] customs)
        {
            List<string> presetNames = new List<string>();

            for (int i = 0; i < Settings.i.qualitySettingsPresets.Length; i++)
                presetNames.Add(Settings.i.qualitySettingsPresets[i].displayName);

            presetNames.AddRange(customs);

            RaiseOnOverrideIndicatorLabel(presetNames.ToArray());
        }

        private int GetCurrentStoredValue()
        {
            for (int i = 0; i < Settings.i.qualitySettingsPresets.Length; i++)
            {
                QualitySettings preset = Settings.i.qualitySettingsPresets[i];

                if (preset.Equals(currentQualitySetting))
                {
                    SetupQualityPresetLabels();
                    RaiseOnCurrentLabelChange(preset.displayName);
                    return i;
                }
            }

            SetupQualityPresetLabels(TEXT_QUALITY_CUSTOM);
            RaiseOnCurrentLabelChange(TEXT_QUALITY_CUSTOM);
            return Settings.i.qualitySettingsPresets.Length;
        }
    }
}