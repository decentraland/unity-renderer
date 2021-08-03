using DCL.Configuration;
using DCL.Helpers;
using UnityEngine;
using UnityGLTF;

namespace DCL
{
    public class AssetPromise_GLTF : AssetPromise<Asset_GLTF>
    {
        public AssetPromiseSettings_Rendering settings = new AssetPromiseSettings_Rendering();
        protected string assetDirectoryPath;

        protected ContentProvider provider = null;
        public string url { get; private set; }

        GLTFComponent gltfComponent = null;
        object id = null;

        private System.Action OnSuccess;
        private System.Action OnFail;
        private bool waitingAssetLoad = false;

        public AssetPromise_GLTF(ContentProvider provider, string url, string hash = null)
        {
            this.provider = provider;
            this.url = url.Substring(url.LastIndexOf('/') + 1);
            this.id = hash ?? url;
            // We separate the directory path of the GLB and its file name, to be able to use the directory path when 
            // fetching relative assets like textures in the ParseGLTFWebRequestedFile() event call
            assetDirectoryPath = URIHelper.GetDirectoryName(url);
        }

        protected override void OnBeforeLoadOrReuse()
        {
#if UNITY_EDITOR
            asset.container.name = "GLTF: " + this.id;
#endif
            settings.ApplyBeforeLoad(asset.container.transform);
        }

        protected override void OnAfterLoadOrReuse()
        {
            if (asset.container != null)
            {
                settings.ApplyAfterLoad(asset.container.transform);
            }
        }

        public override object GetId() { return id; }

        protected override void OnLoad(System.Action OnSuccess, System.Action OnFail)
        {
            gltfComponent = asset.container.AddComponent<GLTFComponent>();
            gltfComponent.Initialize(DCL.Environment.i.platform.webRequest);

            GLTFComponent.Settings tmpSettings = new GLTFComponent.Settings()
            {
                useVisualFeedback = settings.visibleFlags == AssetPromiseSettings_Rendering.VisibleFlags.VISIBLE_WITH_TRANSITION,
                initialVisibility = settings.visibleFlags != AssetPromiseSettings_Rendering.VisibleFlags.INVISIBLE,
                shaderOverride = settings.shaderOverride,
                addMaterialsToPersistentCaching = (settings.cachingFlags & MaterialCachingHelper.Mode.CACHE_MATERIALS) != 0
            };

            tmpSettings.OnWebRequestStartEvent += ParseGLTFWebRequestedFile;

            gltfComponent.LoadAsset(url, GetId() as string, false, tmpSettings);

            this.OnSuccess = OnSuccess;
            this.OnFail = OnFail;

            gltfComponent.OnSuccess += this.OnSuccess;
            gltfComponent.OnFail += this.OnFail;

            asset.name = url;
        }

        void ParseGLTFWebRequestedFile(ref string requestedFileName) { provider.TryGetContentsUrl(assetDirectoryPath + requestedFileName, out requestedFileName); }

        protected override void OnReuse(System.Action OnSuccess)
        {
            //NOTE(Brian):  Show the asset using the simple gradient feedback.
            asset.Show(settings.visibleFlags == AssetPromiseSettings_Rendering.VisibleFlags.VISIBLE_WITH_TRANSITION, OnSuccess);
        }

        protected override bool AddToLibrary()
        {
            if (!library.Add(asset))
                return false;

            if (!asset.visible)
                return true;

            //NOTE(Brian): If the asset did load "in world" add it to library and then Get it immediately
            //             So it keeps being there. As master gltfs can't be in the world.
            //
            //             ApplySettings will reposition the newly Get asset to the proper coordinates.
            if (settings.forceNewInstance)
            {
                asset = (library as AssetLibrary_GLTF).GetCopyFromOriginal(asset.id);
            }
            else
            {
                asset = library.Get(asset.id);
            }

            //NOTE(Brian): Call again this method because we are replacing the asset.
            OnBeforeLoadOrReuse();
            return true;
        }

        protected override void OnCancelLoading()
        {
            if (asset != null)
            {
                asset.CancelShow();
            }
        }

        protected override Asset_GLTF GetAsset(object id)
        {
            if (settings.forceNewInstance)
            {
                return ((AssetLibrary_GLTF) library).GetCopyFromOriginal(id);
            }
            else
            {
                return base.GetAsset(id);
            }
        }

        internal override void Unload()
        {
            Debug.Log($"PATO: unload {id} waitingAssetLoad = {waitingAssetLoad} state = {state} {GetHashCode()}");

            if (waitingAssetLoad)
                return;

            settings.parent = null;

            // NOTE: if Unload is called before finish loading we wait until gltf is loaded or failed before unloading it
            if (state == AssetPromiseState.LOADING && gltfComponent != null)
            {
                waitingAssetLoad = true;

                gltfComponent.OnSuccess -= OnSuccess;
                gltfComponent.OnFail -= OnFail;

                gltfComponent.OnSuccess += DoUnload;
                gltfComponent.OnFail += DoUnload;

                gltfComponent.CancelIfQueued();

                return;
            }

            base.Unload();
        }

        internal override void OnForget()
        {
            if (state == AssetPromiseState.LOADING)
            {
                settings.parent = null;
                if (asset != null)
                {
                    asset.CancelShow();
                    asset.Hide();
                    Debug.Log($"PATO: unchild {id} {GetHashCode()}");
                }
            }
            base.OnForget();
        }

        // NOTE: master promise are silently forgotten. We should make sure that they are loaded anyway since
        // other promises are waiting for them
        internal override void OnSilentForget()
        {
            Debug.Log($"PATO: OnSilentForget {id} {GetHashCode()} gltfComponent = {gltfComponent != null}");
            asset.Hide();
            if (gltfComponent != null)
            {
                gltfComponent.SetIgnoreDistanceTest();
            }
        }

        void DoUnload()
        {
            Debug.Log($"PATO: DoUnload {id} {GetHashCode()}");
            base.Unload();
        }
    }
}