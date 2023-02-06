using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace DCL
{
    public struct CombineLayersList : IDisposable
    {
        private static readonly Predicate<CombineLayer> SANITIZE_LAYER_FUNC = x => x.renderers.Count == 0;

        private List<CombineLayer> layers;

        public int TotalVerticesCount { get; private set; }
        public int Count => layers.Count;

        public void Add(CombineLayer combineLayer)
        {
            layers.Add(combineLayer);
            AddToTotalMetrics(combineLayer.renderers);
        }

        public void AddRenderer(CombineLayer combineLayer, SkinnedMeshRenderer renderer)
        {
            if (renderer == null)
                return;

            combineLayer.renderers.Add(renderer);
            TotalVerticesCount += renderer.sharedMesh.vertexCount;
        }

        public void AddRenderers(CombineLayer combineLayer, IEnumerable<SkinnedMeshRenderer> renderers)
        {
            combineLayer.renderers.AddRange(renderers);
        }

        public void Sanitize()
        {
            layers.RemoveAll(SANITIZE_LAYER_FUNC);
        }

        private void AddToTotalMetrics(IEnumerable<SkinnedMeshRenderer> renderers)
        {
            foreach (SkinnedMeshRenderer meshRenderer in renderers)
            {
                TotalVerticesCount += meshRenderer.sharedMesh.vertexCount;
            }
        }

        public void Dispose()
        {
            ListPool<CombineLayer>.Release(layers);
            layers = null;
        }

        public static CombineLayersList Create()
        {
            var instance = new CombineLayersList();
            instance.layers = ListPool<CombineLayer>.Get();
            return instance;
        }
    }
}
