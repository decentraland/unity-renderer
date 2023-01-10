using System.Collections;
using System.Collections.Generic;
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
        private ICollection<Renderer> detectedRenderers = new List<Renderer>();
        private ICollection<Renderer> usingRenderers;
        private ICollection<SkinnedMeshRenderer> detectedSkinnedRenderers;
        private ICollection<SkinnedMeshRenderer> usingSkinnedRenderers;
        private Animation[] usingAnimations;

        private int ignoredLayersMask;
        private bool dirty = true;


        public CullingObjectsTracker()
        {
            PoolManager.i.OnGet -= MarkDirty;
            PoolManager.i.OnGet += MarkDirty;
        }

        public void AddRenderers(ICollection<Renderer> inRenderers)
        {
            foreach (Renderer renderer in inRenderers)
            {
                if (renderer is not SkinnedMeshRenderer and not ParticleSystemRenderer)
                    detectedRenderers.Add(renderer);
            }
        }

        /// <summary>
        /// If the dirty flag is true, this coroutine will re-populate all the tracked objects.
        /// </summary>
        public IEnumerator PopulateRenderersList()
        {
            if (!dirty)
                yield break;

            detectedSkinnedRenderers = Object.FindObjectsOfType<SkinnedMeshRenderer>();
            yield return null;
            yield return CalculateRenderers();
            yield return null;
            yield return CalculateSkinnedRenderers();
            yield return null;
            usingAnimations = Object.FindObjectsOfType<Animation>();
            yield return null;
            
            dirty = false;
        }

        /// <summary>
        ///  This will re-populate all the tracked objects in a sync way.
        /// </summary>
        /// <param name="includeInactives">True for add inactive objects to the tracked list.</param>
        public void ForcePopulateRenderersList(bool includeInactives)
        {
            detectedSkinnedRenderers = Object.FindObjectsOfType<SkinnedMeshRenderer>();

            ForceCalculateRenderers();
            ForceCalculateSkinnedRenderers();
            usingAnimations = Object.FindObjectsOfType<Animation>();
            
            dirty = false;
        }


        private IEnumerator CalculateRenderers()
        {
            int amount = 0;
            List<Renderer> renderersToRemove = new List<Renderer>();
            List<Renderer> checkingRenderers = new List<Renderer>(detectedRenderers);
            usingRenderers.Clear();
            foreach (Renderer renderer in checkingRenderers)
            {
                if (renderer == null)
                {
                    renderersToRemove.Add(renderer);
                    continue;
                }

                if (amount >= CullingControllerSettings.MAX_POPULATING_ELEMENTS_PER_FRAME)
                {
                    yield return null;
                    amount = 0;
                }
                amount++;

                if (((1 << renderer.gameObject.layer) & ignoredLayersMask) == 0)
                    usingRenderers.Add(renderer);
            }
            for (int i = renderersToRemove.Count - 1; i >= 0; i--)
                renderersToRemove.Remove(renderersToRemove[i]);
        }

        private IEnumerator CalculateSkinnedRenderers()
        {
            int amount = 0;
            List<SkinnedMeshRenderer> skinnedRenderersToRemove = new List<SkinnedMeshRenderer>();
            List<SkinnedMeshRenderer> checkingSkinnedRenderers = new List<SkinnedMeshRenderer>(skinnedRenderersToRemove);
            usingSkinnedRenderers.Clear();
            foreach (SkinnedMeshRenderer skinnedRenderer in checkingSkinnedRenderers)
            {
                if (skinnedRenderer == null)
                {
                    skinnedRenderersToRemove.Add(skinnedRenderer);
                    continue;
                }

                if (amount >= CullingControllerSettings.MAX_POPULATING_ELEMENTS_PER_FRAME)
                {
                    yield return null;
                    amount = 0;
                }
                amount++;

                if (((1 << skinnedRenderer.gameObject.layer) & ignoredLayersMask) == 0)
                    usingSkinnedRenderers.Add(skinnedRenderer);
            }
            for (int i = skinnedRenderersToRemove.Count - 1; i >= 0; i--)
                skinnedRenderersToRemove.Remove(skinnedRenderersToRemove[i]);
        }

        private void ForceCalculateRenderers()
        {
            List<Renderer> renderersToRemove = new List<Renderer>();
            usingRenderers.Clear();
            foreach (Renderer renderer in detectedRenderers)
            {
                if (renderer == null)
                {
                    renderersToRemove.Add(renderer);
                    continue;
                }

                if (((1 << renderer.gameObject.layer) & ignoredLayersMask) == 0)
                    usingRenderers.Add(renderer);
            }
            for (int i = renderersToRemove.Count - 1; i >= 0; i--)
                renderersToRemove.Remove(renderersToRemove[i]);
        }

        private void ForceCalculateSkinnedRenderers()
        {
            List<SkinnedMeshRenderer> skinnedRenderersToRemove = new List<SkinnedMeshRenderer>();
            usingSkinnedRenderers.Clear();
            foreach (SkinnedMeshRenderer skinnedRenderer in detectedSkinnedRenderers)
            {
                if (skinnedRenderer == null)
                {
                    skinnedRenderersToRemove.Add(skinnedRenderer);
                    continue;
                }

                if (((1 << skinnedRenderer.gameObject.layer) & ignoredLayersMask) == 0)
                    usingSkinnedRenderers.Add(skinnedRenderer);
            }
            for (int i = skinnedRenderersToRemove.Count - 1; i >= 0; i--)
                skinnedRenderersToRemove.Remove(skinnedRenderersToRemove[i]);
        }


        public void SetIgnoredLayersMask(int ignoredLayersMask) { this.ignoredLayersMask = ignoredLayersMask; }

        /// <summary>
        /// Sets the dirty flag to true to make PopulateRenderersList retrieve all the scene objects on its next call.
        /// </summary>
        public void MarkDirty() => dirty = true;

        /// <summary>
        /// Returns true if dirty.
        /// </summary>
        /// <returns>True if dirty.</returns>
        public bool IsDirty() => dirty;

        /// <summary>
        /// Returns the renderers list.
        /// </summary>
        /// <returns>An ICollection with all the tracked renderers.</returns>
        public ICollection<Renderer> GetRenderers() => usingRenderers;

        /// <summary>
        /// Returns the skinned renderers list.
        /// </summary>
        /// <returns>An ICollection with all the tracked skinned renderers.</returns>
        public ICollection<SkinnedMeshRenderer> GetSkinnedRenderers() => usingSkinnedRenderers;

        /// <summary>
        /// Returns the animations list.
        /// </summary>
        /// <returns>An array with all the tracked animations.</returns>
        public Animation[] GetAnimations() => usingAnimations;

        public void Dispose()
        {
            dirty = true;
            usingRenderers = null;
            detectedRenderers = null;
            usingSkinnedRenderers = null;
            detectedSkinnedRenderers = null;
            usingAnimations = null;
            PoolManager.i.OnGet -= MarkDirty;
        }
    }
}
