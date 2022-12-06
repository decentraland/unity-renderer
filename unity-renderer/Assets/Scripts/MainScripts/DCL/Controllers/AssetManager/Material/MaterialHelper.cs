using DCL.Helpers;
using UnityEngine;
using UnityEngine.Rendering;

namespace DCL
{
    internal static class BasicMaterial
    {
        private static readonly string MATERIAL_PATH = "Materials/BasicShapeMaterial";

        public static Material Create()
        {
            return new Material(Utils.EnsureResourcesMaterial(MATERIAL_PATH));
        }

        public static void SetUp(Material material, float alphaTest)
        {
            material.EnableKeyword("_ALPHATEST_ON");
            material.SetInt(ShaderUtils.ZWrite, 1);
            material.SetFloat(ShaderUtils.AlphaClip, 1);
            material.SetFloat(ShaderUtils.Cutoff, alphaTest);
            material.renderQueue = (int)RenderQueue.AlphaTest;
        }
    }

    internal static class PBRMaterial
    {
        private static readonly string MATERIAL_PATH = "Materials/ShapeMaterial";

        public static Material Create()
        {
            return new Material(Utils.EnsureResourcesMaterial(MATERIAL_PATH));
        }

        public static void SetUpColors(Material material, Color albedo, Color emissive, Color reflectivity, float emissiveIntensity)
        {
            material.SetColor(ShaderUtils.BaseColor, albedo);

            if (emissive != Color.clear && emissive != Color.black)
            {
                material.EnableKeyword("_EMISSION");
            }

            material.SetColor(ShaderUtils.EmissionColor, emissive * emissiveIntensity);
            material.SetColor(ShaderUtils.SpecColor, reflectivity);
        }

        public static void SetUpProps(Material material, float metallic, float roughness, float glossiness,
            float specularIntensity, float directIntensity)
        {
            material.SetFloat(ShaderUtils.Metallic, metallic);
            material.SetFloat(ShaderUtils.Smoothness, 1 - roughness);
            material.SetFloat(ShaderUtils.EnvironmentReflections, glossiness);
            material.SetFloat(ShaderUtils.SpecularHighlights, specularIntensity * directIntensity);
        }

        public static void SetUpTransparency(Material material, AssetPromise_Material_Model.TransparencyMode transparencyMode,
            AssetPromise_Material_Model.Texture? alphaTexture, Color albedoColor, float alphaTest)
        {
            // Reset shader keywords
            material.DisableKeyword("_ALPHATEST_ON"); // Cut Out Transparency
            material.DisableKeyword("_ALPHABLEND_ON"); // Fade Transparency
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON"); // Transparent

            if (transparencyMode == AssetPromise_Material_Model.TransparencyMode.Auto)
            {
                if (alphaTexture != null || albedoColor.a < 1f) //AlphaBlend
                {
                    transparencyMode = AssetPromise_Material_Model.TransparencyMode.AlphaBlend;
                }
                else // Opaque
                {
                    transparencyMode = AssetPromise_Material_Model.TransparencyMode.Opaque;
                }
            }

            switch (transparencyMode)
            {
                case AssetPromise_Material_Model.TransparencyMode.Opaque:
                    material.renderQueue = (int)RenderQueue.Geometry;
                    material.SetFloat(ShaderUtils.AlphaClip, 0);
                    break;
                case AssetPromise_Material_Model.TransparencyMode.AlphaTest: // ALPHATEST
                    material.EnableKeyword("_ALPHATEST_ON");

                    material.SetInt(ShaderUtils.SrcBlend, (int)BlendMode.One);
                    material.SetInt(ShaderUtils.DstBlend, (int)BlendMode.Zero);
                    material.SetInt(ShaderUtils.ZWrite, 1);
                    material.SetFloat(ShaderUtils.AlphaClip, 1);
                    material.SetFloat(ShaderUtils.Cutoff, alphaTest);
                    material.SetInt(ShaderUtils.Surface, 0);
                    material.renderQueue = (int)RenderQueue.AlphaTest;
                    break;
                case AssetPromise_Material_Model.TransparencyMode.AlphaBlend: // ALPHABLEND
                    material.EnableKeyword("_ALPHABLEND_ON");

                    material.SetInt(ShaderUtils.SrcBlend, (int)BlendMode.SrcAlpha);
                    material.SetInt(ShaderUtils.DstBlend, (int)BlendMode.OneMinusSrcAlpha);
                    material.SetInt(ShaderUtils.ZWrite, 0);
                    material.SetFloat(ShaderUtils.AlphaClip, 0);
                    material.renderQueue = (int)RenderQueue.Transparent;
                    material.SetInt(ShaderUtils.Surface, 1);
                    break;
                case AssetPromise_Material_Model.TransparencyMode.AlphaTestAndAlphaBlend:
                    material.EnableKeyword("_ALPHAPREMULTIPLY_ON");

                    material.SetInt(ShaderUtils.SrcBlend, (int)BlendMode.One);
                    material.SetInt(ShaderUtils.DstBlend, (int)BlendMode.OneMinusSrcAlpha);
                    material.SetInt(ShaderUtils.ZWrite, 0);
                    material.SetFloat(ShaderUtils.AlphaClip, 1);
                    material.renderQueue = (int)RenderQueue.Transparent;
                    material.SetInt(ShaderUtils.Surface, 1);
                    break;
            }
        }
    }
}