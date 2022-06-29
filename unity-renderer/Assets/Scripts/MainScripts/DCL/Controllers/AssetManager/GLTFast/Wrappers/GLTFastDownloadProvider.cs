using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GLTFast.Loading;
using UnityEngine.Networking;

namespace DCL
{
    internal class GLTFastDownloadProvider : IDownloadProvider, IDisposable
    {
        readonly IWebRequestController webRequestController;
        private readonly AssetIdConverter fileToUrl;
        private List<IDisposable> disposables = new List<IDisposable>();
        public GLTFastDownloadProvider( IWebRequestController webRequestController, AssetIdConverter fileToUrl)
        {
            this.webRequestController = webRequestController;
            this.fileToUrl = fileToUrl;
        }

        public async Task<IDownload> Request(Uri url)
        {
            WebRequestAsyncOperation asyncOp = (WebRequestAsyncOperation)webRequestController.Get(
                url: url.AbsoluteUri,
                downloadHandler: new DownloadHandlerBuffer(),
                timeout: 30,
                disposeOnCompleted: false,
                requestAttemps: 3);

            GLTFDownloaderWrapper wrapper = new GLTFDownloaderWrapper(asyncOp);
            disposables.Add(wrapper);

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
                timeout: 30,
                disposeOnCompleted: false,
                requestAttemps: 3);

            GLTFTextureDownloaderWrapper wrapper = new GLTFTextureDownloaderWrapper(asyncOp, nonReadable);
            disposables.Add(wrapper);
            
            while (wrapper.MoveNext())
            {
                await Task.Yield();
            }

            return wrapper;
        }
        public void Dispose()
        {
            foreach (IDisposable disposable in disposables)
            {
                disposable.Dispose();
            }
            disposables = null;
        }
    }
}