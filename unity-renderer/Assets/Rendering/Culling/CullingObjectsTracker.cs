using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL.Rendering
{
    /// <summary>
    /// This class is used for tracking all the renderers, skinnedMeshRenderers and Animations of the world.
    ///
    /// It currently uses a very lazy FindObjectsOfType approach, but is enough for its purposes as its used
    /// to optimize a bigger bottleneck.
    /// </summary>
    public class CullingObjectsTracker : ICullingObjectsTracker
    {
        private ICollection<Renderer> renderers;
        private ICollection<SkinnedMeshRenderer> skinnedRenderers;
        private Animation[] animations;

        private int ignoredLayersMask;
        private bool dirty = true;


        public CullingObjectsTracker()
        {
            PoolManager.i.OnGet -= MarkDirty;
            PoolManager.i.OnGet += MarkDirty;
        }

        /// <summary>
        /// If the dirty flag is true, this coroutine will re-populate all the tracked objects.
        /// </summary>
        public IEnumerator PopulateRenderersList()
        {
            if (!dirty)
                yield break;

            List<Renderer> renderersList = new List<Renderer>();
            List<SkinnedMeshRenderer> skinnedRenderersList = new List<SkinnedMeshRenderer>();
            Renderer[] allRenderers = Object.FindObjectsOfType<Renderer>();

            foreach (Renderer renderer in allRenderers)
            {
                if ((((1 << renderer.gameObject.layer) & ignoredLayersMask) == 0)
                    && !(renderer is ParticleSystemRenderer))
                {
                    if (renderer is SkinnedMeshRenderer)
                        skinnedRenderersList.Add((SkinnedMeshRenderer)renderer);
                    else
                        renderersList.Add(renderer);
                }
            }
            renderers = renderersList;
            skinnedRenderers = skinnedRenderersList;

            yield return null;

            animations = Object.FindObjectsOfType<Animation>();

            dirty = false;
        }

        /// <summary>
        ///  This will re-populate all the tracked objects in a sync way.
        /// </summary>
        /// <param name="includeInactives">True for add inactive objects to the tracked list.</param>
        public void ForcePopulateRenderersList(bool includeInactives)
        {
            List<Renderer> renderersList = new List<Renderer>();
            List<SkinnedMeshRenderer> skinnedRenderersList = new List<SkinnedMeshRenderer>();
            Renderer[] allRenderers = Object.FindObjectsOfType<Renderer>(includeInactives);

            foreach (Renderer renderer in allRenderers)
            {
                if ((((1 << renderer.gameObject.layer) & ignoredLayersMask) == 0)
                    && !(renderer is ParticleSystemRenderer))
                {
                    if (renderer is SkinnedMeshRenderer)
                        skinnedRenderersList.Add((SkinnedMeshRenderer)renderer);
                    else
                        renderersList.Add(renderer);
                }
            }
            renderers = renderersList;
            skinnedRenderers = skinnedRenderersList;
        }

        public void SetIgnoredLayersMask(int ignoredLayersMask) { this.ignoredLayersMask = ignoredLayersMask; }

        /// <summary>
        /// Sets the dirty flag to true to make PopulateRenderersList retrieve all the scene objects on its next call.
        /// </summary>
        public void MarkDirty() { dirty = true; }

        /// <summary>
        /// Returns true if dirty.
        /// </summary>
        /// <returns>True if dirty.</returns>
        public bool IsDirty() { return dirty; }

        /// <summary>
        /// Returns the renderers list.
        /// </summary>
        /// <returns>An ICollection with all the tracked renderers.</returns>
        public ICollection<Renderer> GetRenderers() { return renderers; }

        /// <summary>
        /// Returns the skinned renderers list.
        /// </summary>
        /// <returns>An ICollection with all the tracked skinned renderers.</returns>
        public ICollection<SkinnedMeshRenderer> GetSkinnedRenderers() { return skinnedRenderers; }

        /// <summary>
        /// Returns the animations list.
        /// </summary>
        /// <returns>An array with all the tracked animations.</returns>
        public Animation[] GetAnimations() { return animations; }

        public void Dispose()
        {
            dirty = true;
            renderers = null;
            animations = null;
            skinnedRenderers = null;
            PoolManager.i.OnGet -= MarkDirty;
        }
    }
}