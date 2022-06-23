using System;
using System.Threading.Tasks;
using GLTFast.Loading;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL
{
    internal class GLTFastDownloadProvider : IDownloadProvider
    {
        readonly IWebRequestController webRequestController;
        private readonly ContentProvider contentProvider;
        private readonly AssetIdConverter fileToUrl;
        public GLTFastDownloadProvider( IWebRequestController webRequestController, ContentProvider contentProvider, AssetIdConverter fileToUrl)
        {
            this.webRequestController = webRequestController;
            this.contentProvider = contentProvider;
            this.fileToUrl = fileToUrl;
        }

        public async Task<IDownload> Request(Uri url)
        {
            WebRequestAsyncOperation asyncOp = (WebRequestAsyncOperation)webRequestController.Get(
                url: url.AbsoluteUri,
                downloadHandler: new DownloadHandlerBuffer(),
                timeout: 999,
                disposeOnCompleted: false,
                requestAttemps: 3);

            GLTFDownloaderWrapper wrapper = new GLTFDownloaderWrapper(asyncOp);

            while (wrapper.MoveNext())
            {
                await Task.Yield();
            }

            return wrapper;
        }

        public async Task<ITextureDownload> RequestTexture(Uri uri, bool nonReadable)
        {
            string fileName = uri.AbsolutePath.Substring(uri.AbsolutePath.LastIndexOf('/') + 1);
            fileToUrl(fileName, out string url);
            WebRequestAsyncOperation asyncOp = webRequestController.GetTexture(
                url: url,
                timeout: 999,
                disposeOnCompleted: false,
                requestAttemps: 3);

            GLTFTextureDownloaderWrapper wrapper = new GLTFTextureDownloaderWrapper(asyncOp, nonReadable);

            while (wrapper.MoveNext())
            {
                await Task.Yield();
            }

            return wrapper;
        }
    }
}