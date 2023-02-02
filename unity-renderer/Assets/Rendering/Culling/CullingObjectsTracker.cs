using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using static DCL.DataStore_WorldObjects;

namespace DCL.Rendering
{
    /// <summary> This class is used for tracking all the renderers, skinnedMeshRenderers and Animations of the world. </summary>
    public class CullingObjectsTracker : ICullingObjectsTracker
    {
        private List<Renderer> usingRenderers = new List<Renderer>();
        private SkinnedMeshRenderer[] detectedSkinnedRenderers;
        private List<SkinnedMeshRenderer> usingSkinnedRenderers = new List<SkinnedMeshRenderer>();
        private Animation[] usingAnimations;

        private int ignoredLayersMask;
        private bool dirty = true;


        public CullingObjectsTracker()
        {
            PoolManager.i.OnGet += MarkDirty;
        }

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
        }

        /// <summary> Sets the dirty flag to true to make PopulateRenderersList retrieve all the scene objects on its next call. </summary>
        public void MarkDirty() => dirty = true;

        /// <summary> Returns true if dirty. </summary>
        public bool IsDirty() => dirty;

        /// <summary> Returns the renderers list. </summary>
        public IReadOnlyList<Renderer> GetRenderers() => usingRenderers;

        /// <summary> Returns the skinned renderers list. </summary>
        public IReadOnlyList<SkinnedMeshRenderer> GetSkinnedRenderers() => usingSkinnedRenderers;

        /// <summary> Returns the animations list. </summary>
        public Animation[] GetAnimations() => usingAnimations;


        private void ForceCalculateRenderers()
        {
            usingRenderers = new List<Renderer>();
            foreach (KeyValuePair<int, SceneData> entry in DataStore.i.sceneWorldObjects.sceneData)
            {
                foreach (Renderer renderer in entry.Value.renderers.Get())
                {
                    if (renderer == null || !renderer.gameObject.activeInHierarchy)
                        continue;

                    if (ShouldNotBeIgnored(renderer))
                        usingRenderers.Add(renderer);
                }
            }
        }

        private void ForceCalculateSkinnedRenderers()
        {
            usingSkinnedRenderers = new List<SkinnedMeshRenderer>(detectedSkinnedRenderers);
            foreach (SkinnedMeshRenderer skinnedRenderer in detectedSkinnedRenderers)
            {
                if (skinnedRenderer != null && ShouldNotBeIgnored(skinnedRenderer))
                    usingSkinnedRenderers.Add(skinnedRenderer);
            }
        }

        private bool ShouldNotBeIgnored(Renderer renderer) => ((1 << renderer.gameObject.layer) & ignoredLayersMask) == 0;


        private IEnumerator CalculateRenderers()
        {
            float currentStartTime = Time.realtimeSinceStartup;
            usingRenderers.Clear();
            foreach (SceneData sceneData in DataStore.i.sceneWorldObjects.sceneData.GetValues())
            {
                if (sceneData == null)
                    continue;

                using (PooledObject<List<Renderer>> pooledObject = ListPool<Renderer>.Get(out List<Renderer> tempList))
                {
                    foreach (Renderer renderer in sceneData.renderers.Get())
                        tempList.Add(renderer);

                    foreach (Renderer renderer in tempList)
                    {
                        if (renderer == null || !renderer.gameObject.activeInHierarchy)
                            continue;

                        if (Time.realtimeSinceStartup - currentStartTime >= CullingControllerSettings.MAX_TIME_BUDGET)
                        {
                            yield return null;
                            currentStartTime = Time.realtimeSinceStartup;
                        }

                        if (ShouldNotBeIgnored(renderer))
                            usingRenderers.Add(renderer);
                    }
                }
            }
        }

        private IEnumerator CalculateSkinnedRenderers()
        {
            float currentStartTime = Time.realtimeSinceStartup;
            List<SkinnedMeshRenderer> checkingSkinnedRenderers = new List<SkinnedMeshRenderer>(detectedSkinnedRenderers);
            usingSkinnedRenderers = new List<SkinnedMeshRenderer>();
            for (int i = checkingSkinnedRenderers.Count - 1; i >= 0; i--)
            {
                SkinnedMeshRenderer skinnedRenderer = checkingSkinnedRenderers[i];
                if (skinnedRenderer == null)
                    continue;

                if (Time.realtimeSinceStartup - currentStartTime >= CullingControllerSettings.MAX_TIME_BUDGET)
                {
                    yield return null;
                    currentStartTime = Time.realtimeSinceStartup;
                }

                if (ShouldNotBeIgnored(skinnedRenderer))
                    usingSkinnedRenderers.Add(skinnedRenderer);
            }
        }
    }
}
