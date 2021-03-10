using UnityEngine;

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

        //Lit properties
        public static readonly int SpecGlossMap = Shader.PropertyToID("_SpecGlossMap");
        public static readonly int SmoothnessTextureChannel = Shader.PropertyToID("_SmoothnessTextureChannel");
        public static readonly int SpecColor = Shader.PropertyToID("_SpecColor");
        public static readonly int GlossMapScale = Shader.PropertyToID("_GlossMapScale");
        public static readonly int Glossiness = Shader.PropertyToID("_Glossiness");

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
    }
}