using System.Collections.Generic;
using UnityEngine;

namespace DCL.Helpers
{
    public static class ShaderUtils
    {
        public static readonly int _SpecGlossMap = Shader.PropertyToID("_SpecGlossMap");
        public static readonly int _SmoothnessTextureChannel = Shader.PropertyToID("_SmoothnessTextureChannel");
        public static readonly int _SpecColor = Shader.PropertyToID("_SpecColor");
        public static readonly int _GlossMapScale = Shader.PropertyToID("_GlossMapScale");
        public static readonly int _Glossiness = Shader.PropertyToID("_Glossiness");

        public static readonly int _BaseMap = Shader.PropertyToID("_BaseMap");
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

    public static class SRPBatchingHelper
    {
        static Dictionary<int, int> crcToQueue = new Dictionary<int, int>();

        public static void OptimizeMaterial(Renderer renderer, Material material)
        {
            //NOTE(Brian): Just enable these keywords so the SRP batcher batches more stuff.
            material.EnableKeyword("_EMISSION");
            material.EnableKeyword("_NORMALMAP");

            material.enableInstancing = true;

            int zWrite = (int) material.GetFloat(ShaderUtils._ZWrite);

            //NOTE(Brian): for transparent meshes skip further variant optimization.
            //             Transparency needs clip space z sorting to be displayed correctly.
            if (zWrite == 0)
            {
                material.renderQueue = (int) UnityEngine.Rendering.RenderQueue.Transparent;
                return;
            }

            int cullMode = (int) material.GetFloat(ShaderUtils._Cull);

            int baseQueue;

            if (material.renderQueue == (int) UnityEngine.Rendering.RenderQueue.AlphaTest)
                baseQueue = (int) UnityEngine.Rendering.RenderQueue.AlphaTest;
            else
                baseQueue = (int) UnityEngine.Rendering.RenderQueue.Geometry;

            material.DisableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
            material.DisableKeyword("_SPECULARHIGHLIGHTS_OFF");
            material.SetFloat(ShaderUtils._SpecularHighlights, 1);
            material.SetFloat(ShaderUtils._EnvironmentReflections, 1);

            //NOTE(Brian): This guarantees grouping calls by same shader keywords. Needed to take advantage of SRP batching.
            string appendedKeywords = string.Join("", material.shaderKeywords);
            int crc = Shader.PropertyToID(appendedKeywords);

            if (!crcToQueue.ContainsKey(crc))
                crcToQueue.Add(crc, crcToQueue.Count + 1);

            //NOTE(Brian): This is to move the rendering of animated stuff on top of the queue, so the SRP batcher
            //             can group all the draw calls.
            if (renderer is SkinnedMeshRenderer)
            {
                material.renderQueue = baseQueue;
            }
            else
            {
                //NOTE(Brian): we use 0, 100, 200 to group calls by culling mode (must group them or batches will break).
                int queueOffset = (cullMode + 1) * 100;

                material.renderQueue = baseQueue + crcToQueue[crc] + queueOffset;
            }
        }
    }
}