using UnityGLTF;

namespace DCL
{
    public class AssetPromise_GLTF : AssetPromise<Asset_GLTF>
    {
        public AssetPromiseSettings_Rendering settings = new AssetPromiseSettings_Rendering();
        protected string assetDirectoryPath;

        protected ContentProvider provider = null;
        public string url { get; private set; }

        public bool useIdForMockedMappings => SceneController.i == null || SceneController.i.isDebugMode || SceneController.i.isWssDebugMode;

        GLTFComponent gltfComponent = null;
        object id = null;

        public AssetPromise_GLTF(ContentProvider provider, string url)
        {
            this.provider = provider;
            this.url = url.Substring(url.LastIndexOf('/') + 1);
            // We separate the directory path of the GLB and its file name, to be able to use the directory path when 
            // fetching relative assets like textures in the ParseGLTFWebRequestedFile() event call
            assetDirectoryPath = URIHelper.GetDirectoryName(url);
        }

        protected override void OnBeforeLoadOrReuse()
        {
            asset.container.name = "GLTF: " + url;
            settings.ApplyBeforeLoad(asset.container.transform);
        }

        protected override void OnAfterLoadOrReuse()
        {
            settings.ApplyAfterLoad(asset.container.transform);
        }

        internal override object GetId()
        {
            if (id == null)
                id = ComputeId(provider, url);

            return id;
        }

        protected override void OnLoad(System.Action OnSuccess, System.Action OnFail)
        {
            gltfComponent = asset.container.AddComponent<GLTFComponent>();

            GLTFComponent.Settings tmpSettings = new GLTFComponent.Settings()
            {
                useVisualFeedback = settings.visibleFlags == AssetPromiseSettings_Rendering.VisibleFlags.VISIBLE_WITH_TRANSITION,
                initialVisibility = settings.visibleFlags != AssetPromiseSettings_Rendering.VisibleFlags.INVISIBLE,
                shaderOverride = settings.shaderOverride
            };

            tmpSettings.OnWebRequestStartEvent += ParseGLTFWebRequestedFile;

            gltfComponent.LoadAsset(url, false, tmpSettings);
            gltfComponent.OnSuccess += OnSuccess;
            gltfComponent.OnFail += OnFail;

            asset.name = url;
        }

        void ParseGLTFWebRequestedFile(ref string requestedFileName)
        {
            provider.TryGetContentsUrl(assetDirectoryPath + requestedFileName, out requestedFileName);
        }

        protected override void OnReuse(System.Action OnSuccess)
        {
            //NOTE(Brian):  Show the asset using the simple gradient feedback.
            asset.Show(settings.visibleFlags == AssetPromiseSettings_Rendering.VisibleFlags.VISIBLE_WITH_TRANSITION, OnSuccess);
        }

        protected override bool AddToLibrary()
        {
            if (!library.Add(asset))
                return false;

            if (asset.visible)
            {
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
            }

            return true;
        }

        private string ComputeId(ContentProvider provider, string url)
        {
            if (provider.contents != null && !useIdForMockedMappings)
            {
                if (provider.TryGetContentsUrl_Raw(url, out string finalUrl))
                {
                    return finalUrl;
                }
            }

            return url;
        }

        protected override void OnCancelLoading()
        {
            if (asset != null)
                asset.CancelShow();
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
    }
}
