using System.Collections;
using System.Collections.Generic;
using DCL.Models;
using UnityEngine;
using UnityEngine.Rendering;
using UniversalRenderPipelineAsset = UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
using static DCL.Rendering.CullingControllerUtils;

namespace DCL.Rendering
{
    /// <summary>
    /// CullingController has the following responsibilities:
    /// - Hides small renderers (detail objects).
    /// - Disable unneeded shadows.
    /// - Enable/disable animation culling for skinned renderers and animation components.
    /// </summary>
    public class CullingController : ICullingController
    {
        private const string ANIMATION_CULLING_STATUS_FEATURE_FLAG = "animation_culling_status";
        private const string SMR_UPDATE_OFFSCREEN_FEATURE_FLAG = "smr_update_offscreen";
        private const bool DRAW_GIZMOS = false;
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
        private bool offScreenUpdate = true;

        // Cache to avoid allocations when getting names
        private readonly HashSet<Shader> avatarShaders = new HashSet<Shader>();
        private readonly HashSet<Shader> nonAvatarShaders = new HashSet<Shader>();

        private BaseVariable<FeatureFlag> featureFlags => DataStore.i.featureFlags.flags;

        public event ICullingController.DataReport OnDataReport;

        public static CullingController Create()
        {
            return new CullingController(
                GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset,
                new CullingControllerSettings()
            );
        }

        private CullingController() { }

        public CullingController(UniversalRenderPipelineAsset urpAsset, CullingControllerSettings settings, ICullingObjectsTracker cullingObjectsTracker = null)
        {
            if (cullingObjectsTracker == null)
                objectsTracker = new CullingObjectsTracker();
            else
                objectsTracker = cullingObjectsTracker;

            objectsTracker.SetIgnoredLayersMask(settings.ignoredLayersMask);

            this.urpAsset = urpAsset;
            this.settings = settings;

            featureFlags.OnChange += OnFeatureFlagChange;
            OnFeatureFlagChange(featureFlags.Get(), null);
        }

        private void OnFeatureFlagChange(FeatureFlag current, FeatureFlag previous)
        {
            SetAnimationCulling(current.IsFeatureEnabled(ANIMATION_CULLING_STATUS_FEATURE_FLAG));
            offScreenUpdate = current.IsFeatureEnabled(SMR_UPDATE_OFFSCREEN_FEATURE_FLAG);
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
            objectsTracker?.MarkDirty();
            StartInternal();
        }

        public void Restart()
        {
            Stop();
            Start();
        }

        private void StartInternal()
        {
            if (updateCoroutine != null)
                return;

            RaiseDataReport();
            profiles = new List<CullingControllerProfile> { settings.rendererProfile, settings.skinnedRendererProfile };
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
            objectsTracker?.ForcePopulateRenderersList();
            ResetObjects();
        }

        private void StopInternal()
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
            // If profile matches the skinned renderer profile in settings the skinned renderers are going to be used.
            IReadOnlyList<Renderer> renderers = profile ==
                settings.rendererProfile ?
                objectsTracker.GetRenderers() :
                objectsTracker.GetSkinnedRenderers();

            yield return settings.enableShadowCulling
                ? ProcessProfileWithEnabledCulling(profile, renderers)
                : (object)ProcessProfileWithDisabledCulling(profile, renderers);
        }

