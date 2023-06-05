using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GLTFast.Loading;
using MainScripts.DCL.Controllers.AssetManager;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL.GLTFast.Wrappers
{
    /// <summary>
    /// With this class we replace all of GLTFast web requests with our own
    /// </summary>
    internal class GltFastDownloadProvider : IDownloadProvider, IDisposable
    {
        public delegate bool AssetIdConverter(string uri, out string id);

        private readonly IWebRequestController webRequestController;
        private readonly AssetIdConverter fileToUrl;
        private readonly AssetPromiseKeeper_Texture texturePromiseKeeper;

        private List<IDisposable> disposables = new ();
        private string baseUrl;
        private bool isDisposed;

        public GltFastDownloadProvider(string baseUrl, IWebRequestController webRequestController, AssetIdConverter fileToUrl, AssetPromiseKeeper_Texture texturePromiseKeeper)
        {
            this.baseUrl = baseUrl;
            this.webRequestController = webRequestController;
            this.fileToUrl = fileToUrl;
            this.texturePromiseKeeper = texturePromiseKeeper;
        }

        public async Task<IDownload> Request(Uri uri)
        {
            if (isDisposed)
                return null;

            string finalUrl = GetFinalUrl(uri, false);

            var asyncOp = await webRequestController.GetAsync(
                url: finalUrl,
                downloadHandler: new DownloadHandlerBuffer(),
                timeout: 30,
                requestAttemps: 3);

            GltfDownloaderWrapper wrapper = new GltfDownloaderWrapper(asyncOp);
            disposables.Add(wrapper);

            if (!wrapper.Success) { Debug.LogError($"<color=Red>[GLTFast WebRequest Failed]</color> {asyncOp.url} {asyncOp.error}"); }

            return wrapper;
        }

        private string GetFinalUrl(Uri uri, bool isTexture)
        {
            string fileName = uri.OriginalString;

            if (string.IsNullOrEmpty(baseUrl))
                return fileName;

            fileName = fileName.Replace(baseUrl, "");

            if (fileName.StartsWith("file"))
                return fileName;

            // this can return false and the url is valid, only happens with asset with hash as a name ( mostly gltf )
            if (fileToUrl(fileName, out string finalUrl))
                return finalUrl;

            if (isTexture)
                throw new Exception($"File not found in scene {finalUrl}");

            return uri.OriginalString;
        }

        public async Task<ITextureDownload> RequestTexture(Uri uri, bool nonReadable, bool forceLinear)
        {
            if (isDisposed)
                return null;

            string finalUrl = GetFinalUrl(uri, true);

            var promise = new AssetPromise_Texture(
                finalUrl,
                storeTexAsNonReadable: nonReadable,
                overrideMaxTextureSize: DataStore.i.textureConfig.gltfMaxSize.Get(),
                overrideCompression:
#if UNITY_WEBGL
                true
#else
                false
#endif
              , linear: forceLinear
            );

            var wrapper = new GLTFastTexturePromiseWrapper(texturePromiseKeeper, promise);
            disposables.Add(wrapper);

            Exception promiseException = null;
            promise.OnFailEvent += (_,e) => promiseException = e;

            texturePromiseKeeper.Keep(promise);
            await promise;

            if (!wrapper.Success)
            {
                string errorMessage = promiseException != null ? promiseException.Message : wrapper.Error;
                Debug.LogError($"[GLTFast Texture WebRequest Failed] Error: {errorMessage}" + finalUrl);
            }

            return wrapper;
        }

        public void Dispose()
        {
            foreach (IDisposable disposable in disposables) { disposable.Dispose(); }
            disposables.Clear();
            isDisposed = true;
        }
    }
}
