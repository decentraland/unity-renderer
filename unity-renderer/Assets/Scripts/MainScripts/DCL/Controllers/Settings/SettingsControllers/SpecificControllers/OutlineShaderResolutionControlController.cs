using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using System;
using UnityEngine;
using OutlineShaderQuality = DCL.SettingsCommon.QualitySettings.OutlineShaderQuality;

namespace MainScripts.DCL.Controllers.Settings.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Outline Shader Resolution", fileName = "OutlineShaderResolutionControlController")]
    public class OutlineShaderResolutionControlController : SpinBoxSettingsControlController
    {
        [SerializeField] private FloatVariable outlineResolution;

        public override object GetStoredValue() =>
            (int)currentQualitySetting.outlineShaderResolution;

        public override void UpdateSetting(object newValue)
        {
            currentQualitySetting.outlineShaderResolution = (OutlineShaderQuality)newValue;

            switch (currentQualitySetting.outlineShaderResolution)
            {
                case OutlineShaderQuality.LOW:
                    outlineResolution.Set(0.25f);
                    break;
                case OutlineShaderQuality.MID:
                    outlineResolution.Set(0.5f);
                    break;
                case OutlineShaderQuality.HIGH:
                    outlineResolution.Set(1);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}
