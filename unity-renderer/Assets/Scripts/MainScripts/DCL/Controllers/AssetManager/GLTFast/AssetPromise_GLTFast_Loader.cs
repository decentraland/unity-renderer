using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL.GLTFast.Wrappers;
using GLTFast;
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
        private readonly ContentProvider contentProvider;
        private readonly string assetDirectoryPath;
        private readonly GLTFastDownloadProvider gltFastDownloadProvider;
        private readonly CancellationTokenSource cancellationSource;

        private readonly IMaterialGenerator gltFastMaterialGenerator;
        private readonly ConsoleLogger consoleLogger;
        
        private static IDeferAgent deferAgent;

        public AssetPromise_GLTFast_Loader(string contentUrl, string hash, IWebRequestController requestController, ContentProvider contentProvider = null) 
            : base(contentUrl, hash)
        {
            this.contentProvider = contentProvider;
            assetDirectoryPath = URIHelper.GetDirectoryName(contentUrl);

            // TODO: Inject this, somehow
            if (deferAgent == null)
            {
                var agentObject = new GameObject("GLTFastDeferAgent");
                deferAgent = agentObject.AddComponent<GLTFastDeferAgent>();
            }
            
            gltFastDownloadProvider = new GLTFastDownloadProvider(requestController, FileToUrl);
            cancellationSource = new CancellationTokenSource();
            //gltFastMaterialGenerator = new GLTFastMaterialGenerator();
            gltFastMaterialGenerator = new GLTFastDCLMaterialGenerator("DCL/Universal Render Pipeline/Lit");
            consoleLogger = new ConsoleLogger();
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
                
                var gltfImport = new GltfImport(gltFastDownloadProvider, deferAgent, gltFastMaterialGenerator, consoleLogger);

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
                    OnFail?.Invoke(new Exception($"[GLTFast] Failed to load asset {url}"));
                }
                else
                {
                    asset.Setup(gltfImport);
                    OnSuccess.Invoke();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                OnFail?.Invoke(e);
            }
        }
        
        bool FileToUrl(string fileName, out string hash) { return contentProvider.TryGetContentsUrl(assetDirectoryPath + fileName, out hash); }
    }

}