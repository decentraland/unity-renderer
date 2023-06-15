using JetBrains.Annotations;
using Sentry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace DCL.Components
{
    public class RendereableAssetLoadHelper
    {
        private const string NEW_CDN_FF = "ab-new-cdn";

        public event Action<Rendereable> OnSuccessEvent;
        public event Action<Exception> OnFailEvent;

        public enum LoadingType
        {
            ASSET_BUNDLE_WITH_GLTF_FALLBACK,
            ASSET_BUNDLE_ONLY,
            GLTF_ONLY,
            DEFAULT
        }

        private const string AB_GO_NAME_PREFIX = "AB:";
        private const string GLTFAST_GO_NAME_PREFIX = "GLTFast:";
        private const string FROM_ASSET_BUNDLE_TAG = "FromAssetBundle";
        private const string FROM_RAW_GLTF_TAG = "FromRawGLTF";

        public static bool VERBOSE = false;
        public static bool useCustomContentServerUrl = false;
        public static string customContentServerUrl;
        public static LoadingType defaultLoadingType = LoadingType.ASSET_BUNDLE_WITH_GLTF_FALLBACK;

        public AssetPromiseSettings_Rendering settings = new ();
        public Rendereable loadedAsset { get; protected set; }

        private readonly string bundlesContentUrl;
        private readonly ContentProvider contentProvider;
        private AssetPromise_GLTFast_Instance gltfastPromise;
        private AssetPromise_AB_GameObject abPromise;
        private string currentLoadingSystem;
        private FeatureFlag featureFlags => DataStore.i.featureFlags.flags.Get();
        private string targetUrl;

        public IEnumerator Promise
        {
            get
            {
                if (gltfastPromise != null)
                    yield return gltfastPromise;

                if (abPromise != null)
                    yield return abPromise;

                yield return null;
            }
        }

        public bool IsFinished
        {
            get
            {
                if (gltfastPromise != null)
                    return gltfastPromise.state == AssetPromiseState.FINISHED;

                if (abPromise != null)
                    return abPromise.state == AssetPromiseState.FINISHED;

                return false;
            }
        }

#if UNITY_EDITOR
        public override string ToString()
        {
            float loadTime = Mathf.Min(loadFinishTime, Time.realtimeSinceStartup) - loadStartTime;

            string result = "not loading";

            if (abPromise != null) { result = $"ASSET BUNDLE -> promise state = {abPromise.ToString()} ({loadTime} load time)... waiting promises = {AssetPromiseKeeper_AB.i.waitingPromisesCount}"; }

            return result;
        }

        float loadStartTime = 0;
        float loadFinishTime = float.MaxValue;
#endif

        public RendereableAssetLoadHelper(ContentProvider contentProvider, string bundlesContentUrl)
        {
            this.contentProvider = contentProvider;
            this.bundlesContentUrl = bundlesContentUrl;
        }

        public void Load(string targetUrl, LoadingType forcedLoadingType = LoadingType.DEFAULT)
        {
            this.targetUrl = targetUrl;
            Assert.IsFalse(string.IsNullOrEmpty(targetUrl), "url is null!!");
#if UNITY_EDITOR
            loadStartTime = Time.realtimeSinceStartup;
#endif
            LoadingType finalLoadingType = forcedLoadingType == LoadingType.DEFAULT ? defaultLoadingType : forcedLoadingType;

            switch (finalLoadingType)
            {
                case LoadingType.ASSET_BUNDLE_ONLY:
                    LoadAssetBundle(targetUrl, OnSuccessEvent, OnFailEvent, false);
                    break;
                case LoadingType.GLTF_ONLY:
                    LoadGLTFast(targetUrl, OnSuccessEvent, OnFailEvent, false);
                    break;
                case LoadingType.DEFAULT:
                case LoadingType.ASSET_BUNDLE_WITH_GLTF_FALLBACK:
                    LoadAssetBundle(targetUrl, OnSuccessEvent, exception => LoadGLTFast(targetUrl, OnSuccessEvent, OnFailEvent, false), true);
                    break;
            }
        }

        public void Unload()
        {
            UnloadAB();
            UnloadGLTFast();
        }

        void UnloadAB()
        {
            if (abPromise != null) { AssetPromiseKeeper_AB_GameObject.i.Forget(abPromise); }
        }

        void UnloadGLTFast()
        {
            if (gltfastPromise != null) { AssetPromiseKeeper_GLTFast_Instance.i.Forget(gltfastPromise); }
        }

        void LoadAssetBundle(string targetUrl, Action<Rendereable> OnSuccess, Action<Exception> OnFail, bool hasFallback)
        {
            currentLoadingSystem = AB_GO_NAME_PREFIX;

            if (abPromise != null)
            {
                UnloadAB();

                if (VERBOSE)
                    Debug.Log("Forgetting not null promise..." + targetUrl);
            }

            string bundlesBaseUrl = useCustomContentServerUrl ? customContentServerUrl : bundlesContentUrl;

            if (string.IsNullOrEmpty(bundlesBaseUrl))
            {
                OnFailWrapper(OnFail, new Exception("bundlesBaseUrl is null"), hasFallback);
                return;
            }

            if (!contentProvider.TryGetContentsUrl_Raw(targetUrl, out string hash))
            {
                OnFailWrapper(OnFail, new Exception($"Content url does not contains {targetUrl}"), hasFallback);
                return;
            }

            if (featureFlags.IsFeatureEnabled(NEW_CDN_FF))
            {
                if (contentProvider.assetBundles.Contains(hash))
                    bundlesBaseUrl = contentProvider.assetBundlesBaseUrl;
                else
                {
                    // we track the failing asset for it to be fixed in the asset bundle converter
                    SentrySdk.CaptureMessage("Scene Asset not converted to AssetBundles", scope =>
                    {
                        scope.SetExtra("hash", hash);
                        scope.SetExtra("baseUrl", contentProvider.assetBundlesBaseUrl);
                        scope.SetExtra("sceneCid", contentProvider.sceneCid);
                    });

                    // exception is null since we are expected to fallback
                    OnFailWrapper(OnFail, null, hasFallback);
                    return;
                }
            }

            abPromise = new AssetPromise_AB_GameObject(bundlesBaseUrl, hash);
            abPromise.settings = this.settings;

            abPromise.OnSuccessEvent += (x) =>
            {
#if UNITY_EDITOR
                x.container.name = AB_GO_NAME_PREFIX + hash + " - " + contentProvider.assetBundlesVersion;
#endif
                var r = new Rendereable()
                {
                    container = x.container,
                    totalTriangleCount = x.totalTriangleCount,
                    meshes = x.meshes,
                    renderers = x.renderers,
                    materials = x.materials,
                    textures = x.textures,
                    meshToTriangleCount = x.meshToTriangleCount,
                    animationClipSize = x.animationClipSize,
                    animationClips = x.animationClips,
                    meshDataSize = x.meshDataSize
                };

                foreach (var someRenderer in r.renderers)
                    someRenderer.tag = FROM_ASSET_BUNDLE_TAG;

                OnSuccessWrapper(r, OnSuccess);
            };

            abPromise.OnFailEvent += (x, exception) => OnFailWrapper(OnFail, exception, hasFallback);

            AssetPromiseKeeper_AB_GameObject.i.Keep(abPromise);
        }

        private void LoadGLTFast(string targetUrl, Action<Rendereable> OnSuccess, Action<Exception> OnFail, bool hasFallback)
        {
            currentLoadingSystem = GLTFAST_GO_NAME_PREFIX;

            if (gltfastPromise != null)
            {
                UnloadGLTFast();

                if (VERBOSE)
                    Debug.Log("Forgetting not null promise... " + targetUrl);
            }

            if (!contentProvider.TryGetContentsUrl_Raw(targetUrl, out string hash))
            {
                OnFailWrapper(OnFail, new Exception($"Content provider does not contains url {targetUrl}"), hasFallback);
                return;
            }

            gltfastPromise = new AssetPromise_GLTFast_Instance(targetUrl, hash,
                Environment.i.platform.webRequest, contentProvider, settings);

            gltfastPromise.OnSuccessEvent += (Asset_GLTFast_Instance x) =>
            {
#if UNITY_EDITOR
                x.container.name = GLTFAST_GO_NAME_PREFIX + hash;
#endif
                Rendereable r = x.ToRendereable();

                foreach (var someRenderer in r.renderers)
                    someRenderer.tag = FROM_RAW_GLTF_TAG;

                OnSuccessWrapper(r, OnSuccess);
            };

            gltfastPromise.OnFailEvent += (asset, exception) =>
            {
                OnFailWrapper(OnFail, exception, hasFallback);
            };

            AssetPromiseKeeper_GLTFast_Instance.i.Keep(gltfastPromise);
        }

        private void OnFailWrapper(Action<Exception> OnFail, Exception exception, bool hasFallback)
        {
#if UNITY_EDITOR
            loadFinishTime = Time.realtimeSinceStartup;
#endif

            // If the entity is destroyed while loading, the exception is expected to be null and no error should be thrown
            if (exception != null)
            {
                if (!hasFallback)
                    Debug.LogWarning("All fallbacks failed for " + targetUrl);
                else if (VERBOSE)
                    Debug.LogWarning($"Load Fail Detected, trying to use a fallback, " +
                                     $"loading type was: {currentLoadingSystem} and error was: {exception.Message}");
            }

            OnFail?.Invoke(exception);
            ClearEvents();
        }

        private void OnSuccessWrapper(Rendereable loadedAsset, Action<Rendereable> OnSuccess)
        {
#if UNITY_EDITOR
            loadFinishTime = Time.realtimeSinceStartup;
#endif
            if (VERBOSE)
            {
                if (gltfastPromise != null)
                    Debug.Log($"GLTF Load(): target URL -> {gltfastPromise.GetId()}. Success!");
                else
                    Debug.Log($"AB Load(): target URL -> {abPromise.hash}. Success!");
            }

            this.loadedAsset = loadedAsset;
            OnSuccess?.Invoke(loadedAsset);
            ClearEvents();
        }

        public void ClearEvents()
        {
            OnSuccessEvent = null;
            OnFailEvent = null;
        }

        private void SendMetric(string eventToSend, string targetUrl, string reason)
        {
            GenericAnalytics.SendAnalytic(eventToSend, new Dictionary<string, string> { { "targetUrl", targetUrl }, { "reason", reason }, { "platform", Application.platform.ToString() } });
        }
    }
}
