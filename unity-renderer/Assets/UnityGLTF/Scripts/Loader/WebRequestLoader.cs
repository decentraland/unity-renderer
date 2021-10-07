using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DCL;
using UnityEngine;
using UnityEngine.Networking;
using Environment = DCL.Environment;

#if WINDOWS_UWP
using System.Threading.Tasks;
#endif

namespace UnityGLTF.Loader
{
    public class WebRequestLoader : ILoader
    {
        public Stream LoadedStream { get; private set; }
        public bool HasSyncLoadMethod { get; private set; }
        public AssetIdConverter assetIdConverter { get; private set; }

        string _rootURI;
        bool VERBOSE = false;
        IWebRequestController webRequestController;

        public WebRequestLoader(string rootURI, IWebRequestController webRequestController, AssetIdConverter fileToHashConverter = null)
        {
            _rootURI = rootURI;
            HasSyncLoadMethod = false;
            this.webRequestController = webRequestController;
            assetIdConverter = fileToHashConverter;
        }

        public IEnumerator LoadStream(string filePath)
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

            yield return CreateHTTPRequest(_rootURI, filePath);
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

        public void LoadStreamSync(string jsonFilePath)
        {
            throw new NotImplementedException();
        }

        private IEnumerator CreateHTTPRequest(string rootUri, string httpRequestPath)
        {
            string finalUrl = httpRequestPath;

            if (!string.IsNullOrEmpty(rootUri))
            {
                finalUrl = Path.Combine(rootUri, httpRequestPath);
            }

            WebRequestAsyncOperation asyncOp = webRequestController.Get(
                url: finalUrl,
                downloadHandler: new DownloadHandlerBuffer(),
                timeout: 5000,
                disposeOnCompleted: false);

            yield return asyncOp;

            bool error = false;
            string errorMessage = null;

            if (!asyncOp.isSucceded)
            {
                Debug.Log($"{asyncOp.webRequest.error} - {finalUrl} - responseCode: {asyncOp.webRequest.responseCode}");
                errorMessage = $"{asyncOp.webRequest.error} {asyncOp.webRequest.downloadHandler.text}";
                error = true;
            }

            if (!error && asyncOp.webRequest.downloadedBytes > int.MaxValue)
            {
                Debug.Log("Stream is too big for a byte array");
                errorMessage = "Stream is too big for a byte array";
                error = true;
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