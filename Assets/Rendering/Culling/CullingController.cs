using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;
using UnityEngine.Rendering;
using UniversalRenderPipelineAsset = UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
using static DCL.Rendering.CullingControllerUtils;

namespace DCL.Rendering
{
    public interface ICullingController : IDisposable
    {
        void Start();
        void Stop();
        void MarkDirty();
        void SetSettings(CullingControllerSettings settings);
        CullingControllerSettings GetSettingsCopy();
        void SetObjectCulling(bool enabled);
        void SetAnimationCulling(bool enabled);
        void SetShadowCulling(bool enabled);
        bool IsRunning();

        bool IsDirty();
        ICullingObjectsTracker objectsTracker { get; }
        event CullingController.DataReport OnDataReport;
    }

    /// <summary>
    /// CullingController has the following responsibilities:
    /// - Hides small renderers (detail objects).
    /// - Disable unneeded shadows.
    /// - Enable/disable animation culling for skinned renderers and animation components.
    /// </summary>
    public class CullingController : ICullingController
    {
        internal List<CullingControllerProfile> profiles = null;

        private CullingControllerSettings settings;

        private HashSet<Renderer> hiddenRenderers = new HashSet<Renderer>();
        private HashSet<Renderer> shadowlessRenderers = new HashSet<Renderer>();

        public UniversalRenderPipelineAsset urpAsset;

        public ICullingObjectsTracker objectsTracker { get; private set; }
        private Coroutine updateCoroutine;
        private float timeBudgetCount = 0;
        private bool resetObjectsNextFrame = false;
        private bool playerPositionDirty;
        private bool objectPositionsDirty;
        private bool running = false;

        public delegate void DataReport(int rendererCount, int hiddenRendererCount, int hiddenShadowCount);

        public event DataReport OnDataReport;

        public static CullingController Create()
        {
            return new CullingController(
                GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset,
                new CullingControllerSettings()
            );
        }

        public CullingController()
        {
        }

        public CullingController(UniversalRenderPipelineAsset urpAsset, CullingControllerSettings settings, ICullingObjectsTracker cullingObjectsTracker = null)
        {
            if (cullingObjectsTracker == null)
                objectsTracker = new CullingObjectsTracker();
            else
                objectsTracker = cullingObjectsTracker;

            this.urpAsset = urpAsset;
            this.settings = settings;
        }

        /// <summary>
        /// Starts culling update coroutine.
        /// The coroutine will keep running until Stop() is called or this class is disposed.
        /// </summary>
        public void Start()
        {
            if (running)
                return;

            running = true;
            CommonScriptableObjects.rendererState.OnChange += OnRendererStateChange;
            CommonScriptableObjects.playerUnityPosition.OnChange += OnPlayerUnityPositionChange;
            MeshesInfo.OnAnyUpdated += MarkDirty;
            StartInternal();
        }

        private void StartInternal()
        {
            if (updateCoroutine != null)
                return;

            RaiseDataReport();
            profiles = new List<CullingControllerProfile> {settings.rendererProfile, settings.skinnedRendererProfile};
            updateCoroutine = CoroutineStarter.Start(UpdateCoroutine());
        }

        /// <summary>
        /// Stops culling update coroutine.
        /// </summary>
        public void Stop()
        {
            if (!running)
                return;

            running = false;
            CommonScriptableObjects.rendererState.OnChange -= OnRendererStateChange;
            CommonScriptableObjects.playerUnityPosition.OnChange -= OnPlayerUnityPositionChange;
            MeshesInfo.OnAnyUpdated -= MarkDirty;
            StopInternal();
        }

        public void StopInternal()
        {
            if (updateCoroutine == null)
                return;

            CoroutineStarter.Stop(updateCoroutine);
            updateCoroutine = null;
        }

