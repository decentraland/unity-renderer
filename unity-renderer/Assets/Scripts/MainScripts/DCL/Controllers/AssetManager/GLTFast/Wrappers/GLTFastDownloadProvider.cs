using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GLTFast.Loading;
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
        private List<IDisposable> disposables = new ();
        private string baseUrl;

        public GltFastDownloadProvider(string baseUrl, IWebRequestController webRequestController, AssetIdConverter fileToUrl)
        {
            this.baseUrl = baseUrl;
            this.webRequestController = webRequestController;
            this.fileToUrl = fileToUrl;
        }

        public async Task<IDownload> Request(Uri uri)
        {
            string finalUrl = GetFinalUrl(uri);

            UnityWebRequest uwr = await webRequestController.Get(
                url: finalUrl,
                downloadHandler: new DownloadHandlerBuffer(),
                timeout: 30,
                requestAttemps: 3);

            GltfDownloaderWrapper wrapper = new GltfDownloaderWrapper(uwr);
            disposables.Add(wrapper);

            if (!wrapper.Success)
                Debug.LogError($"<color=Red>[GLTFast WebRequest Failed]</color> {uwr.url} {uwr.error}");

            return wrapper;
        }

        private string GetFinalUrl(Uri uri)
        {
            var finalUrl = uri.OriginalString;

            finalUrl = finalUrl.Replace(baseUrl, "");

            return fileToUrl(finalUrl, out string url) ? url : uri.OriginalString;
        }

        public async Task<ITextureDownload> RequestTexture(Uri uri, bool nonReadable)
        {
            string finalUrl = GetFinalUrl(uri);

            UnityWebRequest uwr = await webRequestController.GetTexture(
                url: finalUrl,
                timeout: 30,
                requestAttemps: 3);

            GltfTextureDownloaderWrapper wrapper = new GltfTextureDownloaderWrapper(uwr);
            disposables.Add(wrapper);

            if (!wrapper.Success)
                Debug.LogError("[GLTFast Texture WebRequest Failed] " + uwr.url);

            return wrapper;
        }

        public void Dispose()
        {
            foreach (IDisposable disposable in disposables) { disposable.Dispose(); }

            disposables = null;
        }
    }
}
