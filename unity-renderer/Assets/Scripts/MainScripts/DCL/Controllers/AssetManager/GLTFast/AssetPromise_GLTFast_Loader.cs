using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL.GLTFast.Wrappers;
using GLTFast;
using GLTFast.Logging;
using GLTFast.Materials;
using System.IO;
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
        private const string SHADER_DCL_LIT = "DCL/Universal Render Pipeline/Lit";
        private const string GLTFAST_THROTTLER_NAME = "GLTFastThrottler";
        private readonly ContentProvider contentProvider;
        private readonly string assetDirectoryPath;
        private readonly GltFastDownloadProvider gltFastDownloadProvider;
        private readonly CancellationTokenSource cancellationSource;

        private readonly IMaterialGenerator gltFastMaterialGenerator;
        private readonly ConsoleLogger consoleLogger;

        private static IDeferAgent staticDeferAgent;
        private bool isLoading = false;

        public AssetPromise_GLTFast_Loader(string contentUrl, string hash, IWebRequestController requestController, ContentProvider contentProvider = null)
            : base(contentUrl, hash)
        {
            this.contentProvider = contentProvider;
            assetDirectoryPath = GetDirectoryName(contentUrl);

            if (staticDeferAgent == null)
            {
                var agentObject = new GameObject(GLTFAST_THROTTLER_NAME);
                staticDeferAgent = agentObject.AddComponent<GltFastDeferAgent>();
            }

            string baseUrl = contentProvider is null ? string.Empty : contentProvider.baseUrl;
            gltFastDownloadProvider = new GltFastDownloadProvider(baseUrl, requestController, FileToUrl, AssetPromiseKeeper_Texture.i);
            cancellationSource = new CancellationTokenSource();
            gltFastMaterialGenerator = new DecentralandMaterialGenerator(SHADER_DCL_LIT);
            consoleLogger = new ConsoleLogger();
        }

        private static string GetDirectoryName(string fullPath)
        {
            var fileName = Path.GetFileName(fullPath);
            return fullPath.Substring(0, fullPath.Length - fileName.Length);
        }

        protected override void OnBeforeLoadOrReuse() { }

        protected override void OnAfterLoadOrReuse() { }

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

        protected override void OnLoad(Action onSuccess, Action<Exception> onFail)
        {
            isLoading = true;
            ImportGltfAsync(onSuccess, onFail, cancellationSource.Token);
        }

        internal override void Unload()
        {
            gltFastDownloadProvider.Dispose();
            base.Unload();
        }

        public override bool keepWaiting => isLoading;

        private async UniTaskVoid ImportGltfAsync(Action onSuccess, Action<Exception> onFail, CancellationToken cancellationSourceToken)
        {
            try
            {
                string url = contentProvider.baseUrl + hash;

                var gltfImport = new GltfImport(gltFastDownloadProvider, staticDeferAgent, gltFastMaterialGenerator, consoleLogger);

                var gltFastSettings = new ImportSettings
                {
                    GenerateMipMaps = false,
                    AnisotropicFilterLevel = 3,
                    NodeNameMethod = NameImportMethod.OriginalUnique
                };

                bool success = await gltfImport.Load(url, gltFastSettings, cancellationSourceToken);

                if (cancellationSourceToken.IsCancellationRequested)
                {
                    gltfImport.Dispose();
                    cancellationSourceToken.ThrowIfCancellationRequested();
                }

                if (!success)
                    onFail?.Invoke(new GltFastLoadException($"[GLTFast] Load failed: {consoleLogger.LastErrorCode}"));
                else
                {
                    asset.Setup(gltfImport);
                    onSuccess.Invoke();
                }
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                    return;

                Debug.LogException(e);
                onFail?.Invoke(e);
            }
            finally { isLoading = false; }
        }

        private bool FileToUrl(string fileName, out string fileHash) =>
            contentProvider.TryGetContentsUrl(assetDirectoryPath + fileName, out fileHash);
    }

    public class GltFastLoadException : Exception
    {
        public GltFastLoadException(string message) : base(message) { }
    }
}
