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
        private readonly IWebRequestController webRequestController;
        private readonly AssetIdConverter fileToUrl;
        private readonly AssetPromiseKeeper_Texture texturePromiseKeeper;

        private List<IDisposable> disposables = new ();
        private string baseUrl;

        public GltFastDownloadProvider(string baseUrl, IWebRequestController webRequestController, AssetIdConverter fileToUrl, AssetPromiseKeeper_Texture texturePromiseKeeper)
        {
            this.baseUrl = baseUrl;
            this.webRequestController = webRequestController;
            this.fileToUrl = fileToUrl;
            this.texturePromiseKeeper = texturePromiseKeeper;
        }

        public async Task<IDownload> Request(Uri uri)
        {
            string finalUrl = GetFinalUrl(uri);

            WebRequestAsyncOperation asyncOp = (WebRequestAsyncOperation)webRequestController.Get(
                url: finalUrl,
                downloadHandler: new DownloadHandlerBuffer(),
                timeout: 30,
                disposeOnCompleted: false,
                requestAttemps: 3);

            GltfDownloaderWrapper wrapper = new GltfDownloaderWrapper(asyncOp);
            disposables.Add(wrapper);

            while (wrapper.MoveNext()) { await Task.Yield(); }

            if (!wrapper.Success) { Debug.LogError($"<color=Red>[GLTFast WebRequest Failed]</color> {asyncOp.asyncOp.webRequest.url} {asyncOp.asyncOp.webRequest.error}"); }

            return wrapper;
        }

        private string GetFinalUrl(Uri uri)
        {
            var finalUrl = uri.OriginalString;

            finalUrl = finalUrl.Replace(baseUrl, "");

            return fileToUrl(finalUrl, out string url) ? url : uri.OriginalString;
        }

        public async Task<ITextureDownload> RequestTexture(Uri uri, bool nonReadable, bool forceLinear)
        {
            string finalUrl = GetFinalUrl(uri);

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

            texturePromiseKeeper.Keep(promise);
            await promise;

            if (!wrapper.Success) Debug.LogError("[GLTFast Texture WebRequest Failed] " + finalUrl);

            return wrapper;
        }

        public void Dispose()
        {
            foreach (IDisposable disposable in disposables) { disposable.Dispose(); }

            disposables = null;
        }
    }
}
