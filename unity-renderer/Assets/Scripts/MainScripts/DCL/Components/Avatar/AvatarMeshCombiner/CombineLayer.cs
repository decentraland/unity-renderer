using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering;

namespace DCL
{
    /// <summary>
    /// This class is used by the AvatarMeshCombiner to combine meshes. Each layer represents a new generated sub-mesh.
    /// </summary>
    public class CombineLayer : IDisposable
    {
        private List<SkinnedMeshRenderer> renderers;

        public IReadOnlyList<SkinnedMeshRenderer> Renderers => renderers;
        public Dictionary<Texture2D, int> textureToId { get; private set; }

        public CullMode cullMode;
        public bool isOpaque;

        public void Dispose()
        {
            ListPool<SkinnedMeshRenderer>.Release(renderers);
            DictionaryPool<Texture2D, int>.Release(textureToId);

            renderers = null;
            textureToId = null;

            GenericPool<CombineLayer>.Release(this);
        }

        internal void AddRenderer(SkinnedMeshRenderer renderer)
        {
            renderers.Add(renderer);
        }

        internal void AddRenderers(IReadOnlyCollection<SkinnedMeshRenderer> renderers)
        {
            this.renderers.AddRange(renderers);
        }

        public static CombineLayer Rent(CullMode cullMode = CullMode.Off, bool isOpaque = false, IReadOnlyCollection<SkinnedMeshRenderer> renderers = null)
        {
            var layer = GenericPool<CombineLayer>.Get();
            layer.cullMode = cullMode;
            layer.isOpaque = isOpaque;
            layer.renderers = ListPool<SkinnedMeshRenderer>.Get();
            layer.textureToId = DictionaryPool<Texture2D, int>.Get();

            if (renderers != null)
                layer.AddRenderers(renderers);

            return layer;
        }

        public override string ToString()
        {
            string rendererString = $"renderer count: {Renderers?.Count ?? 0}";
            string textureIdString = "texture ids: {";

            foreach ( var kvp in textureToId )
            {
                textureIdString += $" tx hash: {kvp.Key.GetHashCode()} id: {kvp.Value} ";
            }

            textureIdString += "}";

            return $"cullMode: {cullMode} - isOpaque: {isOpaque} - {rendererString} - {textureIdString}";
        }
    }
}