        internal IEnumerator ProcessProfileWithEnabledCulling(CullingControllerProfile profile, IReadOnlyList<Renderer> renderers)
        {
            Vector3 playerPosition = CommonScriptableObjects.playerUnityPosition;
            float currentStartTime = Time.realtimeSinceStartup;

            foreach (Renderer r in renderers)
            {
                if (r == null)
                    continue;

                if (Time.realtimeSinceStartup - currentStartTime >= CullingControllerSettings.MAX_TIME_BUDGET)
                {
                    yield return null;
                    playerPosition = CommonScriptableObjects.playerUnityPosition;
                    currentStartTime = Time.realtimeSinceStartup;
                }

                Bounds bounds = MeshesInfoUtils.GetSafeBounds(r.bounds, r.transform.position);
                Vector3 boundingPoint = bounds.ClosestPoint(playerPosition);

                float distance = Vector3.Distance(playerPosition, boundingPoint);
                float boundsSize = bounds.size.magnitude;
                float viewportSize = (boundsSize / distance) * Mathf.Rad2Deg;

                float shadowTexelSize = ComputeShadowMapTexelSize(boundsSize, urpAsset.shadowDistance, urpAsset.mainLightShadowmapResolution);

                bool shouldBeVisible =
                    distance < profile.visibleDistanceThreshold ||
                    bounds.Contains(playerPosition) ||
                    // At the end we perform queries for emissive and opaque conditions
                    // these are the last conditions because IsEmissive and IsOpaque are a bit more costly
                    viewportSize > profile.emissiveSizeThreshold && IsEmissive(r) ||
                    viewportSize > profile.opaqueSizeThreshold && IsOpaque(r)
                ;

                bool shouldHaveShadow = !settings.enableShadowCulling || TestRendererShadowRule(profile, viewportSize, distance, shadowTexelSize);

                if (r is SkinnedMeshRenderer skr)
                {
                    Material mat = skr.sharedMaterial;

                    if (IsAvatarRenderer(mat))
                        shouldHaveShadow &= TestAvatarShadowRule(profile, distance);

                    skr.updateWhenOffscreen = offScreenUpdate;
                }

                if (OnDataReport != null)
                {
                    if (!shouldBeVisible && !hiddenRenderers.Contains(r))
                        hiddenRenderers.Add(r);

                    if (shouldBeVisible && !shouldHaveShadow && !shadowlessRenderers.Contains(r))
                        shadowlessRenderers.Add(r);
                }

                SetCullingForRenderer(r, shouldBeVisible, shouldHaveShadow);
#if UNITY_EDITOR
                if (DRAW_GIZMOS)
                    DrawDebugGizmos(shouldBeVisible, bounds, boundingPoint);
#endif
            }
        }

        internal IEnumerator ProcessProfileWithDisabledCulling(CullingControllerProfile profile, IEnumerable<Renderer> renderers)
        {
            Vector3 playerPosition = CommonScriptableObjects.playerUnityPosition;
            float currentStartTime = Time.realtimeSinceStartup;
            foreach (Renderer r in renderers)
            {
                if (r == null)
                    continue;

                if (Time.realtimeSinceStartup - currentStartTime >= CullingControllerSettings.MAX_TIME_BUDGET)
                {
                    yield return null;
                    playerPosition = CommonScriptableObjects.playerUnityPosition;
                    currentStartTime = Time.realtimeSinceStartup;
                }

                Bounds bounds = MeshesInfoUtils.GetSafeBounds(r.bounds, r.transform.position);
                Vector3 boundingPoint = bounds.ClosestPoint(playerPosition);

                float distance = Vector3.Distance(playerPosition, boundingPoint);
                float boundsSize = bounds.size.magnitude;
                float viewportSize = (boundsSize / distance) * Mathf.Rad2Deg;

                float shadowTexelSize = ComputeShadowMapTexelSize(boundsSize, urpAsset.shadowDistance, urpAsset.mainLightShadowmapResolution);
                bool shouldHaveShadow = TestRendererShadowRule(profile, viewportSize, distance, shadowTexelSize);

                if (r is SkinnedMeshRenderer skr)
                    skr.updateWhenOffscreen = offScreenUpdate;

                if (OnDataReport != null)
                {
                    if (!shouldHaveShadow && !shadowlessRenderers.Contains(r))
                        shadowlessRenderers.Add(r);
                }

                SetCullingForRenderer(r, true, shouldHaveShadow);

#if UNITY_EDITOR
                if (DRAW_GIZMOS)
                    DrawDebugGizmos(true, bounds, boundingPoint);
#endif
            }
        }

