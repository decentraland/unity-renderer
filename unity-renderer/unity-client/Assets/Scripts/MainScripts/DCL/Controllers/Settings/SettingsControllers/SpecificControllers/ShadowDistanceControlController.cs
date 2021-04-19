using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DCL.SettingsControls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Shadow Distance", fileName = "ShadowDistanceControlController")]
    public class ShadowDistanceControlController : SliderSettingsControlController
    {
        private UniversalRenderPipelineAsset lightweightRenderPipelineAsset = null;

        public override void Initialize()
        {
            base.Initialize();

            lightweightRenderPipelineAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
        }

        public override object GetStoredValue()
        {
            return currentQualitySetting.shadowDistance;
        }

        public override void UpdateSetting(object newValue)
        {
            currentQualitySetting.shadowDistance = (float)newValue;

            if (lightweightRenderPipelineAsset)
                lightweightRenderPipelineAsset.shadowDistance = currentQualitySetting.shadowDistance;
        }
    }
}