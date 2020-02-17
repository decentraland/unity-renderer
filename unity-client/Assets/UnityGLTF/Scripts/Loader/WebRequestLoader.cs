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

        public WebRequestLoader(string rootURI)
        {
            _rootURI = rootURI;
            HasSyncLoadMethod = false;
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

            if (OnLoadStreamStart != null)
            {
                OnLoadStreamStart(ref filePath);
            }

            yield return CreateHTTPRequest(_rootURI, filePath);
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

            UnityWebRequest www = new UnityWebRequest(finalUrl, "GET", new DownloadHandlerBuffer(), null);

            www.timeout = 5000;
#if UNITY_2017_2_OR_NEWER
            yield return www.SendWebRequest();
#else
            yield return www.Send();
#endif
            if ((int)www.responseCode >= 400)
            {
                Debug.LogError($"{www.responseCode} - {www.url}");
                yield break;
            }

            if (www.downloadedBytes > int.MaxValue)
            {
                Debug.LogError("Stream is larger than can be copied into byte array");
                yield break;
            }

            //NOTE(Brian): Caution, www.downloadHandler.data returns a COPY of the data, if accessed twice,
            //             2 copies will be performed for the entire file (and then discarded by GC, introducing hiccups).
            //             The correct fix is by using DownloadHandler.ReceiveData. But this is in version > 2019.3.
            byte[] data = www.downloadHandler.data;
            LoadedStream = new MemoryStream(data, 0, data.Length, true, true);
        }
    }
}
