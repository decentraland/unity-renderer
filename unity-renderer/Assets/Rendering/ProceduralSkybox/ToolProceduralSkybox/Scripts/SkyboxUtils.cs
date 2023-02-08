using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Skybox
{
    public static class SkyboxUtils
    {
        /// <summary>
        /// Time for one complete circle. In Hours. default 24
        /// </summary>
        public const float CYCLE_TIME = 24;
        public const float DOME_DEFAULT_SIZE = 50;

        public static float GetNormalizedDayTime(float timeOfTheDay)
        {
            float tTime = timeOfTheDay / SkyboxUtils.CYCLE_TIME;
            tTime = Mathf.Clamp(tTime, 0, 1);
            return tTime;
        }

        public static Quaternion Vector4ToQuaternion(Vector4 val) { return new Quaternion(val.x, val.y, val.z, val.w); }
    }

    public static class SkyboxShaderUtils
    {
        public const int TOTAL_SKYBOX_LAYERS = 5;

        private static Dictionary<string, int>[] shaderLayersProperties = null;
        public static readonly int LightTint = Shader.PropertyToID("_lightTint");
        public static readonly int LightDirection = Shader.PropertyToID("_lightDirection");
        public static readonly int SkyColor = Shader.PropertyToID("_skyColor");
        public static readonly int GroundColor = Shader.PropertyToID("_groundColor");
        public static readonly int HorizonColor = Shader.PropertyToID("_horizonColor");
        public static readonly int HorizonHeight = Shader.PropertyToID("_horizonHeight");
        public static readonly int HorizonWidth = Shader.PropertyToID("_horizonWidth");
        public static readonly int HorizonMask = Shader.PropertyToID("_horizonMask");
        public static readonly int HorizonMaskValues = Shader.PropertyToID("_horizonMaskValues");
        public static readonly int HorizonPlane = Shader.PropertyToID("_horizonPlane");
        public static readonly int HorizonPlaneValues = Shader.PropertyToID("_horizonPlaneValues");
        public static readonly int HorizonPlaneColor = Shader.PropertyToID("_horizonPlaneColor");
        public static readonly int HorizonPlaneHeight = Shader.PropertyToID("_horizonPlaneHeight");
        public static readonly int PlaneSmoothRange = Shader.PropertyToID("_smoothRange");
        public static readonly int HorizonLightIntensity = Shader.PropertyToID("_horizonLigthIntesity");
        public static readonly int FogIntensity = Shader.PropertyToID("_fogIntesity");
        public static readonly int Opacity = Shader.PropertyToID("_Opacity");

        static SkyboxShaderUtils() { CacheShaderProperties(); }

        static void CacheShaderProperties()
        {
            if (shaderLayersProperties != null) return;
            shaderLayersProperties = new Dictionary<string, int>[TOTAL_SKYBOX_LAYERS];
            for (int i = 0; i < TOTAL_SKYBOX_LAYERS; i++)
            {
                shaderLayersProperties[i] = new Dictionary<string, int>();

                shaderLayersProperties[i].Add("_layerType_", Shader.PropertyToID("_layerType_" + i));
                shaderLayersProperties[i].Add("_fadeTime_" , Shader.PropertyToID("_fadeTime_" + i));

                shaderLayersProperties[i].Add("_RenderDistance_" , Shader.PropertyToID("_RenderDistance_" + i));
                shaderLayersProperties[i].Add("_tex_" , Shader.PropertyToID("_tex_" + i));
                shaderLayersProperties[i].Add("_cubemap_" , Shader.PropertyToID("_cubemap_" + i));
                shaderLayersProperties[i].Add("_normals_" , Shader.PropertyToID("_normals_" + i));
                shaderLayersProperties[i].Add("_color_" , Shader.PropertyToID("_color_" + i));
                shaderLayersProperties[i].Add("_timeFrame_" , Shader.PropertyToID("_timeFrame_" + i));
                shaderLayersProperties[i].Add("_rowAndCollumns_" , Shader.PropertyToID("_rowAndCollumns_" + i));

                shaderLayersProperties[i].Add("_lightIntensity_" , Shader.PropertyToID("_lightIntensity_" + i));
                shaderLayersProperties[i].Add("_normalIntensity_" , Shader.PropertyToID("_normalIntensity_" + i));

                shaderLayersProperties[i].Add("_distortIntAndSize_" , Shader.PropertyToID("_distortIntAndSize_" + i));
                shaderLayersProperties[i].Add("_distortSpeedAndSharp_" , Shader.PropertyToID("_distortSpeedAndSharp_" + i));


                shaderLayersProperties[i].Add("_particlesMainParameters_" , Shader.PropertyToID("_particlesMainParameters_" + i));
                shaderLayersProperties[i].Add("_particlesSecondaryParameters_" , Shader.PropertyToID("_particlesSecondaryParameters_" + i));

                shaderLayersProperties[i].Add("_tilingAndOffset_" , Shader.PropertyToID("_tilingAndOffset_" + i ));
                shaderLayersProperties[i].Add("_speedAndRotation_" , Shader.PropertyToID("_speedAndRotation_" + i));
            }

        }

        public static int GetLayerProperty(string name, int layer)
        {
            if (layer >= TOTAL_SKYBOX_LAYERS)
            {
                throw new ArgumentException($"Maximum Skybox Layers is {TOTAL_SKYBOX_LAYERS}");
            }

            int propertyInt;
            if (shaderLayersProperties is not { Length: > 0 })
            {
                CacheShaderProperties();
            }

            if (shaderLayersProperties[layer].TryGetValue(name, out int val))
            {
                propertyInt = val;
            }
            else
            {
                propertyInt = Shader.PropertyToID(name);
                shaderLayersProperties[layer].Add(name, propertyInt);
            }

            return propertyInt;
        }

    }
}
