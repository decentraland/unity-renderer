using DCL.Helpers;
using UnityEngine;
using UnityGLTF;

namespace DCL
{
    public class AssetPromise_GLTF : AssetPromise<Asset_GLTF>
    {
        public enum VisibleFlags
        {
            INVISIBLE,
            VISIBLE_WITHOUT_TRANSITION,
            VISIBLE_WITH_TRANSITION
        }

        public class Settings
        {
            public VisibleFlags visibleFlags = VisibleFlags.VISIBLE_WITH_TRANSITION;
            public Shader shaderOverride;

            public Transform parent;
            public Vector3? initialLocalPosition;
            public Quaternion? initialLocalRotation;
            public Vector3? initialLocalScale;
        }

        public Settings settings = new Settings();
        string assetDirectoryPath;
        public string url { get; private set; }

        public bool useIdForMockedMappings => SceneController.i == null || SceneController.i.isDebugMode || SceneController.i.isWssDebugMode;

        ContentProvider provider = null;
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

        protected override void ApplySettings_LoadStart()
        {
            Transform assetTransform = asset.container.transform;

            asset.container.name = "GLTF: " + url;

            if (settings.parent != null)
            {
                assetTransform.SetParent(settings.parent, false);
                assetTransform.ResetLocalTRS();
            }

            if (settings.initialLocalPosition.HasValue)
            {
                assetTransform.localPosition = settings.initialLocalPosition.Value;
            }

            if (settings.initialLocalRotation.HasValue)
            {
                assetTransform.localRotation = settings.initialLocalRotation.Value;
            }

            if (settings.initialLocalScale.HasValue)
            {
                assetTransform.localScale = settings.initialLocalScale.Value;
            }
        }

        protected override void ApplySettings_LoadFinished()
        {
            if (settings.visibleFlags == VisibleFlags.INVISIBLE)
            {
                Renderer[] renderers = asset.container.GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < renderers.Length; i++)
                {
                    Renderer renderer = renderers[i];
                    renderer.enabled = false;
                }
            }
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
                useVisualFeedback = settings.visibleFlags == VisibleFlags.VISIBLE_WITH_TRANSITION,
                initialVisibility = settings.visibleFlags != VisibleFlags.INVISIBLE,
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
            asset.Show(settings.visibleFlags == VisibleFlags.VISIBLE_WITH_TRANSITION, OnSuccess);
        }

        protected override void AddToLibrary()
        {
            library.Add(asset);

            if (asset.visible)
            {
                //NOTE(Brian): If the asset did load "in world" add it to library and then Get it immediately
                //             So it keeps being there. As master gltfs can't be in the world.
                //
                //             ApplySettings will reposition the newly Get asset to the proper coordinates.
                asset = library.Get(asset.id);
                ApplySettings_LoadStart();
            }
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
    }
}
