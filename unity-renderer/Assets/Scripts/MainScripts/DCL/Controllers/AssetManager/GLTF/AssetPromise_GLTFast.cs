using System;
using Cysharp.Threading.Tasks;
using DCL.Models;
using GLTFast;
using GLTFast.Logging;
using UnityEngine;
using UnityEngine.Assertions;

namespace DCL
{
    public class AssetPromise_GLTFast : AssetPromise<Asset_GLTFast>
    {
        public AssetPromiseSettings_Rendering settings = new AssetPromiseSettings_Rendering();
        protected string assetDirectoryPath;

        protected ContentProvider provider = null;
        public string fileName { get; private set; }

        IWebRequestController webRequestController = null;

        readonly object id = null;
        private GltfImport gltfImport;

        public AssetPromise_GLTFast(string url)
            : this(new ContentProvider_Dummy(), url, null, Environment.i.platform.webRequest) { }

        public AssetPromise_GLTFast(string url, IWebRequestController webRequestController)
            : this(new ContentProvider_Dummy(), url, null, webRequestController) { }

        public AssetPromise_GLTFast(ContentProvider provider, string url, string hash = null)
            : this(provider, url, hash, Environment.i.platform.webRequest) { }

        public AssetPromise_GLTFast(ContentProvider provider, string url, IWebRequestController webRequestController)
            : this(provider, url, null, webRequestController) { }

        public AssetPromise_GLTFast(ContentProvider provider, string url, string hash, IWebRequestController webRequestController)
        {
            this.provider = provider;
            this.fileName = url.Substring(url.LastIndexOf('/') + 1);
            this.id = hash ?? url;
            this.webRequestController = webRequestController;
            // We separate the directory path of the GLB and its file name, to be able to use the directory path when 
            // fetching relative assets like textures in the ParseGLTFWebRequestedFile() event call
            assetDirectoryPath = URIHelper.GetDirectoryName(url);
        }

        protected override void OnBeforeLoadOrReuse() { settings.ApplyBeforeLoad(asset.container.transform); }

        protected override void OnAfterLoadOrReuse()
        {
            /*if (asset?.container != null)
            {
                asset.renderers = MeshesInfoUtils.ExtractUniqueRenderers(asset.container);
                settings.ApplyAfterLoad(asset.renderers.ToList());
            }*/
        }

        public override object GetId() { return id; }

        protected override void OnLoad(Action OnSuccess, Action<Exception> OnFail)
        {
            UniTask.Run(() => ImportGLTF(OnSuccess, OnFail));

            return;

            try
            {
                /*gltfComponent = asset.container.AddComponent<GLTFComponent>();

                gltfComponent.Initialize(webRequestController, AssetPromiseKeeper_GLTF.i.throttlingCounter);
                gltfComponent.RegisterCallbacks(MeshCreated, RendererCreated);

                GLTFComponent.Settings tmpSettings = new GLTFComponent.Settings
                {
                    useVisualFeedback = settings.visibleFlags ==
                                        AssetPromiseSettings_Rendering.VisibleFlags.VISIBLE_WITH_TRANSITION,
                    initialVisibility = settings.visibleFlags != AssetPromiseSettings_Rendering.VisibleFlags.INVISIBLE,
                    shaderOverride = settings.shaderOverride,
                    addMaterialsToPersistentCaching =
                        (settings.cachingFlags & MaterialCachingHelper.Mode.CACHE_MATERIALS) != 0,
                    forceGPUOnlyMesh = settings.forceGPUOnlyMesh
                };

                gltfComponent.OnSuccess += () =>
                {
#if UNITY_STANDALONE || UNITY_EDITOR
                    if (DataStore.i.common.isApplicationQuitting.Get())
                        return;
#endif
                    
                    if (asset != null)
                    {
                        asset.totalTriangleCount =
                            MeshesInfoUtils.ComputeTotalTriangles(asset.renderers, asset.meshToTriangleCount);
                        asset.materials = MeshesInfoUtils.ExtractUniqueMaterials(asset.renderers);
                        asset.textures = MeshesInfoUtils.ExtractUniqueTextures(asset.materials);
                        asset.animationClipSize = gltfComponent.GetAnimationClipMemorySize();
                        asset.meshDataSize = gltfComponent.GetMeshesMemorySize();
                        var animations = MeshesInfoUtils.ExtractUniqueAnimations(asset.container);
                        asset.animationClips = MeshesInfoUtils.ExtractUniqueAnimationClips(animations);
                    }

                    OnSuccess.Invoke();
                };

                gltfComponent.OnFail += OnFail;
                
                gltfComponent.LoadAsset(provider.baseUrl ?? assetDirectoryPath, fileName, GetId() as string,
                    false, tmpSettings, FileToHash);

                asset.name = fileName;*/

            }
            catch (Exception error)
            {
                OnFail?.Invoke(error);
            }
        }