        /// <summary>
        /// Checks if the material is from an Avatar by checking if the shader is DCL/Toon Shader
        /// This Method avoids the allocation of the name getter by storing the result on a HashSet
        /// </summary>
        /// <param name="mat"></param>
        /// <returns></returns>
        private bool IsAvatarRenderer(Material mat)
        {
            if (mat != null && mat.shader != null)
            {
                Shader matShader = mat.shader;

                if (!avatarShaders.Contains(matShader) && !nonAvatarShaders.Contains(matShader))
                {
                    // This allocates memory on the GC
                    bool isAvatar = matShader.name == "DCL/Toon Shader";

                    if (isAvatar)
                        avatarShaders.Add(matShader);
                    else
                        nonAvatarShaders.Add(matShader);
                }

                return avatarShaders.Contains(matShader);

            }

            return false;
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
                for (int profileIndex = 0; profileIndex < profilesCount; profileIndex++)
                    yield return ProcessProfile(profiles[profileIndex]);

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

            if (r.forceRenderingOff != !shouldBeVisible)
                r.forceRenderingOff = !shouldBeVisible;

            if (r.shadowCastingMode != targetMode)
                r.shadowCastingMode = targetMode;
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
                if (timeBudgetCount > CullingControllerSettings.MAX_TIME_BUDGET)
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
            IEnumerable<Renderer> renderers = objectsTracker.GetRenderers();
            IEnumerable<SkinnedMeshRenderer> skinnedRenderers = objectsTracker.GetSkinnedRenderers();
            Animation[] animations = objectsTracker.GetAnimations();

            foreach (Renderer renderer in renderers)
            {
                if (renderer != null)
                    renderer.forceRenderingOff = false;
            }

            foreach (SkinnedMeshRenderer skinnedRenderer in skinnedRenderers)
            {
                if (skinnedRenderer != null)
                    skinnedRenderer.updateWhenOffscreen = offScreenUpdate;
            }

            for (int i = 0; i < animations?.Length; i++)
            {
                if (animations[i] != null)
                    animations[i].cullingType = AnimationCullingType.AlwaysAnimate;
            }
        }

        public void Dispose()
        {
            objectsTracker.Dispose();
            Stop();
            featureFlags.OnChange -= OnFeatureFlagChange;
        }

        public void Initialize()
        {
            Start();
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
        private void OnPlayerUnityPositionChange(Vector3 previous, Vector3 current) { playerPositionDirty = true; }

        /// <summary>
        /// Sets the scene objects dirtiness.
        /// In the next update iteration, all the scene objects are going to be gathered.
        /// This method has performance impact.
        /// </summary>
        public void MarkDirty() { objectPositionsDirty = true; }

        /// <summary>
        /// Gets the scene objects dirtiness.
        /// </summary>
        public bool IsDirty() { return objectPositionsDirty; }

        /// <summary>
        /// Set settings. This will dirty the scene objects and has performance impact.
        /// </summary>
        /// <param name="settings">Settings to be set</param>
        public void SetSettings(CullingControllerSettings settings)
        {
            this.settings = settings;
            profiles = new List<CullingControllerProfile> { settings.rendererProfile, settings.skinnedRendererProfile };

            objectsTracker?.SetIgnoredLayersMask(settings.ignoredLayersMask);
            objectsTracker?.MarkDirty();
            MarkDirty();
            resetObjectsNextFrame = true;
        }

        /// <summary>
        /// Get current settings copy. If you need to modify it, you must set them via SetSettings afterwards.
        /// </summary>
        /// <returns>Current settings object copy.</returns>
        public CullingControllerSettings GetSettingsCopy() { return settings.Clone(); }

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

            int rendererCount = (objectsTracker.GetRenderers()?.Count ?? 0) + (objectsTracker.GetSkinnedRenderers()?.Count ?? 0);

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
                DrawBounds(bounds, Color.blue, 1);
                DrawBounds(new Bounds() { center = boundingPoint, size = Vector3.one }, Color.red, 1);
            }
        }
    }
}
