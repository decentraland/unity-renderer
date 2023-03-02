using DCL.Shaders;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering;

namespace DCL
{
    public static class SliceByRenderState
    {
        private static readonly Dictionary<Shader, bool> SHADERS_OPACITY = new ();

        private readonly struct LayerKey : IEquatable<LayerKey>
        {
            public readonly bool IsOpaque;
            public readonly CullMode CullMode;

            public LayerKey(bool isOpaque, CullMode cullMode)
            {
                IsOpaque = isOpaque;
                CullMode = cullMode;
            }

            public bool Equals(LayerKey other) =>
                IsOpaque == other.IsOpaque && CullMode == other.CullMode;

            public override bool Equals(object obj) =>
                obj is LayerKey other && Equals(other);

            public override int GetHashCode() =>
                HashCode.Combine(IsOpaque, (int)CullMode);
        }

        /// <summary>
        /// <p>
        /// This method takes a skinned mesh renderer list and turns it into a series of CombineLayer elements.
        /// Each CombineLayer element represents a combining group, and the renderers are grouped using a set of criteria.
        /// </p>
        /// <p>
        /// For SliceByRenderState, the returned CombineLayer list will be grouped according to shared cull mode and
        /// blend state.
        /// </p>
        /// </summary>
        /// <param name="renderers">List of renderers to slice.</param>
        /// <returns>List of CombineLayer objects that can be used to produce a highly optimized combined mesh.</returns>
        internal static void Execute(IReadOnlyList<SkinnedMeshRenderer> renderers, List<CombineLayer> result, bool cullOpaque)
        {
            var grouping = DictionaryPool<LayerKey, CombineLayer>.Get();

            // Group renderers on opaque and transparent materials
            // Then, make subgroups to divide them between culling modes
            foreach (var meshRenderer in renderers)
            {
                var opaque = IsOpaque(meshRenderer);
                var cullMode = cullOpaque && opaque ? GetCullModeWithoutCullOff(meshRenderer) : GetCullMode(meshRenderer);
                var key = new LayerKey(opaque, cullMode);

                if (!grouping.TryGetValue(key, out var combineLayer))
                {
                    grouping[key] = combineLayer = CombineLayer.Rent(cullMode, opaque);
                    result.Add(combineLayer);
                }

                combineLayer.AddRenderer(meshRenderer);
            }

            DictionaryPool<LayerKey, CombineLayer>.Release(grouping);

            /*
            * The grouping outcome ends up like this:
            *
            *                 Opaque           Transparent
            *             /     |     \        /    |    \
            *          Back - Front - Off - Back - Front - Off -> rendererGroups
            */
        }

        internal static CullMode GetCullMode(Renderer renderer) =>
            GetCullMode(renderer.sharedMaterial);

        internal static CullMode GetCullMode(Material material)
        {
            if (material.HasProperty(ShaderUtils.Cull))
            {
                CullMode result = (CullMode)material.GetInt(ShaderUtils.Cull);
                return result;
            }

            // GLTFast materials dont have culling, instead they have the "Double Sided" check toggled on "double" suffixed shaders
            if (material.shader.name.Contains("double"))
                return CullMode.Off;

            return CullMode.Back;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="renderer"></param>
        /// <returns></returns>
        internal static CullMode GetCullModeWithoutCullOff(Renderer renderer)
        {
            CullMode result = GetCullMode(renderer.sharedMaterial);

            if (result == CullMode.Off)
                result = CullMode.Back;

            return result;
        }

        /// <summary>
        /// Determines if the given renderer is going to be enqueued at the opaque section of the rendering pipeline.
        /// </summary>
        /// <param name="renderer">Renderer to be checked.</param>
        /// <returns>True if its opaque</returns>
        internal static bool IsOpaque(Renderer renderer) => IsOpaque(renderer.sharedMaterial);

        /// <summary>
        /// Determines if the given renderer is going to be enqueued at the opaque section of the rendering pipeline.
        /// </summary>
        /// <param name="material">Material to be checked</param>
        /// <returns>True if its opaque</returns>
        internal static bool IsOpaque(Material material)
        {
            if (material == null)
                return true;

            var shader = material.shader;

            if (!SHADERS_OPACITY.TryGetValue(shader, out var isOpaqueShader))
            {
                // NOTE(Kinerius): Since GLTFast materials doesn't have ZWrite property, we check if the shader name is opaque instead
                bool hasOpaqueName = shader.name.Contains("opaque", StringComparison.OrdinalIgnoreCase);
                SHADERS_OPACITY[shader] = isOpaqueShader = hasOpaqueName;
            }

            bool hasZWrite = material.HasProperty(ShaderUtils.ZWrite);
            isOpaqueShader = (!hasZWrite && !isOpaqueShader) || (hasZWrite && (int)material.GetFloat(ShaderUtils.ZWrite) == 0);

            return !isOpaqueShader;
        }
    }
}
