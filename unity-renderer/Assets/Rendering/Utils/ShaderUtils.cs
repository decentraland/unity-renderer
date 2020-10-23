using UnityEngine;

namespace DCL.Helpers
{
    public static class ShaderUtils
    {
        //Toon shader properties
        public static readonly int _LightDir = Shader.PropertyToID("_LightDir");
        public static readonly int _LightColor = Shader.PropertyToID("_LightColor");
        public static readonly int _TintColor = Shader.PropertyToID("_TintColor");

        public static readonly int _GlossMatCap = Shader.PropertyToID("_GlossMatCap");
        public static readonly int _FresnelMatCap = Shader.PropertyToID("_FresnelMatCap");
        public static readonly int _MatCap = Shader.PropertyToID("_MatCap");

        //Lit properties
        public static readonly int _SpecGlossMap = Shader.PropertyToID("_SpecGlossMap");
        public static readonly int _SmoothnessTextureChannel = Shader.PropertyToID("_SmoothnessTextureChannel");
        public static readonly int _SpecColor = Shader.PropertyToID("_SpecColor");
        public static readonly int _GlossMapScale = Shader.PropertyToID("_GlossMapScale");
        public static readonly int _Glossiness = Shader.PropertyToID("_Glossiness");

        public static readonly int _BaseMap = Shader.PropertyToID("_BaseMap");
        public static readonly int _AlphaTexture = Shader.PropertyToID("_AlphaTexture");
        public static readonly int _BaseColor = Shader.PropertyToID("_BaseColor");
        public static readonly int _Metallic = Shader.PropertyToID("_Metallic");
        public static readonly int _Smoothness = Shader.PropertyToID("_Smoothness");

        public static readonly int _Cutoff = Shader.PropertyToID("_Cutoff");
        public static readonly int _BumpMap = Shader.PropertyToID("_BumpMap");
        public static readonly int _BumpScale = Shader.PropertyToID("_BumpScale");

        public static readonly int _OcclusionMap = Shader.PropertyToID("_OcclusionMap");
        public static readonly int _OcclusionStrength = Shader.PropertyToID("_OcclusionStrength");

        public static readonly int _EmissionMap = Shader.PropertyToID("_EmissionMap");
        public static readonly int _EmissionColor = Shader.PropertyToID("_EmissionColor");

        public static readonly int _SrcBlend = Shader.PropertyToID("_SrcBlend");
        public static readonly int _DstBlend = Shader.PropertyToID("_DstBlend");
        public static readonly int _ZWrite = Shader.PropertyToID("_ZWrite");
        public static readonly int _AlphaClip = Shader.PropertyToID("_AlphaClip");
        public static readonly int _Cull = Shader.PropertyToID("_Cull");

        public static readonly int _SpecularHighlights = Shader.PropertyToID("_SpecularHighlights");
        public static readonly int _EnvironmentReflections = Shader.PropertyToID("_EnvironmentReflections");
    }
}