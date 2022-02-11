using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Skybox
{
    public static class SkyboxShaderUtils
    {
        public static Dictionary<string, int> shaderLayersProperties;
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

        static SkyboxShaderUtils() { CacheShaderProperties(); }

        static void CacheShaderProperties()
        {
            shaderLayersProperties = new Dictionary<string, int>();
            for (int i = 0; i < 5; i++)
            {
                shaderLayersProperties.Add("_layerType_" + i, Shader.PropertyToID("_layerType_" + i));
                shaderLayersProperties.Add("_fadeTime_" + i, Shader.PropertyToID("_fadeTime_" + i));

                shaderLayersProperties.Add("_RenderDistance_" + i, Shader.PropertyToID("_RenderDistance_" + i));
                shaderLayersProperties.Add("_tex_" + i, Shader.PropertyToID("_tex_" + i));
                shaderLayersProperties.Add("_cubemap_" + i, Shader.PropertyToID("_cubemap_" + i));
                shaderLayersProperties.Add("_normals_" + i, Shader.PropertyToID("_normals_" + i));
                shaderLayersProperties.Add("_color_" + i, Shader.PropertyToID("_color_" + i));
                shaderLayersProperties.Add("_rowAndCollumns_" + i, Shader.PropertyToID("_rowAndCollumns_" + i));

                shaderLayersProperties.Add("_lightIntensity_" + i, Shader.PropertyToID("_lightIntensity_" + i));

                shaderLayersProperties.Add("_distortIntAndSize_" + i, Shader.PropertyToID("_distortIntAndSize_" + i));
                shaderLayersProperties.Add("_distortSpeedAndSharp_" + i, Shader.PropertyToID("_distortSpeedAndSharp_" + i));


                shaderLayersProperties.Add("_particlesMainParameters_" + i, Shader.PropertyToID("_particlesMainParameters_" + i));
                shaderLayersProperties.Add("_particlesSecondaryParameters_" + i, Shader.PropertyToID("_particlesSecondaryParameters_" + i));

                shaderLayersProperties.Add("_tilingAndOffset_" + i, Shader.PropertyToID("_tilingAndOffset_" + i));
                shaderLayersProperties.Add("_speedAndRotation_" + i, Shader.PropertyToID("_speedAndRotation_" + i));
            }

        }

        public static int GetLayerProperty(string name)
        {
            int propertyInt;
            if (shaderLayersProperties == null || shaderLayersProperties.Count <= 0)
            {
                CacheShaderProperties();
            }

            if (!shaderLayersProperties.ContainsKey(name))
            {
                propertyInt = Shader.PropertyToID(name);
                shaderLayersProperties.Add(name, propertyInt);
            }
            else
            {
                propertyInt = shaderLayersProperties[name];
            }

            return propertyInt;
        }

    }
}