using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DCL.SettingsControls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Rendering Scale", fileName = "RenderingScaleControlController")]
    public class RenderingScaleControlController : SliderSettingsControlController
    {
        private UniversalRenderPipelineAsset lightweightRenderPipelineAsset = null;

        public override void Initialize()
        {
            base.Initialize();

            if (lightweightRenderPipelineAsset == null)
                lightweightRenderPipelineAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
        }

        public override object GetStoredValue()
        {
            return currentQualitySetting.renderScale;
        }

        public override void UpdateSetting(object newValue)
        {
            currentQualitySetting.renderScale = (float)newValue;

            if (lightweightRenderPipelineAsset != null)
            {
                lightweightRenderPipelineAsset.renderScale = currentQualitySetting.renderScale;
            }

            RaiseOnIndicatorLabelChange(currentQualitySetting.renderScale.ToString("0.0"));
        }
    }
}