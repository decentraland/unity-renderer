using JetBrains.Annotations;
using MainScripts.DCL.Controllers.AssetManager.AssetBundles.SceneAB;
using Sentry;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace DCL.Components
{
    public class RendereableAssetLoadHelper
    {
        private const string NEW_CDN_FF = "ab-new-cdn";
        private const string FAILED_GLTFAST_LOAD_EVENT = "failed_GLTFast_load";
        private const string SUCESS_GLTF_LOAD_AFTER_GLTFAST_FAIL_EVENT = "success_GLTF_load_after_GLTFast_fail";
        private const string FAIL_GLTF_LOAD_AFTER_GLTFAST_FAIL_EVENT = "failed_GLTF_load_after_GLTFast_fail";

        public event Action<Rendereable> OnSuccessEvent;
        public event Action<Exception> OnFailEvent;

        public enum LoadingType
        {
            ASSET_BUNDLE_WITH_GLTF_FALLBACK,
            ASSET_BUNDLE_WITH_OLD_GLTF_FALLBACK,
            ASSET_BUNDLE_ONLY,
            GLTF_ONLY,
            OLD_GLTF,
            DEFAULT
        }

        private const string AB_GO_NAME_PREFIX = "AB:";
        private const string GLTF_GO_NAME_PREFIX = "GLTF:";
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
        private readonly Func<bool> IsGltFastEnabled;
        private readonly ContentProvider contentProvider;
        private AssetPromise_GLTF gltfPromise;
        private AssetPromise_GLTFast_Instance gltfastPromise;
        private AssetPromise_AB_GameObject abPromise;
        private string currentLoadingSystem;
        private FeatureFlag featureFlags => DataStore.i.featureFlags.flags.Get();

        public bool isFinished
        {
            get
            {
                if (gltfPromise != null)
                    return gltfPromise.state == AssetPromiseState.FINISHED;

                if (abPromise != null)
                    return abPromise.state == AssetPromiseState.FINISHED;

                return true;
            }
        }

#if UNITY_EDITOR
        public override string ToString()
        {
            float loadTime = Mathf.Min(loadFinishTime, Time.realtimeSinceStartup) - loadStartTime;

            string result = "not loading";

            if (gltfPromise != null)
            {
                result = $"GLTF -> promise state = {gltfPromise.state} ({loadTime} load time)... waiting promises = {AssetPromiseKeeper_GLTF.i.waitingPromisesCount}";

                if (gltfPromise.state == AssetPromiseState.WAITING) { result += $"\nmaster promise state... is blocked... {AssetPromiseKeeper_GLTF.i.GetMasterState(gltfPromise)}"; }
            }

            if (abPromise != null) { result = $"ASSET BUNDLE -> promise state = {abPromise.ToString()} ({loadTime} load time)... waiting promises = {AssetPromiseKeeper_AB.i.waitingPromisesCount}"; }

            return result;
        }

        float loadStartTime = 0;
        float loadFinishTime = float.MaxValue;
#endif

        public RendereableAssetLoadHelper(ContentProvider contentProvider, string bundlesContentUrl, Func<bool> isGltFastEnabled)
        {
            this.contentProvider = contentProvider;
            this.bundlesContentUrl = bundlesContentUrl;
            this.IsGltFastEnabled = isGltFastEnabled;
        }

        public void Load(string targetUrl, LoadingType forcedLoadingType = LoadingType.DEFAULT)
        {
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
                    ProxyLoadGltf(targetUrl, false);
                    break;
                case LoadingType.OLD_GLTF:
                    LoadGltf(targetUrl, OnSuccessEvent, OnFailEvent, false);
                    break;
                case LoadingType.DEFAULT:
                case LoadingType.ASSET_BUNDLE_WITH_GLTF_FALLBACK:
                    LoadAssetBundle(targetUrl, OnSuccessEvent, exception => ProxyLoadGltf(targetUrl, false), true);
                    break;
                case LoadingType.ASSET_BUNDLE_WITH_OLD_GLTF_FALLBACK:
                    LoadAssetBundle(targetUrl, OnSuccessEvent, exception => LoadGltf(targetUrl, OnSuccessEvent, OnFailEvent, false), true);
                    break;
            }
        }

        private void ProxyLoadGltf(string targetUrl, bool hasFallback)
        {
            if (IsGltFastEnabled())
                LoadGLTFast(targetUrl, OnSuccessEvent, exception => { OnGLTFastLoadFail(targetUrl, hasFallback, exception); }, true);
            else
                LoadGltf(targetUrl, OnSuccessEvent, OnFailEvent, hasFallback);
        }

        private void OnGLTFastLoadFail(string targetUrl, bool hasFallback, Exception exception)
        {
            SendMetric(FAILED_GLTFAST_LOAD_EVENT, targetUrl, exception.Message);

            if (VERBOSE)
                Debug.Log($"GLTFast failed to load for {targetUrl} so we are going to fallback into old gltf");

            LoadGltf(targetUrl, OnSuccessEvent, OnFailEvent, hasFallback, true);
        }

        public void Unload()
        {
            UnloadAB();
            UnloadGLTF();
            UnloadGLTFast();
        }

        void UnloadAB()
        {
            if (abPromise != null) { AssetPromiseKeeper_AB_GameObject.i.Forget(abPromise); }
        }

        void UnloadGLTF()
        {
            if (gltfPromise != null) { AssetPromiseKeeper_GLTF.i.Forget(gltfPromise); }
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
                x.container.name = AB_GO_NAME_PREFIX + x.container.name;
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

        void LoadGltf(string targetUrl, Action<Rendereable> OnSuccess, Action<Exception> OnFail, bool hasFallback, bool sendMetric = false)
        {
            currentLoadingSystem = GLTF_GO_NAME_PREFIX;

            if (gltfPromise != null)
            {
                UnloadGLTF();

                if (VERBOSE)
                    Debug.Log("Forgetting not null promise... " + targetUrl);
            }

            if (!contentProvider.TryGetContentsUrl_Raw(targetUrl, out string hash))
            {
                if (sendMetric)
                    SendMetric(FAIL_GLTF_LOAD_AFTER_GLTFAST_FAIL_EVENT, targetUrl, $"Content provider does not contains url {targetUrl}");

                OnFailWrapper(OnFail, new Exception($"Content provider does not contains url {targetUrl}"), hasFallback);
                return;
            }

            gltfPromise = new AssetPromise_GLTF(contentProvider, targetUrl, hash);
            gltfPromise.settings = settings;

            gltfPromise.OnSuccessEvent += (Asset_GLTF x) =>
            {
#if UNITY_EDITOR
                x.container.name = GLTF_GO_NAME_PREFIX + x.container.name;
#endif
                var r = new Rendereable
                {
                    container = x.container,
                    totalTriangleCount = x.totalTriangleCount,
                    meshes = x.meshes,
                    renderers = x.renderers,
                    materials = x.materials,
                    textures = x.textures,
                    meshToTriangleCount = x.meshToTriangleCount,
                    animationClipSize = x.animationClipSize,
                    meshDataSize = x.meshDataSize,
                    animationClips = x.animationClips
                };

                foreach (var someRenderer in r.renderers)
                    someRenderer.tag = FROM_RAW_GLTF_TAG;

                if (sendMetric)
                    SendMetric(SUCESS_GLTF_LOAD_AFTER_GLTFAST_FAIL_EVENT, targetUrl, "");

                OnSuccessWrapper(r, OnSuccess);
            };

            gltfPromise.OnFailEvent += (asset, exception) =>
            {
                if (sendMetric)
                    SendMetric(FAIL_GLTF_LOAD_AFTER_GLTFAST_FAIL_EVENT, targetUrl, exception.Message);
                OnFailWrapper(OnFail, exception, hasFallback);
            };

            AssetPromiseKeeper_GLTF.i.Keep(gltfPromise);
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
                x.container.name = GLTFAST_GO_NAME_PREFIX + x.container.name;
#endif
                Rendereable r = x.ToRendereable();

                foreach (var someRenderer in r.renderers)
                    someRenderer.tag = FROM_RAW_GLTF_TAG;

                OnSuccessWrapper(r, OnSuccess);
            };

            gltfastPromise.OnFailEvent += (asset, exception) => { OnFailWrapper(OnFail, exception, hasFallback); };

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
                    Debug.LogException(exception);
                else if (VERBOSE)
                {
                    Debug.Log($"Load Fail Detected, trying to use a fallback, " +
                              $"loading type was: {currentLoadingSystem} and error was: {exception.Message}");
                }
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
                if (gltfPromise != null)
                    Debug.Log($"GLTF Load(): target URL -> {gltfPromise.GetId()}. Success!");
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
