using System;
using DCL.Helpers;
using UnityGLTF;

namespace DCL
{
    public class AssetPromise_GLTF : AssetPromise<Asset_GLTF>
    {
        public AssetPromiseSettings_Rendering settings = new AssetPromiseSettings_Rendering();
        protected string assetDirectoryPath;

        protected ContentProvider provider = null;
        public string fileName { get; private set; }

        GLTFComponent gltfComponent = null;
        IWebRequestController webRequestController = null;

        object id = null;

        private Action OnSuccess;
        private Action OnFail;
        private bool waitingAssetLoad = false;

        public AssetPromise_GLTF(string url, IWebRequestController webRequestController)
            : this(new ContentProvider_Dummy(), url, null, webRequestController) { }

        public AssetPromise_GLTF(ContentProvider provider, string url, string hash = null)
            : this(provider, url, hash, Environment.i.platform.webRequest) { }

        public AssetPromise_GLTF(ContentProvider provider, string url, IWebRequestController webRequestController)
            : this(provider, url, null, webRequestController) { }

        public AssetPromise_GLTF(ContentProvider provider, string url, string hash, IWebRequestController webRequestController)
        {
            this.provider = provider;
            this.fileName = url.Substring(url.LastIndexOf('/') + 1);
            this.id = hash ?? url;
            this.webRequestController = webRequestController;
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
            if (asset?.container != null)
            {
                settings.ApplyAfterLoad(asset.container.transform);
            }
        }

        public override object GetId() { return id; }

        internal override void Load()
        {
            if (waitingAssetLoad)
                return;

            base.Load();
        }

        protected override void OnLoad(Action OnSuccess, Action OnFail)
        {
            gltfComponent = asset.container.AddComponent<GLTFComponent>();
            gltfComponent.Initialize(webRequestController);

            GLTFComponent.Settings tmpSettings = new GLTFComponent.Settings()
            {
                useVisualFeedback = settings.visibleFlags == AssetPromiseSettings_Rendering.VisibleFlags.VISIBLE_WITH_TRANSITION,
                initialVisibility = settings.visibleFlags != AssetPromiseSettings_Rendering.VisibleFlags.INVISIBLE,
                shaderOverride = settings.shaderOverride,
                addMaterialsToPersistentCaching = (settings.cachingFlags & MaterialCachingHelper.Mode.CACHE_MATERIALS) != 0,
            };

            gltfComponent.LoadAsset(provider.baseUrl ?? assetDirectoryPath, fileName, GetId() as string,
                false, tmpSettings, FileToHash);

            this.OnSuccess = OnSuccess;
            this.OnFail = OnFail;

            gltfComponent.OnSuccess += this.OnSuccess;
            gltfComponent.OnFail += this.OnFail;

            asset.name = fileName;
        }

        bool FileToHash(string fileName, out string hash)
        {
            return provider.TryGetContentHash(assetDirectoryPath + fileName, out hash);
        }

        protected override void OnReuse(Action OnSuccess)
        {
            //NOTE(Brian):  Show the asset using the simple gradient feedback.
            asset.Show(settings.visibleFlags == AssetPromiseSettings_Rendering.VisibleFlags.VISIBLE_WITH_TRANSITION, OnSuccess);
        }

        protected override bool AddToLibrary()
        {
            if (!library.Add(asset))
                return false;

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
                return ((AssetLibrary_GLTF)library).GetCopyFromOriginal(id);
            }
            else
            {
                return base.GetAsset(id);
            }
        }

        internal override void Unload()
        {
            if (waitingAssetLoad)
                return;

            settings.parent = null;

            // NOTE: if Unload is called before finish loading we wait until gltf is loaded or failed before unloading it
            if (state == AssetPromiseState.LOADING && gltfComponent != null)
            {
                waitingAssetLoad = true;

                state = AssetPromiseState.IDLE_AND_EMPTY;

                var pendingAsset = asset;
                asset.Hide();
                asset = null;

                gltfComponent.OnSuccess -= OnSuccess;
                gltfComponent.OnFail -= OnFail;

                gltfComponent.OnSuccess += () => OnLoadedAfterForget(true, pendingAsset);
                gltfComponent.OnFail += () => OnLoadedAfterForget(false, pendingAsset);

                gltfComponent.CancelIfQueued();

                return;
            }

            base.Unload();
        }

        // NOTE: master promise are silently forgotten. We should make sure that they are loaded anyway since
        // other promises are waiting for them
        internal void OnSilentForget()
        {
            asset.Hide();

            if (gltfComponent != null)
            {
                gltfComponent.SetPrioritized();
            }
        }

        private void OnLoadedAfterForget(bool success, Asset_GLTF loadedAsset)
        {
            asset = loadedAsset;
            waitingAssetLoad = false;

            if (success && asset != null)
            {
                bool alreadyInLibrary = library.Contains(asset);
                if (alreadyInLibrary)
                {
                    asset.Cleanup();
                    asset = null;
                }
                else
                {
                    AddToLibrary();
                }
            }

            ClearEvents();
            CallAndClearEvents(success);
            Cleanup();
        }
    }
}