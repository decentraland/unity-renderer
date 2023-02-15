using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace DCL
{
    public class CombineLayersList : IDisposable
    {
        private static readonly Predicate<CombineLayer> SANITIZE_LAYER_FUNC = x => x.Renderers.Count == 0;

        private List<CombineLayer> layers;

        public IReadOnlyList<CombineLayer> Layers => layers;

        public int TotalVerticesCount { get; private set; }
        public int TotalRenderersCount { get; private set; }
        public int Count => layers.Count;

        public CombineLayer this[int index] => layers[index];

        public void Add(CombineLayer combineLayer)
        {
            layers.Add(combineLayer);
            AddToTotalMetrics(combineLayer.Renderers);
        }

        public void AddRenderer(CombineLayer combineLayer, SkinnedMeshRenderer renderer)
        {
            if (renderer == null)
                return;

            combineLayer.AddRenderer(renderer);
            TotalVerticesCount += renderer.sharedMesh.vertexCount;
            TotalRenderersCount++;
        }

        public void AddRenderers(CombineLayer combineLayer, IReadOnlyList<SkinnedMeshRenderer> renderers)
        {
            combineLayer.AddRenderers(renderers);
            AddToTotalMetrics(renderers);
        }

        public void Sanitize()
        {
            layers.RemoveAll(SANITIZE_LAYER_FUNC);
        }

        private void AddToTotalMetrics(IReadOnlyList<SkinnedMeshRenderer> renderers)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < renderers.Count; i++)
            {
                SkinnedMeshRenderer meshRenderer = renderers[i];
                TotalVerticesCount += meshRenderer.sharedMesh.vertexCount;
            }

            TotalRenderersCount += renderers.Count;
        }

        public void Dispose()
        {
            TotalVerticesCount = 0;
            TotalRenderersCount = 0;

            foreach (CombineLayer layer in layers)
                layer.Dispose();

            ListPool<CombineLayer>.Release(layers);
            layers = null;
            GenericPool<CombineLayersList>.Release(this);
        }

        public static CombineLayersList Rent()
        {
            var instance = GenericPool<CombineLayersList>.Get();
            instance.layers = ListPool<CombineLayer>.Get();
            return instance;
        }
    }
}
