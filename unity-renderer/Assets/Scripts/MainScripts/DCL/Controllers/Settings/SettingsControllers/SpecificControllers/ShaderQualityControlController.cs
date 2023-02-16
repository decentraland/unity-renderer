using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using System;
using UnityEngine;
using ShaderQuality = DCL.SettingsCommon.QualitySettings.ShaderQuality;

namespace MainScripts.DCL.Controllers.Settings.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Shader Quality", fileName = "ShaderQualityControlController")]
    public class ShaderQualityControlController : SpinBoxSettingsControlController
    {
        [SerializeField] private OutlineScreenEffectFeature outlineScreenEffectFeature;

        private static readonly int GAUSSIAN_BLUR_KERNEL_SIZE = Shader.PropertyToID("gaussianBlurQuality");

        public override object GetStoredValue() =>
            (int)currentQualitySetting.shaderQuality;

        public override void UpdateSetting(object newValue)
        {
            OutlineScreenEffectFeature.OutlineSettings outlineSettings = outlineScreenEffectFeature.settings;
            currentQualitySetting.shaderQuality = (ShaderQuality)newValue;

            switch (currentQualitySetting.shaderQuality)
            {
                case ShaderQuality.LOW:
                    Shader.SetGlobalInt(GAUSSIAN_BLUR_KERNEL_SIZE, 4);
                    outlineSettings.blurSize = 1;
                    outlineSettings.blurSigma = 5;
                    outlineSettings.outlineThickness = 0.5f;
                    break;
                case ShaderQuality.MID:
                    Shader.SetGlobalInt(GAUSSIAN_BLUR_KERNEL_SIZE, 32);
                    outlineSettings.blurSize = 1.5f;
                    outlineSettings.blurSigma = 5.5f;
                    outlineSettings.outlineThickness = 0.75f;
                    break;
                case ShaderQuality.HIGH:
                    Shader.SetGlobalInt(GAUSSIAN_BLUR_KERNEL_SIZE, 64);
                    outlineSettings.blurSize = 2;
                    outlineSettings.blurSigma = 6;
                    outlineSettings.outlineThickness = 1;
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}
