using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DCL.SettingsControls
{

    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/AntiAliasing", fileName = "AntiAliasingControlController")]
    public class AntiAliasingControlController : SliderSettingsControlController
    {
        public const string TEXT_OFF = "OFF";

        private UniversalRenderPipelineAsset lightweightRenderPipelineAsset = null;

        public override void Initialize()
        {
            base.Initialize();

            if (lightweightRenderPipelineAsset == null)
                lightweightRenderPipelineAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
        }

        public override object GetStoredValue()
        {
            float antiAliasingValue =
                currentQualitySetting.antiAliasing == MsaaQuality.Disabled
                    ? 0
                    : ((int)currentQualitySetting.antiAliasing >> 2) + 1;

            return antiAliasingValue;
        }

        public override void UpdateSetting(object newValue)
        {
            float newFloatValue = (float)newValue;

            int antiAliasingValue = 1 << (int)newFloatValue;
            currentQualitySetting.antiAliasing = (MsaaQuality)antiAliasingValue;

            if (lightweightRenderPipelineAsset != null)
                lightweightRenderPipelineAsset.msaaSampleCount = antiAliasingValue;

            if (newFloatValue == 0)
                RaiseOnIndicatorLabelChange(TEXT_OFF);
            else
                RaiseOnIndicatorLabelChange(antiAliasingValue.ToString("0x"));
        }
    }
}