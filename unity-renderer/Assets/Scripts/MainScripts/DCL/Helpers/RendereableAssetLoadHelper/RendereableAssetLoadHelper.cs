using UnityEngine;
using UnityEngine.Assertions;

namespace DCL.Components
{
    public class RendereableAssetLoadHelper
    {
        public enum LoadingType
        {
            ASSET_BUNDLE_WITH_GLTF_FALLBACK,
            ASSET_BUNDLE_ONLY,
            GLTF_ONLY
        }

        public static bool VERBOSE = false;

        public static bool useCustomContentServerUrl = false;
        public static string customContentServerUrl;

        public static LoadingType loadingType = LoadingType.ASSET_BUNDLE_WITH_GLTF_FALLBACK;

        public AssetPromiseSettings_Rendering settings = new AssetPromiseSettings_Rendering();

        public Rendereable loadedAsset { get; protected set; }

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

        string bundlesContentUrl;
        ContentProvider contentProvider;

        AssetPromise_GLTF gltfPromise;
        AssetPromise_AB_GameObject abPromise;

#if UNITY_EDITOR
        public override string ToString()
        {
            float loadTime = Mathf.Min(loadFinishTime, Time.realtimeSinceStartup) - loadStartTime;

            string result = "not loading";

            if (gltfPromise != null)
            {
                result = $"GLTF -> promise state = {gltfPromise.state} ({loadTime} load time)... waiting promises = {AssetPromiseKeeper_GLTF.i.waitingPromisesCount}";

                if (gltfPromise.state == AssetPromiseState.WAITING)
                {
                    result += $"\nmaster promise state... is blocked... {AssetPromiseKeeper_GLTF.i.GetMasterState(gltfPromise)}";
                }
            }

            if (abPromise != null)
            {
                result = $"ASSET BUNDLE -> promise state = {abPromise.ToString()} ({loadTime} load time)... waiting promises = {AssetPromiseKeeper_AB.i.waitingPromisesCount}";
            }

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

        public event System.Action<Rendereable> OnSuccessEvent;
        public event System.Action OnFailEvent;

        public void Load(string targetUrl)
        {
            Assert.IsFalse(string.IsNullOrEmpty(targetUrl), "url is null!!");
#if UNITY_EDITOR
            loadStartTime = Time.realtimeSinceStartup;
#endif
            switch (loadingType)
            {
                case LoadingType.ASSET_BUNDLE_WITH_GLTF_FALLBACK:
                    LoadAssetBundle(targetUrl, OnSuccessEvent, () => LoadGltf(targetUrl, OnSuccessEvent, OnFailEvent));
                    break;
                case LoadingType.ASSET_BUNDLE_ONLY:
                    LoadAssetBundle(targetUrl, OnSuccessEvent, OnFailEvent);
                    break;
                case LoadingType.GLTF_ONLY:
                    LoadGltf(targetUrl, OnSuccessEvent, OnFailEvent);
                    break;
            }
        }

        public void Unload()
        {
            UnloadAB();
            UnloadGLTF();
        }

        void UnloadAB()
        {
            if ( abPromise != null )
            {
                AssetPromiseKeeper_AB_GameObject.i.Forget(abPromise);
            }
        }

        void UnloadGLTF()
        {
            if ( gltfPromise != null )
            {
                AssetPromiseKeeper_GLTF.i.Forget(gltfPromise);
            }
        }

        void LoadAssetBundle(string targetUrl, System.Action<Rendereable> OnSuccess, System.Action OnFail)
        {
            if (abPromise != null)
            {
                UnloadAB();
                if (VERBOSE)
                    Debug.Log("Forgetting not null promise..." + targetUrl);
            }

            string bundlesBaseUrl = useCustomContentServerUrl ? customContentServerUrl : bundlesContentUrl;

            if (string.IsNullOrEmpty(bundlesBaseUrl))
            {
                OnFailWrapper(null, OnFail);
                return;
            }

            if (!contentProvider.TryGetContentsUrl_Raw(targetUrl, out string hash))
            {
                OnFailWrapper(null, OnFail);
                return;
            }

            abPromise = new AssetPromise_AB_GameObject(bundlesBaseUrl, hash);
            abPromise.settings = this.settings;

            abPromise.OnSuccessEvent += (x) =>
            {
                var r = new Rendereable()
                {
                    container = x.container,
                    totalTriangleCount = x.totalTriangleCount,
                    meshes = x.meshes,
                    renderers = x.renderers,
                    meshToTriangleCount = x.meshToTriangleCount
                };

                OnSuccessWrapper(r, OnSuccess);
            };

            abPromise.OnFailEvent += (x) => OnFailWrapper(x, OnFail);

            AssetPromiseKeeper_AB_GameObject.i.Keep(abPromise);
        }

        void LoadGltf(string targetUrl, System.Action<Rendereable> OnSuccess, System.Action OnFail)
        {
            if (gltfPromise != null)
            {
                UnloadGLTF();

                if (VERBOSE)
                    Debug.Log("Forgetting not null promise... " + targetUrl);
            }

            if (!contentProvider.TryGetContentsUrl_Raw(targetUrl, out string hash))
            {
                OnFailWrapper(null, OnFail);
                return;
            }

            gltfPromise = new AssetPromise_GLTF(contentProvider, targetUrl, hash);
            gltfPromise.settings = this.settings;

            gltfPromise.OnSuccessEvent += (x) =>
            {
                var r = new Rendereable()
                {
                    container = x.container,
                    totalTriangleCount = x.totalTriangleCount,
                    meshes = x.meshes,
                    renderers = x.renderers,
                    meshToTriangleCount = x.meshToTriangleCount
                };

                OnSuccessWrapper(r, OnSuccess);
            };
            gltfPromise.OnFailEvent += (x) => OnFailWrapper(x, OnFail);

            AssetPromiseKeeper_GLTF.i.Keep(gltfPromise);
        }

        private void OnFailWrapper(Asset_WithPoolableContainer loadedAsset, System.Action OnFail)
        {
#if UNITY_EDITOR
            loadFinishTime = Time.realtimeSinceStartup;
#endif


            OnFail?.Invoke();
            ClearEvents();
        }

        private void OnSuccessWrapper(Rendereable loadedAsset, System.Action<Rendereable> OnSuccess)
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
    }
}