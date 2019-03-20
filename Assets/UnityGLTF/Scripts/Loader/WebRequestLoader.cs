using System;
using System.Collections;
using System.IO;
using GLTF;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Net;
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

        private string _rootURI;

        public WebRequestLoader(string rootURI)
        {
            _rootURI = rootURI;
            HasSyncLoadMethod = false;
        }

        public IEnumerator LoadStream(string gltfFilePath)
        {
            if (gltfFilePath == null)
            {
                throw new ArgumentNullException("gltfFilePath");
            }

            yield return CreateHTTPRequest(_rootURI, gltfFilePath);
        }

        public void LoadStreamSync(string jsonFilePath)
        {
            throw new NotImplementedException();
        }

        private IEnumerator CreateHTTPRequest(string rootUri, string httpRequestPath)
        {
            UnityWebRequest www = new UnityWebRequest(Path.Combine(rootUri, httpRequestPath), "GET", new DownloadHandlerBuffer(), null);
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

            LoadedStream = new MemoryStream(www.downloadHandler.data, 0, www.downloadHandler.data.Length, true, true);
        }
    }
}
