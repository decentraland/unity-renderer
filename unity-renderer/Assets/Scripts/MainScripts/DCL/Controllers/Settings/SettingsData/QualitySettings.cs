using System;
using DCL.Helpers;
using UnityEngine;

namespace DCL.SettingsCommon
{
    [Serializable]
    public struct QualitySettings
    {
        public enum ShaderQuality
        {
            LOW,
            MID,
            HIGH
        }

        public enum BaseResolution
        {
            BaseRes_Normal,
            BaseRes_Match_display
        }

        public enum SSAOQualityLevel
        {
            OFF,
            LOW,
            MID,
            HIGH
        }

        public enum ReflectionResolution
        {
            Res_256,
            Res_512,
            Res_1024,
            Res_2048
        }

        public string displayName;

        [Tooltip("Base resolution level")] public BaseResolution baseResolution;

        [Tooltip("Controls the global anti aliasing setting")]
        public UnityEngine.Rendering.Universal.MsaaQuality antiAliasing;

        [Tooltip(
            "Scales the camera render target allowing the game to render at a resolution different than native resolution. UI is always rendered at native resolution")]
        [Range(0.5f, 1)]
        public float renderScale;

        [Tooltip("If enabled the main light can be a shadow casting light")]
        public bool shadows;

        [Tooltip(
            "If enabled pipeline will perform shadow filterin. Otherwise all lights that cast shadows will fallback to perform a single shadow sample")]
        public bool softShadows;

        [Tooltip("Resolution of the main light shadowmap texture")]
        public UnityEngine.Rendering.Universal.ShadowResolution shadowResolution;

        [Tooltip("Camera Far")]
        [Range(40, 500)]
        public float cameraDrawDistance;

        [Tooltip("Enable bloom post process")] public bool bloom;

        [Tooltip("Enable 30 FPS capping for more stable framerate")]
        public bool fpsCap;

        [Tooltip("Shadow Distance")]
        [Range(30, 100)]
        public float shadowDistance;

        [Tooltip("Enable culling for detail objects in the viewport.")]
        public bool enableDetailObjectCulling;

        [Tooltip(
            "If detail object culling is ON, this slider determines the relative size of culled objects from tiny to big. Bigger values gives better performance, but more objects will be hidden.")]
        [Range(0, 100)]
        public float detailObjectCullingLimit;

        [Tooltip("SSAO quality level")] public SSAOQualityLevel ssaoQuality;

        [Tooltip("Amount of HQ Avatars visible at any time")]
        public int maxHQAvatars;

        public ReflectionResolution reflectionResolution;

        public ShaderQuality shaderQuality;
    }
}
