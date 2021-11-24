using System.Reflection;
using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/SoftShadows", fileName = "SoftShadowsControlController")]
    public class SoftShadowsControlController : ToggleSettingsControlController
    {
        private UniversalRenderPipelineAsset lightweightRenderPipelineAsset = null;
        private FieldInfo lwrpaSoftShadowField = null;

        public override void Initialize()
        {
            base.Initialize();

            lightweightRenderPipelineAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;

            if (lightweightRenderPipelineAsset == null)
                return;

            lwrpaSoftShadowField = lightweightRenderPipelineAsset.GetType().GetField("m_SoftShadowsSupported", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public override object GetStoredValue() { return currentQualitySetting.softShadows; }

        public override void UpdateSetting(object newValue)
        {
            currentQualitySetting.softShadows = (bool)newValue;

            if (lightweightRenderPipelineAsset != null)
                lwrpaSoftShadowField?.SetValue(lightweightRenderPipelineAsset, currentQualitySetting.softShadows);

            if (SceneReferences.i.environmentLight)
            {
                LightShadows shadowType = LightShadows.None;

                if (currentQualitySetting.shadows)
                    shadowType = currentQualitySetting.softShadows ? LightShadows.Soft : LightShadows.Hard;

                SceneReferences.i.environmentLight.shadows = shadowType;
            }
        }
    }
}