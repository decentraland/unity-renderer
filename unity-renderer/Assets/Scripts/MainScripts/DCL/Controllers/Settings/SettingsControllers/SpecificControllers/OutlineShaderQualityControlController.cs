using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using System;
using UnityEngine;
using OutlineShaderQuality = DCL.SettingsCommon.QualitySettings.OutlineShaderQuality;

namespace MainScripts.DCL.Controllers.Settings.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Outline Shader Quality", fileName = "OutlineShaderQualityControlController")]
    public class OutlineShaderQualityControlController : SpinBoxSettingsControlController
    {
        private static readonly int GAUSSIAN_BLUR_KERNEL_SIZE = Shader.PropertyToID("gaussianBlurKernelSize");

        public override object GetStoredValue() =>
            (int)currentQualitySetting.outlineShaderQuality;

        public override void UpdateSetting(object newValue)
        {
            currentQualitySetting.outlineShaderQuality = (OutlineShaderQuality)newValue;

            switch (currentQualitySetting.outlineShaderQuality)
            {
                case OutlineShaderQuality.LOW:
                    Shader.SetGlobalInt(GAUSSIAN_BLUR_KERNEL_SIZE, 8);
                    break;
                case OutlineShaderQuality.MID:
                    Shader.SetGlobalInt(GAUSSIAN_BLUR_KERNEL_SIZE, 32);
                    break;
                case OutlineShaderQuality.HIGH:
                    Shader.SetGlobalInt(GAUSSIAN_BLUR_KERNEL_SIZE, 64);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}
