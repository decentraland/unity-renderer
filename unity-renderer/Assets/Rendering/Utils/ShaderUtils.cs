using UnityEngine;
using UnityEngine.Rendering;

namespace DCL.Helpers
{
    public static class ShaderUtils
    {
        //Toon shader properties
        public static readonly int LightDir = Shader.PropertyToID("_LightDir");
        public static readonly int LightColor = Shader.PropertyToID("_LightColor");
        public static readonly int TintColor = Shader.PropertyToID("_TintColor");

        public static readonly int GlossMatCap = Shader.PropertyToID("_GlossMatCap");
        public static readonly int FresnelMatCap = Shader.PropertyToID("_FresnelMatCap");
        public static readonly int MatCap = Shader.PropertyToID("_MatCap");
        public static readonly int DitherFade = Shader.PropertyToID("_DitherFade");

        //Lit properties
        public static readonly int SpecGlossMap = Shader.PropertyToID("_SpecGlossMap");
        public static readonly int SmoothnessTextureChannel = Shader.PropertyToID("_SmoothnessTextureChannel");
        public static readonly int SpecColor = Shader.PropertyToID("_SpecColor");
        public static readonly int GlossMapScale = Shader.PropertyToID("_GlossMapScale");
        public static readonly int Glossiness = Shader.PropertyToID("_Glossiness");
        public static readonly int MetallicGlossMap = Shader.PropertyToID("_MetallicGlossMap");

        public static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        public static readonly int AlphaTexture = Shader.PropertyToID("_AlphaTexture");
        public static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        public static readonly int Metallic = Shader.PropertyToID("_Metallic");
        public static readonly int Smoothness = Shader.PropertyToID("_Smoothness");

        public static readonly int Cutoff = Shader.PropertyToID("_Cutoff");
        public static readonly int BumpMap = Shader.PropertyToID("_BumpMap");
        public static readonly int BumpScale = Shader.PropertyToID("_BumpScale");

        public static readonly int OcclusionMap = Shader.PropertyToID("_OcclusionMap");
        public static readonly int OcclusionStrength = Shader.PropertyToID("_OcclusionStrength");

        public static readonly int EmissionMap = Shader.PropertyToID("_EmissionMap");
        public static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        public static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
        public static readonly int DstBlend = Shader.PropertyToID("_DstBlend");
        public static readonly int ZWrite = Shader.PropertyToID("_ZWrite");
        public static readonly int AlphaClip = Shader.PropertyToID("_AlphaClip");
        public static readonly int Cull = Shader.PropertyToID("_Cull");

        public static readonly int SpecularHighlights = Shader.PropertyToID("_SpecularHighlights");
        public static readonly int EnvironmentReflections = Shader.PropertyToID("_EnvironmentReflections");
        public static readonly int Surface = Shader.PropertyToID("_Surface");

        //Lit keywords
        public static readonly string NormalMapKeyword = "_NORMALMAP";
        public static readonly string MetallicSpecGlossmapKeyword = "_METALLICSPECGLOSSMAP";
        public static readonly string AlphaTestOnKeyword = "_ALPHATEST_ON";
        public static readonly string AlphaPremultiplyOnKeyword = "_ALPHAPREMULTIPLY_ON";

        //Lit rendertypes
        public static readonly string RenderType = "RenderType";
        public static readonly string Transparent = "Transparent";
        public static readonly string TransparentCutout = "TransparentCutout";
        public static readonly string Opaque = "Opaque";

        //Avatar specific properties
        public static readonly int EyesTexture = Shader.PropertyToID("_EyesTexture");
        public static readonly int EyeTint = Shader.PropertyToID("_EyeTint");
        public static readonly int IrisMask = Shader.PropertyToID("_IrisMask");
        public static readonly int TintMask = Shader.PropertyToID("_TintMask");
        public static readonly string SSAO_OFF_KEYWORD = "_SSAO_OFF";

        /// <summary>
        /// Upgrades an AB Material from 2020 to 2021
        /// </summary>
        /// <param name="material"></param>
        public static void UpgradeMaterial_2020_To_2021(Material material)
        {
            int originalRenderQueue = material.renderQueue;
            material.shader = Shader.Find("DCL/Universal Render Pipeline/Lit");
            material.renderQueue = originalRenderQueue;
            float cutoff = material.GetFloat(Cutoff);

            if (material.HasTexture( BumpMap) && material.GetTexture(BumpMap) != null)
                material.EnableKeyword(NormalMapKeyword);
            if (material.HasTexture(MetallicGlossMap) && material.GetTexture(MetallicGlossMap) != null)
                material.EnableKeyword(MetallicSpecGlossmapKeyword);

            if (originalRenderQueue == (int)RenderQueue.Transparent)
            {
                material.SetOverrideTag(RenderType, Transparent);
                material.SetInt(SrcBlend, (int)BlendMode.SrcAlpha);
                material.SetInt(DstBlend, (int)BlendMode.OneMinusSrcAlpha);
                material.SetInt(ZWrite, 0);
                material.DisableKeyword(AlphaTestOnKeyword);
                material.DisableKeyword(AlphaPremultiplyOnKeyword);
                return;
            }

            if (originalRenderQueue > 2600 || cutoff > 0)
            {
                material.SetOverrideTag(RenderType, TransparentCutout);
                material.SetInt(SrcBlend, (int)BlendMode.One);
                material.SetInt(DstBlend, (int)BlendMode.Zero);
                material.SetInt(ZWrite, 1);
                material.SetFloat(AlphaClip, 1);
                material.EnableKeyword(AlphaTestOnKeyword);
                material.DisableKeyword(AlphaPremultiplyOnKeyword);
                return;
            }

            material.SetOverrideTag(RenderType, Opaque);
            material.SetInt(SrcBlend, (int)BlendMode.One);
            material.SetInt(DstBlend, (int)BlendMode.Zero);
            material.SetInt(ZWrite, 1);
            material.DisableKeyword(AlphaTestOnKeyword);
            material.DisableKeyword(AlphaPremultiplyOnKeyword);
        }
    }
}
