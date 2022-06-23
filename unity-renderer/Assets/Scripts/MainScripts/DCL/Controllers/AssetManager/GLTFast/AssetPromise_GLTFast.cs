using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GLTFast;
using GLTFast.Logging;
using UnityEngine;

namespace DCL
{
    public class AssetPromise_GLTFast : AssetPromise_WithUrl<Asset_GLTFast>
    {
        public static int MAX_CONCURRENT_REQUESTS => CommonScriptableObjects.rendererState.Get() ? 30 : 256;

        public static int concurrentRequests = 0;
        
        bool requestRegistered = false;
        private Transform containerTransform;
        private CancellationTokenSource cancellationSource;
        public AssetPromise_GLTFast(string contentUrl, string hash,
            Transform containerTransform = null) : base(contentUrl,
            hash)
        {
            this.containerTransform = containerTransform;
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

        private async UniTaskVoid ImportGLTF( Action OnSuccess, Action<Exception> OnFail, CancellationToken cancellationSourceToken)
        {
            try
            {
                await UniTask.SwitchToMainThread();
                
                // TODO: wrap our providers here
                var gltfImport = new GltfImport(null, null, null, new GLTFImportLogger());

                var gltfastSettings = new ImportSettings
                {
                    generateMipMaps = false,
                    anisotropicFilterLevel = 3,
                    nodeNameMethod = ImportSettings.NameImportMethod.OriginalUnique
                };

                string providerBaseUrl = contentUrl;
                
                // TODO: Implement a cancellation token for the GLTFImport when its supported https://github.com/atteneder/glTFast/issues/177
                var success = await gltfImport.Load(providerBaseUrl, gltfastSettings);

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