        private async UniTaskVoid ImportGLTF(Action OnSuccess, Action<Exception> OnFail)
        {
            try
            {
                await UniTask.SwitchToMainThread();
                gltfImport = new GltfImport(null, null, null, new GLTFImportLogger());

                var gltfastSettings = new ImportSettings
                {
                    generateMipMaps = false,
                    anisotropicFilterLevel = 3,
                    nodeNameMethod = ImportSettings.NameImportMethod.OriginalUnique
                };

                string providerBaseUrl = provider.baseUrl ?? assetDirectoryPath + fileName;

                Debug.Log($"[GLTFast] Loading {providerBaseUrl}");
                var success = await gltfImport.Load(providerBaseUrl, gltfastSettings);

                if (!success)
                {
                    Debug.LogError($"[GLTFast] Failed {providerBaseUrl}");
                    OnFail?.Invoke(new Exception("GLTFast promise failure"));
                }
                else
                {
                    Debug.Log($"[GLTFast] Finished loading, Instantiating");
                    asset.Setup(gltfImport);
                    gltfImport.InstantiateMainScene(asset.container.transform);
                    OnSuccess.Invoke();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                OnFail?.Invoke(e);
            }
        }

        /*private void RendererCreated(Renderer r)
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

        bool FileToHash(string fileName, out string hash) { return provider.TryGetContentHash(assetDirectoryPath + fileName, out hash); }
*/
        protected override void OnReuse(Action OnSuccess)
        {
            // Materials and textures are reused, so they are not extracted again
            //asset.renderers = MeshesInfoUtils.ExtractUniqueRenderers(asset.container);

            //NOTE(Brian):  Show the asset using the simple gradient feedback.
            asset.Show(settings.visibleFlags == AssetPromiseSettings_Rendering.VisibleFlags.VISIBLE_WITH_TRANSITION, OnSuccess);
        }

        protected override bool AddToLibrary()
        {
            if (!library.Add(asset))
            {
                Debug.Log("add to library fail?");
                return false;
            }

            if (asset == null)
            {
                Debug.LogWarning($"Asset is null when trying to add it to the library: hash == {this.GetId()}");
                return false;
            }

            asset = library.Get(asset.id);
            return true;
        }

        protected override void OnCancelLoading()
        {
            /*if (asset != null)
            {
                asset.CancelShow();
            }*/
            Debug.Log("Cancel!");
        }
       

        // NOTE: master promise are silently forgotten. We should make sure that they are loaded anyway since
        // other promises are waiting for them
        internal void OnSilentForget()
        {
            Debug.Log("Im being hidden!", asset.container);
            asset.Hide();
            settings.parent = null;

            /*if (gltfComponent != null)
            {
                gltfComponent.SetPrioritized();
            }*/
        }

        public override void Cleanup()
        {
            base.Cleanup();
            gltfImport.Dispose();
        }
    }

    internal class GLTFImportLogger : ICodeLogger
    {
        public void Error(LogCode code, params string[] messages) { Debug.LogError(string.Join("\n", messages)); }
        public void Warning(LogCode code, params string[] messages) {  Debug.LogWarning(string.Join("\n", messages)); }
        public void Info(LogCode code, params string[] messages) { Debug.Log(string.Join("\n", messages)); }
        public void Error(string message) { Debug.LogError(message); }
        public void Warning(string message) { Debug.LogWarning(message); }
        public void Info(string message) { Debug.Log(message); }
    }
}