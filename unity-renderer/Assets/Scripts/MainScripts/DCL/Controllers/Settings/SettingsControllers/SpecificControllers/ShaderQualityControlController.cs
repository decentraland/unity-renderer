using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using System;
using UnityEngine;
using ShaderQuality = DCL.SettingsCommon.QualitySettings.ShaderQuality;

namespace MainScripts.DCL.Controllers.Settings.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Shader Quality", fileName = "ShaderQualityControlController")]
    public class ShaderQualityControlController : SpinBoxSettingsControlController
    {
        [SerializeField] private FloatVariable blurSize;
        [SerializeField] private FloatVariable blurSigma;
        [SerializeField] private FloatVariable outlineThickness;

        private static readonly int GAUSSIAN_BLUR_KERNEL_SIZE = Shader.PropertyToID("gaussianBlurQuality");

        public override object GetStoredValue() =>
            (int)currentQualitySetting.shaderQuality;

        public override void UpdateSetting(object newValue)
        {
            currentQualitySetting.shaderQuality = (ShaderQuality)newValue;

            switch (currentQualitySetting.shaderQuality)
            {
                case ShaderQuality.LOW:
                    Shader.SetGlobalInt(GAUSSIAN_BLUR_KERNEL_SIZE, 0);
                    blurSize.Set(1);
                    blurSigma.Set(5);
                    outlineThickness.Set(0.5f);
                    break;
                case ShaderQuality.MID:
                    Shader.SetGlobalInt(GAUSSIAN_BLUR_KERNEL_SIZE, 1);
                    blurSize.Set(1.5f);
                    blurSigma.Set(5.5f);
                    outlineThickness.Set(0.75f);
                    break;
                case ShaderQuality.HIGH: // old production values
                    Shader.SetGlobalInt(GAUSSIAN_BLUR_KERNEL_SIZE, 2);
                    blurSize.Set(2);
                    blurSigma.Set(6);
                    outlineThickness.Set(1f);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}
