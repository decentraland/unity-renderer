using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GLTFast;
using UnityEngine;

namespace DCL
{
    public class AssetPromise_GLTFast : AssetPromise_WithUrl<Asset_GLTFast>
    {
        public static int MAX_CONCURRENT_REQUESTS => CommonScriptableObjects.rendererState.Get() ? 30 : 256;

        public static int concurrentRequests = 0;
        
        bool requestRegistered = false;
        private readonly ContentProvider contentProvider;
        private CancellationTokenSource cancellationSource;
        private readonly string fileName;
        private readonly string assetDirectoryPath;
        private IWebRequestController webRequestController => Environment.i.platform.webRequest;
        public AssetPromise_GLTFast(string contentUrl, string hash, ContentProvider contentProvider = null) 
            : base(contentUrl, hash)
        {
            this.contentProvider = contentProvider;
            fileName = contentUrl.Substring(contentUrl.LastIndexOf('/') + 1);
            assetDirectoryPath = URIHelper.GetDirectoryName(contentUrl);
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
            cancellationSource = new CancellationTokenSource();
            UniTask.Run(() => ImportGLTF(OnSuccess, OnFail, cancellationSource.Token), true, cancellationSource.Token);
        }

        bool FileToUrl(string fileName, out string hash) { return contentProvider.TryGetContentsUrl(assetDirectoryPath + fileName, out hash); }
        private async UniTaskVoid ImportGLTF( Action OnSuccess, Action<Exception> OnFail, CancellationToken cancellationSourceToken)
        {
            try
            {
                await UniTask.SwitchToMainThread();
                
                // TODO: wrap our providers here
                var gltfImport = new GltfImport(new GLTFastDownloadProvider(webRequestController, contentProvider, FileToUrl), null, null, new GLTFImportLogger());

                var gltfastSettings = new ImportSettings
                {
                    generateMipMaps = false,
                    anisotropicFilterLevel = 3,
                    nodeNameMethod = ImportSettings.NameImportMethod.OriginalUnique
                };

                // TODO: Implement a cancellation token for the GLTFImport when its supported https://github.com/atteneder/glTFast/issues/177
                var success = await gltfImport.Load(contentProvider.baseUrl + hash, gltfastSettings);

                if (cancellationSourceToken.IsCancellationRequested)
                {
                    gltfImport.Dispose();
                    cancellationSourceToken.ThrowIfCancellationRequested();
                }
                
                if (!success)
                {
                    OnFail?.Invoke(new Exception("GLTFast promise failure"));
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
        
    }

}