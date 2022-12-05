﻿using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL.GLTFast.Wrappers;
using GLTFast;
using GLTFast.Logging;
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
        private const string SHADER_DCL_LIT = "DCL/Universal Render Pipeline/Lit";
        private const string GLTFAST_THROTTLER_NAME = "GLTFastThrottler";
        private readonly ContentProvider contentProvider;
        private readonly string assetDirectoryPath;
        private readonly GltFastDownloadProvider gltFastDownloadProvider;
        private readonly CancellationTokenSource cancellationSource;

        private readonly IMaterialGenerator gltFastMaterialGenerator;
        private readonly ConsoleLogger consoleLogger;

        private static IDeferAgent staticDeferAgent;

        public AssetPromise_GLTFast_Loader(string contentUrl, string hash, IWebRequestController requestController, ContentProvider contentProvider = null)
            : base(contentUrl, hash)
        {
            this.contentProvider = contentProvider;
            assetDirectoryPath = URIHelper.GetDirectoryName(contentUrl);

            if (staticDeferAgent == null)
            {
                var agentObject = new GameObject(GLTFAST_THROTTLER_NAME);
                staticDeferAgent = agentObject.AddComponent<GltFastDeferAgent>();
            }

            gltFastDownloadProvider = new GltFastDownloadProvider(requestController, FileToUrl);
            cancellationSource = new CancellationTokenSource();
            gltFastMaterialGenerator = new DecentralandMaterialGenerator(SHADER_DCL_LIT);
            consoleLogger = new ConsoleLogger();
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
            ImportGltfAsync(onSuccess, onFail, cancellationSource.Token);
        }

        internal override void Unload()
        {
            base.Unload();
            gltFastDownloadProvider.Dispose();
        }

        private async UniTaskVoid ImportGltfAsync(Action onSuccess, Action<Exception> onFail, CancellationToken cancellationSourceToken)
        {
            try
            {
                string url = contentProvider.baseUrl + hash;

                var gltfImport = new GltfImport(gltFastDownloadProvider, staticDeferAgent, gltFastMaterialGenerator, consoleLogger);

                var gltFastSettings = new ImportSettings
                {
                    generateMipMaps = false,
                    anisotropicFilterLevel = 3,
                    nodeNameMethod = ImportSettings.NameImportMethod.OriginalUnique
                };

                bool success = await gltfImport.Load(url, gltFastSettings, cancellationSourceToken).AsUniTask().AttachExternalCancellation(cancellationSourceToken);

                if (cancellationSourceToken.IsCancellationRequested)
                {
                    gltfImport.Dispose();
                    cancellationSourceToken.ThrowIfCancellationRequested();
                }

                if (!success)
                    onFail?.Invoke(new Exception($"[GLTFast] Failed to load asset {url}"));
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
        }

        private bool FileToUrl(string fileName, out string fileHash) =>
            contentProvider.TryGetContentsUrl(assetDirectoryPath + fileName, out fileHash);
    }
}
