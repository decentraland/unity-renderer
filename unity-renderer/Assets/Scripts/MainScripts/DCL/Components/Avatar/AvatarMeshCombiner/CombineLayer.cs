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
        public List<SkinnedMeshRenderer> renderers { get; private set; }
        public Dictionary<Texture2D, int> textureToId { get; private set; }

        public CullMode cullMode;
        public bool isOpaque;

        public CombineLayer()
        {
        }

        public void Dispose()
        {
            ListPool<SkinnedMeshRenderer>.Release(renderers);
            DictionaryPool<Texture2D, int>.Release(textureToId);

            renderers = null;
            textureToId = null;

            GenericPool<CombineLayer>.Release(this);
        }

        public static CombineLayer Rent(CullMode cullMode = CullMode.Off, bool isOpaque = false)
        {
            var layer = GenericPool<CombineLayer>.Get();
            layer.cullMode = cullMode;
            layer.isOpaque = isOpaque;
            layer.renderers = ListPool<SkinnedMeshRenderer>.Get();
            layer.textureToId = DictionaryPool<Texture2D, int>.Get();

            return layer;
        }

        public override string ToString()
        {
            string rendererString = $"renderer count: {renderers?.Count ?? 0}";
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