        /// <summary>
        /// Process all sceneObject renderers with the parameters set by the given profile.
        /// </summary>
        /// <param name="profile">any CullingControllerProfile</param>
        /// <returns>IEnumerator to be yielded.</returns>
        internal IEnumerator ProcessProfile(CullingControllerProfile profile)
        {
            Renderer[] renderers;

            // If profile matches the skinned renderer profile in settings,
            // the skinned renderers are going to be used.
            if (profile == settings.rendererProfile)
                renderers = objectsTracker.GetRenderers();
            else
                renderers = objectsTracker.GetSkinnedRenderers();


            for (var i = 0; i < renderers.Length; i++)
            {
                if (timeBudgetCount > settings.maxTimeBudget)
                {
                    timeBudgetCount = 0;
                    yield return null;
                }

                Renderer r = renderers[i];

                if (r == null)
                    continue;

                bool rendererIsInIgnoreLayer = ((1 << r.gameObject.layer) & settings.ignoredLayersMask) != 0;
                if (rendererIsInIgnoreLayer)
                {
                    SetCullingForRenderer(r, true, true);
                    continue;
                }


                float startTime = Time.realtimeSinceStartup;

                //NOTE(Brian): Need to retrieve positions every frame to take into account
                //             world repositioning.
                Vector3 playerPosition = CommonScriptableObjects.playerUnityPosition;

                Bounds bounds = r.bounds;
                Vector3 boundingPoint = bounds.ClosestPoint(playerPosition);
                float distance = Vector3.Distance(playerPosition, boundingPoint);
                bool boundsContainsPlayer = bounds.Contains(playerPosition);
                float boundsSize = bounds.size.magnitude;
                float viewportSize = (boundsSize / distance) * Mathf.Rad2Deg;

                bool isEmissive = IsEmissive(r);
                bool isOpaque = IsOpaque(r);

                float shadowTexelSize = ComputeShadowMapTexelSize(boundsSize, urpAsset.shadowDistance, urpAsset.mainLightShadowmapResolution);

                bool shouldBeVisible = TestRendererVisibleRule(profile, viewportSize, distance, boundsContainsPlayer, isOpaque, isEmissive);
                bool shouldHaveShadow = TestRendererShadowRule(profile, viewportSize, distance, shadowTexelSize);

                SetCullingForRenderer(r, shouldBeVisible, shouldHaveShadow);

                if (OnDataReport != null)
                {
                    if (!shouldBeVisible && !hiddenRenderers.Contains(r))
                        hiddenRenderers.Add(r);

                    if (shouldBeVisible && !shouldHaveShadow && !shadowlessRenderers.Contains(r))
                        shadowlessRenderers.Add(r);
                }

                if (r is SkinnedMeshRenderer skr)
                {
                    skr.updateWhenOffscreen = TestSkinnedRendererOffscreenRule(settings, distance);
                }
#if UNITY_EDITOR
                DrawDebugGizmos(shouldBeVisible, bounds, boundingPoint);
#endif
                timeBudgetCount += Time.realtimeSinceStartup - startTime;
            }
        }

        /// <summary>
        /// Main culling loop. Controlled by Start() and Stop() methods.
        /// </summary>
        IEnumerator UpdateCoroutine()
        {
            while (true)
            {
                bool shouldCheck = objectPositionsDirty || playerPositionDirty;

                playerPositionDirty = false;
                objectPositionsDirty = false;

                if (!shouldCheck)
                {
                    timeBudgetCount = 0;
                    yield return null;
                    continue;
                }

                yield return objectsTracker.PopulateRenderersList();

                if (resetObjectsNextFrame)
                {
                    ResetObjects();
                    resetObjectsNextFrame = false;
                }

                yield return ProcessAnimations();

                if (OnDataReport != null)
                {
                    hiddenRenderers.Clear();
                    shadowlessRenderers.Clear();
                }

                int profilesCount = profiles.Count;

                for (var pIndex = 0; pIndex < profilesCount; pIndex++)
                {
                    yield return ProcessProfile(profiles[pIndex]);
                }

                RaiseDataReport();
                timeBudgetCount = 0;
                yield return null;
            }
        }

        /// <summary>
        /// Sets shadows and visibility for a given renderer.
        /// </summary>
        /// <param name="r">Renderer to be culled</param>
        /// <param name="shouldBeVisible">If false, the renderer visibility will be set to false.</param>
        /// <param name="shouldHaveShadow">If false, the renderer shadow will be toggled off.</param>
        internal void SetCullingForRenderer(Renderer r, bool shouldBeVisible, bool shouldHaveShadow)
        {
            var targetMode = shouldHaveShadow ? ShadowCastingMode.On : ShadowCastingMode.Off;

            if (settings.enableObjectCulling)
            {
                if (r.forceRenderingOff != !shouldBeVisible)
                    r.forceRenderingOff = !shouldBeVisible;
            }

            if (settings.enableShadowCulling)
            {
                if (r.shadowCastingMode != targetMode)
                    r.shadowCastingMode = targetMode;
            }
        }

        /// <summary>
        /// Sets cullingType to all tracked animation components according to our culling rules.
        /// </summary>
        /// <returns>IEnumerator to be yielded.</returns>
        internal IEnumerator ProcessAnimations()
        {
            if (!settings.enableAnimationCulling)
                yield break;

            Animation[] animations = objectsTracker.GetAnimations();
            int animsLength = animations.Length;

            for (var i = 0; i < animsLength; i++)
            {
                if (timeBudgetCount > settings.maxTimeBudget)
                {
                    timeBudgetCount = 0;
                    yield return null;
                }

                Animation anim = animations[i];

                if (anim == null)
                    continue;

                float startTime = Time.realtimeSinceStartup;
                Transform t = anim.transform;

                Vector3 playerPosition = CommonScriptableObjects.playerUnityPosition;
                float distance = Vector3.Distance(playerPosition, t.position);

                if (distance > settings.enableAnimationCullingDistance)
                    anim.cullingType = AnimationCullingType.BasedOnRenderers;
                else
                    anim.cullingType = AnimationCullingType.AlwaysAnimate;

                timeBudgetCount += Time.realtimeSinceStartup - startTime;
            }
        }

