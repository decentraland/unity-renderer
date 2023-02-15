using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using System;
using UnityEngine;
using OutlineShaderQuality = DCL.SettingsCommon.QualitySettings.OutlineShaderQuality;

namespace MainScripts.DCL.Controllers.Settings.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Outline Shader Quality", fileName = "OutlineShaderQualityControlController")]
    public class OutlineShaderQualityControlController : SpinBoxSettingsControlController
    {
        [SerializeField] private FloatVariable blurSize;
        [SerializeField] private FloatVariable blurSigma;
        [SerializeField] private FloatVariable outlineThickness;

        private static readonly int GAUSSIAN_BLUR_KERNEL_SIZE = Shader.PropertyToID("gaussianBlurQuality");

        public override object GetStoredValue() =>
            (int)currentQualitySetting.outlineShaderQuality;

        public override void UpdateSetting(object newValue)
        {
            currentQualitySetting.outlineShaderQuality = (OutlineShaderQuality)newValue;

            switch (currentQualitySetting.outlineShaderQuality)
            {
                case OutlineShaderQuality.LOW:
                    Shader.SetGlobalInt(GAUSSIAN_BLUR_KERNEL_SIZE, 0);
                    blurSize.Set(1);
                    blurSigma.Set(5);
                    outlineThickness.Set(0.5f);

                    //blurSize.Set(2);
                    //blurSigma.Set(7);
                    //outlineThickness.Set(1.5f);
                    break;
                case OutlineShaderQuality.MID:
                    Shader.SetGlobalInt(GAUSSIAN_BLUR_KERNEL_SIZE, 1);
                    blurSize.Set(1.5f);
                    blurSigma.Set(5.5f);
                    outlineThickness.Set(0.75f);

                    //blurSize.Set(1.5f);
                    //blurSigma.Set(6);
                    //outlineThickness.Set(1f);
                    break;
                case OutlineShaderQuality.HIGH: // old production values
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
