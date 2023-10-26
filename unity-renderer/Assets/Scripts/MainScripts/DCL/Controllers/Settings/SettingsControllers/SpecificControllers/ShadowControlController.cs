using System.Reflection;
using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Shadow", fileName = "ShadowControlController")]
    public class ShadowControlController : ToggleSettingsControlController
    {
        private UniversalRenderPipelineAsset lightweightRenderPipelineAsset = null;
        private FieldInfo lwrpaShadowField = null;

        public override void Initialize()
        {
            base.Initialize();

            lightweightRenderPipelineAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;

            if (lightweightRenderPipelineAsset == null)
                return;

            lwrpaShadowField = lightweightRenderPipelineAsset.GetType().GetField("m_MainLightShadowsSupported", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public override object GetStoredValue()
        {
            // TODO: force shadows always enabled on webgl until we fix the problem when rendering models with shadows disabled
#if UNITY_WEBGL && !UNITY_EDITOR
            return true;
#else
            return currentQualitySetting.shadows;
#endif
        }

        public override void UpdateSetting(object newValue)
        {
            // TODO: force shadows always enabled on webgl until we fix the problem when rendering models with shadows disabled
#if UNITY_WEBGL && !UNITY_EDITOR
            newValue = true;
#endif

            currentQualitySetting.shadows = (bool)newValue;

            if (lightweightRenderPipelineAsset != null)
                lwrpaShadowField?.SetValue(lightweightRenderPipelineAsset, currentQualitySetting.shadows);

            if (SceneReferences.i.environmentLight)
            {
                LightShadows shadowType = LightShadows.None;

                if (currentQualitySetting.shadows)
                    shadowType = currentQualitySetting.shadows ? LightShadows.Soft : LightShadows.Hard;

                SceneReferences.i.environmentLight.shadows = shadowType;
            }

            CommonSettingsScriptableObjects.shadowsDisabled.Set(!currentQualitySetting.shadows);
        }
    }
}