        /// <summary>
        /// Reset all tracked renderers properties. Needed when toggling or changing settings.
        /// </summary>
        internal void ResetObjects()
        {
            var skinnedRenderers = objectsTracker.GetSkinnedRenderers();
            var renderers = objectsTracker.GetRenderers();
            var animations = objectsTracker.GetAnimations();

            for (var i = 0; i < skinnedRenderers.Length; i++)
            {
                skinnedRenderers[i].updateWhenOffscreen = true;
            }

            for (var i = 0; i < animations.Length; i++)
            {
                animations[i].cullingType = AnimationCullingType.AlwaysAnimate;
            }

            for (var i = 0; i < renderers.Length; i++)
            {
                renderers[i].forceRenderingOff = false;
            }
        }

        public void Dispose()
        {
            objectsTracker.Dispose();
            Stop();
        }

        /// <summary>
        /// Method suscribed to renderer state change
        /// </summary>
        private void OnRendererStateChange(bool rendererState, bool oldRendererState)
        {
            if (!running)
                return;

            MarkDirty();

            if (rendererState)
                StartInternal();
            else
                StopInternal();
        }

        /// <summary>
        /// Method suscribed to playerUnityPosition change
        /// </summary>
        private void OnPlayerUnityPositionChange(Vector3 previous, Vector3 current)
        {
            playerPositionDirty = true;
        }

        /// <summary>
        /// Sets the scene objects dirtiness.
        /// In the next update iteration, all the scene objects are going to be gathered.
        /// This method has performance impact.
        /// </summary>
        public void MarkDirty()
        {
            objectPositionsDirty = true;
        }

        /// <summary>
        /// Gets the scene objects dirtiness.
        /// </summary>
        public bool IsDirty()
        {
            return objectPositionsDirty;
        }

        /// <summary>
        /// Set settings. This will dirty the scene objects and has performance impact.
        /// </summary>
        /// <param name="settings">Settings to be set</param>
        public void SetSettings(CullingControllerSettings settings)
        {
            this.settings = settings;
            profiles = new List<CullingControllerProfile> {settings.rendererProfile, settings.skinnedRendererProfile};
            objectsTracker?.MarkDirty();
            MarkDirty();
        }

        /// <summary>
        /// Get current settings copy. If you need to modify it, you must set them via SetSettings afterwards.
        /// </summary>
        /// <returns>Current settings object copy.</returns>
        public CullingControllerSettings GetSettingsCopy()
        {
            return settings.Clone();
        }

        /// <summary>
        /// Enable or disable object visibility culling.
        /// </summary>
        /// <param name="enabled">If disabled, object visibility culling will be toggled.
        /// </param>
        public void SetObjectCulling(bool enabled)
        {
            if (settings.enableObjectCulling == enabled)
                return;

            settings.enableObjectCulling = enabled;
            resetObjectsNextFrame = true;
            MarkDirty();
            objectsTracker?.MarkDirty();
        }

        /// <summary>
        /// Enable or disable animation culling.
        /// </summary>
        /// <param name="enabled">If disabled, animation culling will be toggled.</param>
        public void SetAnimationCulling(bool enabled)
        {
            if (settings.enableAnimationCulling == enabled)
                return;

            settings.enableAnimationCulling = enabled;
            resetObjectsNextFrame = true;
            MarkDirty();
            objectsTracker?.MarkDirty();
        }

        /// <summary>
        /// Enable or disable shadow culling
        /// </summary>
        /// <param name="enabled">If disabled, no shadows will be toggled.</param>
        public void SetShadowCulling(bool enabled)
        {
            if (settings.enableShadowCulling == enabled)
                return;

            settings.enableShadowCulling = enabled;
            resetObjectsNextFrame = true;
            MarkDirty();
            objectsTracker?.MarkDirty();
        }

        /// <summary>
        /// Fire the DataReport event. This will be useful for showing stats in a debug panel.
        /// </summary>
        private void RaiseDataReport()
        {
            if (OnDataReport == null)
                return;

            int rendererCount = (objectsTracker.GetRenderers()?.Length ?? 0) + (objectsTracker.GetSkinnedRenderers()?.Length ?? 0);

            OnDataReport.Invoke(rendererCount, hiddenRenderers.Count, shadowlessRenderers.Count);
        }

        /// <summary>
        /// Returns true if the culling loop is running
        /// </summary>
        public bool IsRunning()
        {
            return updateCoroutine != null;
        }

        /// <summary>
        /// Draw debug gizmos on the scene view.
        /// </summary>
        /// <param name="shouldBeVisible"></param>
        /// <param name="bounds"></param>
        /// <param name="boundingPoint"></param>
        private static void DrawDebugGizmos(bool shouldBeVisible, Bounds bounds, Vector3 boundingPoint)
        {
            if (!shouldBeVisible)
            {
                CullingControllerUtils.DrawBounds(bounds, Color.blue, 1);
                CullingControllerUtils.DrawBounds(new Bounds() {center = boundingPoint, size = Vector3.one}, Color.red, 1);
            }
        }
    }
}