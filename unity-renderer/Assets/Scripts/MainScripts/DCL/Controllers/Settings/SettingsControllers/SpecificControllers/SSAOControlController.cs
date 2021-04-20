using System.Collections;
using System.Reflection;
using DCL.SettingsData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using QualitySettings = DCL.SettingsData.QualitySettings;

namespace DCL.SettingsControls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/SSAO Controller", fileName = "SSAOControlController")]
    public class SSAOControlController : SpinBoxSettingsControlController
    {
        private UniversalRenderPipelineAsset urpAsset = null;
        private ScriptableRendererFeature ssaoFeature;

        private object settings;
        private FieldInfo sourceField;
        private FieldInfo downsampleField;

        public override void Initialize()
        {
            base.Initialize();

            urpAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;

            ScriptableRenderer forwardRenderer = urpAsset.GetRenderer(0) as ScriptableRenderer;
            var featuresField = typeof(ScriptableRenderer).GetField("m_RendererFeatures", BindingFlags.NonPublic | BindingFlags.Instance);

            IList features = featuresField.GetValue(forwardRenderer) as IList;
            ssaoFeature = features[0] as ScriptableRendererFeature;

            FieldInfo settingsField = ssaoFeature.GetType().GetField("m_Settings", BindingFlags.NonPublic | BindingFlags.Instance);
            settings = settingsField.GetValue(ssaoFeature);

            sourceField = settings.GetType().GetField("Source", BindingFlags.NonPublic | BindingFlags.Instance);
            downsampleField = settings.GetType().GetField("Downsample", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public override object GetStoredValue()
        {
            return currentQualitySetting.ssaoQuality;
        }

        public override void UpdateSetting(object newValue)
        {
            int value = (int)newValue;
            switch ( value )
            {
                case (int)QualitySettings.SSAOQualityLevel.OFF:
                    ssaoFeature.SetActive(false);
                    break;
                case (int)QualitySettings.SSAOQualityLevel.LOW:
                    ssaoFeature.SetActive(true);
                    sourceField.SetValue(settings, 0);
                    downsampleField.SetValue(settings, true);
                    break;
                case (int)QualitySettings.SSAOQualityLevel.MID:
                    ssaoFeature.SetActive(true);
                    sourceField.SetValue(settings, 1);
                    downsampleField.SetValue(settings, true);
                    break;
                case (int)QualitySettings.SSAOQualityLevel.HIGH:
                    ssaoFeature.SetActive(true);
                    sourceField.SetValue(settings, 1);
                    downsampleField.SetValue(settings, false);
                    break;
            }

            currentQualitySetting.ssaoQuality = (QualitySettings.SSAOQualityLevel)value;
        }
    }
}