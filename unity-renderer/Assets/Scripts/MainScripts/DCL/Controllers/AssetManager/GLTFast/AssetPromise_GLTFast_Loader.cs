using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GLTFast;
using GLTFast.Materials;
using UnityEngine;

// Disable async call not being awaited warning
#pragma warning disable CS4014

namespace DCL
{
    /// <summary>
    /// Do not use this class directly to instantiate a new GLTF, use AssetPromise_GLTFast_Instance instead
    /// </summary>
    public class AssetPromise_GLTFast_Loader : AssetPromise_WithUrl<Asset_GLTFast_Loader>
    {
        public static int MAX_CONCURRENT_REQUESTS => CommonScriptableObjects.rendererState.Get() ? 30 : 256;

        public static int concurrentRequests = 0;
        
        bool requestRegistered = false;

        private readonly ContentProvider contentProvider;
        private readonly string fileName;
        private readonly string assetDirectoryPath;
        private readonly GLTFastDownloadProvider gltFastDownloadProvider;
        private readonly CancellationTokenSource cancellationSource;


        private static IDeferAgent deferAgent;
        private GLTFImportLogger gltfImportLogger;

        public AssetPromise_GLTFast_Loader(  string contentUrl, string hash, IWebRequestController requestController, ContentProvider contentProvider = null) 
            : base(contentUrl, hash)
        {
            this.contentProvider = contentProvider;
            fileName = contentUrl.Substring(contentUrl.LastIndexOf('/') + 1);
            assetDirectoryPath = URIHelper.GetDirectoryName(contentUrl);

            if (deferAgent == null)
            {
                var agentObject = new GameObject("GLTFastDeferAgent");
                deferAgent = agentObject.AddComponent<GLTFastDeferAgent>();
            }
            
            gltFastDownloadProvider = new GLTFastDownloadProvider(requestController, FileToUrl);
            cancellationSource = new CancellationTokenSource();
            gltfImportLogger = new GLTFImportLogger();
        }

        protected override void OnBeforeLoadOrReuse(){}
        protected override void OnAfterLoadOrReuse(){}
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
            cancellationSource.Cancel();
        }

        protected override void OnLoad(Action OnSuccess, Action<Exception> OnFail)
        {
            ImportGLTF(OnSuccess, OnFail, cancellationSource.Token);
        }

        internal override void Unload()
        {
            base.Unload();
            gltFastDownloadProvider.Dispose();
        }

        private async UniTaskVoid ImportGLTF( Action OnSuccess, Action<Exception> OnFail, CancellationToken cancellationSourceToken)
        {
            try
            {
                string url = contentProvider.baseUrl + hash;

                var gltfImport = new GltfImport(gltFastDownloadProvider, deferAgent, new GLTFastMaterialGenerator(), gltfImportLogger);

                var gltfastSettings = new ImportSettings
                {
                    generateMipMaps = false,
                    anisotropicFilterLevel = 3,
                    nodeNameMethod = ImportSettings.NameImportMethod.OriginalUnique
                };

                // TODO: Implement a cancellation token for the GLTFImport when its supported https://github.com/atteneder/glTFast/issues/177
                var success = await gltfImport.Load(url, gltfastSettings);

                if (cancellationSourceToken.IsCancellationRequested)
                {
                    gltfImport.Dispose();
                    cancellationSourceToken.ThrowIfCancellationRequested();
                }
                
                if (!success)
                {
                    OnFail?.Invoke(new Exception(gltfImportLogger.GetLastError()));
                }
                else
                {
                    asset.Setup(gltfImport);
                    OnSuccess.Invoke();
                }
            }
            catch (Exception e)
            {
                OnFail?.Invoke(e);
            }
        }
        
        bool FileToUrl(string fileName, out string hash) { return contentProvider.TryGetContentsUrl(assetDirectoryPath + fileName, out hash); }
    }

}