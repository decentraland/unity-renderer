using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using Environment = DCL.Environment;

namespace UnityGLTF.Loader
{
    public class WebRequestLoader : ILoader
    {
        private const int TIMEOUT_IN_SECONDS = 300;
        public Stream LoadedStream { get; private set; }
        public bool HasSyncLoadMethod { get; private set; }
        public AssetIdConverter assetIdConverter { get; private set; }

        string _rootURI;
        bool VERBOSE = false;
        IWebRequestController webRequestController;

        public WebRequestLoader(string rootURI, IWebRequestController webRequestController, AssetIdConverter fileToHashConverter = null)
        {
            Assert.IsNotNull(webRequestController, "webRequestController should never be null!");
            _rootURI = rootURI;
            HasSyncLoadMethod = false;
            this.webRequestController = webRequestController;
            assetIdConverter = fileToHashConverter;
        }

        public async UniTask LoadStream(string filePath, CancellationToken token)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException("gltfFilePath");
            }

            if (VERBOSE)
            {
                Debug.Log($"CreateHTTPRequest rootUri: {_rootURI}, httpRequestPath: {filePath}");
            }

            filePath = GetWrappedUri(filePath);

            await CreateHTTPRequest(_rootURI, filePath, token);
        }

        public string GetWrappedUri(string uri)
        {
            if (assetIdConverter != null)
            {
                if (assetIdConverter(uri, out string result))
                {
                    return result;
                }
            }

            return uri;
        }

        public void LoadStreamSync(string jsonFilePath) { throw new NotImplementedException(); }

        private static int awaitCount = 0;
        private async UniTask CreateHTTPRequest(string rootUri, string httpRequestPath, CancellationToken token)
        {
            string finalUrl = httpRequestPath;

            if (!string.IsNullOrEmpty(rootUri))
            {
                finalUrl = Path.Combine(rootUri, httpRequestPath);
            }

            token.ThrowIfCancellationRequested();

            WebRequestAsyncOperation asyncOp = (WebRequestAsyncOperation)webRequestController.Get(
                url: finalUrl,
                downloadHandler: new DownloadHandlerBuffer(),
                timeout: TIMEOUT_IN_SECONDS,
                disposeOnCompleted: false,
                requestAttemps: 3);

            Assert.IsNotNull(asyncOp, "asyncOp == null ... Maybe you are using a mocked WebRequestController?");

            token.ThrowIfCancellationRequested();

            await UniTask.WaitUntil( () => asyncOp.isDone || asyncOp.isDisposed || asyncOp.isSucceeded, cancellationToken: token);

#if UNITY_STANDALONE || UNITY_EDITOR
            if (DataStore.i.common.isApplicationQuitting.Get())
                return;
#endif

            token.ThrowIfCancellationRequested();

            bool error = false;
            string errorMessage = null;

            if (!asyncOp.isSucceeded)
            {
                errorMessage = $"{asyncOp.webRequest.error} {asyncOp.webRequest.downloadHandler.text} {finalUrl}";
                throw new Exception(errorMessage);
            }

            if (!error && asyncOp.webRequest.downloadedBytes > int.MaxValue)
            {
                Debug.Log("Stream is too big for a byte array");
                errorMessage = "Stream is too big for a byte array";
                throw new Exception(errorMessage);
            }

            if (!error)
            {
                //NOTE(Brian): Caution, webRequestResult.downloadHandler.data returns a COPY of the data, if accessed twice,
                //             2 copies will be performed for the entire file (and then discarded by GC, introducing hiccups).
                //             The correct fix is by using DownloadHandler.ReceiveData. But this is in version > 2019.3.
                byte[] data = asyncOp.webRequest.downloadHandler.data;

                if (data != null)
                {
                    LoadedStream = new MemoryStream(data, 0, data.Length, true, true);
                }
                else
                {
                    error = true;
                    errorMessage = "Downloaded data is null";
                }
            }

            if (error && Environment.i != null)
            {
                Environment.i.platform.serviceProviders.analytics.SendAnalytic("gltf_fail_download",
                    new Dictionary<string, string>()
                    {
                        { "url", finalUrl },
                        { "message", errorMessage }
                    });
            }

            asyncOp.Dispose();
        }
    }
}
