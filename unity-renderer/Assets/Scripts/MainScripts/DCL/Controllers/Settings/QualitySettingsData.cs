using UnityEngine;
using System;
using DCL.Helpers;
using UnityEngine.Serialization;

namespace DCL.SettingsData
{
    [CreateAssetMenu(fileName = "QualitySettings", menuName = "QualitySettings")]
    public class QualitySettingsData : ScriptableObject
    {
        [SerializeField] int defaultPresetIndex = 0;
        [SerializeField] QualitySettings[] settings = null;

        public QualitySettings this[int i]
        {
            get { return settings[i]; }
        }

        public int Length
        {
            get { return settings.Length; }
        }

        public QualitySettings defaultPreset
        {
            get { return settings[defaultPresetIndex]; }
        }

        public int defaultIndex
        {
            get { return defaultPresetIndex; }
        }

        public void Set(QualitySettings[] newSettings)
        {
            settings = newSettings;
        }
    }

    [Serializable]
    public struct QualitySettings
    {
        public enum BaseResolution
        {
            BaseRes_720,
            BaseRes_1080,
            BaseRes_Unlimited
        }

        public enum SSAOQualityLevel
        {
            OFF,
            LOW,
            MID,
            HIGH
        }

        public string displayName;

        [Tooltip("Base resolution level")]
        public BaseResolution baseResolution;

        [Tooltip("Controls the global anti aliasing setting")]
        public UnityEngine.Rendering.Universal.MsaaQuality antiAliasing;

        [Tooltip("Scales the camera render target allowing the game to render at a resolution different than native resolution. UI is always rendered at native resolution")] [Range(0.5f, 1)]
        public float renderScale;

        [Tooltip("If enabled the main light can be a shadow casting light")]
        public bool shadows;

        [Tooltip("If enabled pipeline will perform shadow filterin. Otherwise all lights that cast shadows will fallback to perform a single shadow sample")]
        public bool softShadows;

        [Tooltip("Resolution of the main light shadowmap texture")]
        public UnityEngine.Rendering.Universal.ShadowResolution shadowResolution;

        [Tooltip("Camera Far")] [Range(40, 100)]
        public float cameraDrawDistance;

        [Tooltip("Enable bloom post process")]
        public bool bloom;

        [Tooltip("Enable 30 FPS capping for more stable framerate")]
        public bool fpsCap;

        [Tooltip("Enable color grading post process")]
        public bool colorGrading;

        [Tooltip("Shadow Distance")] [Range(30, 100)]
        public float shadowDistance;

        [Tooltip("Enable culling for detail objects in the viewport.")]
        public bool enableDetailObjectCulling;

        [Tooltip("If detail object culling is ON, this slider determines the relative size of culled objects from tiny to big. Bigger values gives better performance, but more objects will be hidden.")] [Range(0, 100)]
        public float detailObjectCullingLimit;

        [Tooltip("SSAO quality level")]
        public SSAOQualityLevel ssaoQuality;

        public bool Equals(QualitySettings otherSetting)
        {
            // The precision is set to 1 because the wholeNumbers setting
            // in the slider can clamp the values, thus we can have a 0 > n > 1 precision error.
            const float comparePrecision = 1.0f;

            if (baseResolution != otherSetting.baseResolution) return false;
            if (antiAliasing != otherSetting.antiAliasing) return false;
            if (Mathf.Abs(renderScale - otherSetting.renderScale) > 0.001f) return false;
            if (shadows != otherSetting.shadows) return false;
            if (softShadows != otherSetting.softShadows) return false;
            if (shadowResolution != otherSetting.shadowResolution) return false;
            if (!Utils.CompareFloats(cameraDrawDistance, otherSetting.cameraDrawDistance, comparePrecision)) return false;
            if (bloom != otherSetting.bloom) return false;
            if (fpsCap != otherSetting.fpsCap) return false;
            if (colorGrading != otherSetting.colorGrading) return false;
            if (!Utils.CompareFloats(shadowDistance, otherSetting.shadowDistance, comparePrecision)) return false;
            if (enableDetailObjectCulling != otherSetting.enableDetailObjectCulling) return false;
            if (!Utils.CompareFloats(detailObjectCullingLimit, otherSetting.detailObjectCullingLimit, comparePrecision)) return false;
            if (ssaoQuality != otherSetting.ssaoQuality) return false;

            return true;
        }
    }
}