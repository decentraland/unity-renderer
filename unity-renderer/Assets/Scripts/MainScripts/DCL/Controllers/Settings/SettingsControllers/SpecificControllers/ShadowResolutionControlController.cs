using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DCL.SettingsControls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Shadow Resolution", fileName = "ShadowResolutionControlController")]
    public class ShadowResolutionControlController : SpinBoxSettingsControlController
    {
        private UniversalRenderPipelineAsset lightweightRenderPipelineAsset = null;
        private FieldInfo lwrpaShadowResolutionField = null;

        public override void Initialize()
        {
            base.Initialize();

            lightweightRenderPipelineAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;

            if (lightweightRenderPipelineAsset == null) return;

            lwrpaShadowResolutionField = lightweightRenderPipelineAsset.GetType().GetField("m_MainLightShadowmapResolution", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public override object GetStoredValue()
        {
            return (int)Mathf.Log((int)currentQualitySetting.shadowResolution, 2) - 8;
        }

        public override void UpdateSetting(object newValue)
        {
            currentQualitySetting.shadowResolution = (UnityEngine.Rendering.Universal.ShadowResolution)(256 << (int)newValue);

            if (lightweightRenderPipelineAsset != null)
                lwrpaShadowResolutionField?.SetValue(lightweightRenderPipelineAsset, currentQualitySetting.shadowResolution);
        }
    }
}