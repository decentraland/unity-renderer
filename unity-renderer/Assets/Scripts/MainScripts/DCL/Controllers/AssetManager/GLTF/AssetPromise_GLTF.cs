using System;
using System.Linq;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;
using UnityEngine.Assertions;
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

        public AssetPromise_GLTF(string url)
            : this(new ContentProvider_Dummy(), url, null, Environment.i.platform.webRequest)
        {
        }

        public AssetPromise_GLTF(string url, IWebRequestController webRequestController)
            : this(new ContentProvider_Dummy(), url, null, webRequestController)
        {
        }

        public AssetPromise_GLTF(ContentProvider provider, string url, string hash = null)
            : this(provider, url, hash, Environment.i.platform.webRequest)
        {
        }

        public AssetPromise_GLTF(ContentProvider provider, string url, IWebRequestController webRequestController)
            : this(provider, url, null, webRequestController)
        {
        }

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

        protected override void OnLoad(Action OnSuccess, Action OnFail)
        {
            gltfComponent = asset.container.AddComponent<GLTFComponent>();
            gltfComponent.throttlingCounter = AssetPromiseKeeper_GLTF.i.throttlingCounter;
            gltfComponent.Initialize(webRequestController);

            GLTFComponent.Settings tmpSettings = new GLTFComponent.Settings()
            {
                useVisualFeedback = settings.visibleFlags == AssetPromiseSettings_Rendering.VisibleFlags.VISIBLE_WITH_TRANSITION,
                initialVisibility = settings.visibleFlags != AssetPromiseSettings_Rendering.VisibleFlags.INVISIBLE,
                shaderOverride = settings.shaderOverride,
                addMaterialsToPersistentCaching = (settings.cachingFlags & MaterialCachingHelper.Mode.CACHE_MATERIALS) != 0,
                forceGPUOnlyMesh = settings.forceGPUOnlyMesh
            };

            gltfComponent.LoadAsset(provider.baseUrl ?? assetDirectoryPath, fileName, GetId() as string,
                false, tmpSettings, FileToHash);

            gltfComponent.sceneImporter.OnMeshCreated += MeshCreated;
            gltfComponent.sceneImporter.OnRendererCreated += RendererCreated;

            gltfComponent.OnSuccess += () =>
            {
                if ( asset != null )
                {
                    asset.totalTriangleCount = MeshesInfoUtils.ComputeTotalTriangles(asset.renderers, asset.meshToTriangleCount);
                }

                OnSuccess.Invoke();
            };

            gltfComponent.OnFail += OnFail;

            asset.name = fileName;
        }


        private void RendererCreated(Renderer r)
        {
            Assert.IsTrue(r != null, "Renderer is null?");

            // TODO(Brian): SilentForget nulls this. Remove this line after fixing the GLTF cancellation. 
            if ( asset == null )
                return;

            asset.renderers.Add(r);
        }

        private void MeshCreated(Mesh mesh)
        {
            Assert.IsTrue(mesh != null, "Mesh is null?");

            // TODO(Brian): SilentForget nulls this. Remove this line after fixing the GLTF cancellation. 
            if ( asset == null )
                return;

            asset.meshes.Add(mesh);
            asset.meshToTriangleCount[mesh] = mesh.triangles.Length;
        }

        bool FileToHash(string fileName, out string hash)
        {
            return provider.TryGetContentHash(assetDirectoryPath + fileName, out hash);
        }

        protected override void OnReuse(Action OnSuccess)
        {
            asset.renderers = asset.container.GetComponentsInChildren<Renderer>(true).ToList();
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

        // NOTE: master promise are silently forgotten. We should make sure that they are loaded anyway since
        // other promises are waiting for them
        internal void OnSilentForget()
        {
            asset.Hide();
            settings.parent = null;

            if (gltfComponent != null)
            {
                gltfComponent.SetPrioritized();
            }
        }
    }
}