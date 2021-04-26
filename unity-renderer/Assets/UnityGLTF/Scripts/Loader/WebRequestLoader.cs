using DCL;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

#if WINDOWS_UWP
using System.Threading.Tasks;
#endif

namespace UnityGLTF.Loader
{
    public class WebRequestLoader : ILoader
    {
        public Stream LoadedStream { get; private set; }
        public bool HasSyncLoadMethod { get; private set; }

        public delegate void WebRequestLoaderEventAction(ref string requestFileName);
        public event WebRequestLoaderEventAction OnLoadStreamStart;

        string _rootURI;
        bool VERBOSE = false;
        IWebRequestController webRequestController;

        public WebRequestLoader(string rootURI, IWebRequestController webRequestController)
        {
            _rootURI = rootURI;
            HasSyncLoadMethod = false;
            this.webRequestController = webRequestController;
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
            OnLoadStreamStart?.Invoke(ref uri);
            return uri;
        }

        public void LoadStreamSync(string jsonFilePath) { throw new NotImplementedException(); }

        private IEnumerator CreateHTTPRequest(string rootUri, string httpRequestPath)
        {
            string finalUrl = httpRequestPath;

            if (!string.IsNullOrEmpty(rootUri))
            {
                finalUrl = Path.Combine(rootUri, httpRequestPath);
            }

            return webRequestController.Get(
                url: finalUrl,
                downloadHandler: new DownloadHandlerBuffer(),
                OnSuccess: (webRequestResult) =>
                {
                    if (webRequestResult.downloadedBytes > int.MaxValue)
                    {
                        Debug.LogError("Stream is too big for a byte array");
                    }
                    else
                    {
                        //NOTE(Brian): Caution, webRequestResult.downloadHandler.data returns a COPY of the data, if accessed twice,
                        //             2 copies will be performed for the entire file (and then discarded by GC, introducing hiccups).
                        //             The correct fix is by using DownloadHandler.ReceiveData. But this is in version > 2019.3.
                        byte[] data = webRequestResult.downloadHandler.data;

                        if (data != null)
                            LoadedStream = new MemoryStream(data, 0, data.Length, true, true);
                    }
                },
                OnFail: (webRequestResult) =>
                {
                    Debug.LogError($"{webRequestResult.error} - {finalUrl}");
                },
                timeout: 5000);

        }
    }
}