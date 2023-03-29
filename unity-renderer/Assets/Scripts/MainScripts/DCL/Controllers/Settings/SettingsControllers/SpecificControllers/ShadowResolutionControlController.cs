using System.Reflection;
using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Shadow Resolution", fileName = "ShadowResolutionControlController")]
    public class ShadowResolutionControlController : SpinBoxSettingsControlController
    {
        private const int LOG2_256 = 8; // log2(256), where 256 is the lowest Resolution in Unity (it goes [256, 512, 1024,...])

        private UniversalRenderPipelineAsset lightweightRenderPipelineAsset;
        private FieldInfo lwrpaShadowResolutionField;

        public override void Initialize()
        {
            base.Initialize();

            lightweightRenderPipelineAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;

            if (lightweightRenderPipelineAsset == null)
                return;

            lwrpaShadowResolutionField = lightweightRenderPipelineAsset.GetType().GetField("m_MainLightShadowmapResolution", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public override object GetStoredValue() =>
            (int)Mathf.Log((int)currentQualitySetting.shadowResolution, 2) - LOG2_256;

        public override void UpdateSetting(object newValue)
        {
            currentQualitySetting.shadowResolution = (UnityEngine.Rendering.Universal.ShadowResolution)(256 << (int)newValue);

            if (lightweightRenderPipelineAsset != null)
                lwrpaShadowResolutionField?.SetValue(lightweightRenderPipelineAsset, currentQualitySetting.shadowResolution);
        }
    }
}
