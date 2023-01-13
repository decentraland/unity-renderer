using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Rendering
{
    /// <summary> This class is used for tracking all the renderers, skinnedMeshRenderers and Animations of the world. </summary>
    public class CullingObjectsTracker : ICullingObjectsTracker
    {
        private readonly List<Rendereable> rendereables = new List<Rendereable>();
        private ICollection<Renderer> usingRenderers = new List<Renderer>();
        private ICollection<SkinnedMeshRenderer> detectedSkinnedRenderers;
        private ICollection<SkinnedMeshRenderer> usingSkinnedRenderers = new List<SkinnedMeshRenderer>();
        private Animation[] usingAnimations;

        private int ignoredLayersMask;
        private bool dirty = true;
        

        public CullingObjectsTracker()
        {
            PoolManager.i.OnGet -= MarkDirty;
            PoolManager.i.OnGet += MarkDirty;

            Rendereable.OnRendereableAdded -= OnRendereableAdded;
            Rendereable.OnRendereableAdded += OnRendereableAdded;
        }

        /// <summary> Adds a rendereable to be tracked and calculating in the culling process. </summary>
        public void OnRendereableAdded(object sender, Rendereable rendereable) => rendereables.Add(rendereable);

        /// <summary> Sets a layer mask to be ignored, only one layer mask can be ingored at a given time. </summary>
        public void SetIgnoredLayersMask(int ignoredLayersMask) { this.ignoredLayersMask = ignoredLayersMask; }

        /// <summary> This will re-populate all the tracked objects synchronously </summary>
        public void ForcePopulateRenderersList()
        {
            detectedSkinnedRenderers = Object.FindObjectsOfType<SkinnedMeshRenderer>();

            ForceCalculateRenderers();
            ForceCalculateSkinnedRenderers();
            usingAnimations = Object.FindObjectsOfType<Animation>();

            dirty = false;
        }

        /// <summary> If the dirty flag is true, this coroutine will re-populate all the tracked objects. </summary>
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

        public void Dispose()
        {
            dirty = true;
            usingRenderers = null;
            usingSkinnedRenderers = null;
            detectedSkinnedRenderers = null;
            usingAnimations = null;
            PoolManager.i.OnGet -= MarkDirty;
            Rendereable.OnRendereableAdded -= OnRendereableAdded;
        }

        /// <summary> Sets the dirty flag to true to make PopulateRenderersList retrieve all the scene objects on its next call. </summary>
        public void MarkDirty() => dirty = true;

        /// <summary> Returns true if dirty. </summary>
        public bool IsDirty() => dirty;

        /// <summary> Returns the renderers list. </summary>
        public ICollection<Renderer> GetRenderers() => usingRenderers;

        /// <summary> Returns the skinned renderers list. </summary>
        public ICollection<SkinnedMeshRenderer> GetSkinnedRenderers() => usingSkinnedRenderers;

        /// <summary> Returns the animations list. </summary>
        public Animation[] GetAnimations() => usingAnimations;


        private void ForceCalculateRenderers()
        {
            ForceCleanRendereables();
            
            usingRenderers = new List<Renderer>();
            foreach (Rendereable rendereable in rendereables)
            {
                if (!rendereable.container.activeInHierarchy)
                    continue;

                foreach (Renderer renderer in rendereable.renderers)
                {
                    if (renderer == null || !renderer.gameObject.activeInHierarchy)
                        continue;

                    if (((1 << renderer.gameObject.layer) & ignoredLayersMask) == 0)
                        usingRenderers.Add(renderer);
                }
            }
        }

        private void ForceCalculateSkinnedRenderers()
        {
            usingSkinnedRenderers = new List<SkinnedMeshRenderer>(detectedSkinnedRenderers);
            foreach (SkinnedMeshRenderer skinnedRenderer in detectedSkinnedRenderers)
            {
                if (skinnedRenderer == null)
                    continue;

                if (((1 << skinnedRenderer.gameObject.layer) & ignoredLayersMask) == 0)
                    usingSkinnedRenderers.Add(skinnedRenderer);
            }
        }

        private void ForceCleanRendereables()
        {
            for (int i = rendereables.Count - 1; i >= 0; i--)
            {
                if (rendereables[i] == null || rendereables[i].container == null)
                    rendereables.RemoveAt(i);
            }
        }

        private IEnumerator CalculateRenderers()
        {
            int amount = 0;
            List<Rendereable> checkingRendereables = new List<Rendereable>(rendereables);
            usingRenderers.Clear();
            foreach (Rendereable rendereable in checkingRendereables)
            {
                if (rendereable == null || !rendereable.container)
                {
                    rendereables.Remove(rendereable);
                    continue;
                }

                if (!rendereable.container.activeInHierarchy)
                    continue;

                foreach (Renderer renderer in rendereable.renderers)
                {
                    if (renderer == null || !renderer.gameObject.activeInHierarchy)
                    {
                        amount++;
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
            }
        }

        private IEnumerator CalculateSkinnedRenderers()
        {
            int amount = 0;
            List<SkinnedMeshRenderer> checkingSkinnedRenderers = new List<SkinnedMeshRenderer>(detectedSkinnedRenderers);
            usingSkinnedRenderers = new List<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer skinnedRenderer in checkingSkinnedRenderers)
            {
                if (skinnedRenderer == null)
                {
                    amount++;
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
        }
    }
}
