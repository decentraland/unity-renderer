using DCL.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL.Models
{
    [Serializable]
    public struct CreateUISceneMessage
    {
        public string id;
        public string baseUrl;
    }

    [Serializable]
    public class LoadParcelScenesMessage
    {
        public List<UnityParcelScene> parcelsToLoad = new List<UnityParcelScene>();

        /**
         * This class is received as an element of the list of parcels that need to be visible
         */
        [Serializable]
        public class UnityParcelScene
        {
            public static bool VERBOSE = false;
            public string id;
            public string baseUrl;

            public Vector2Int basePosition;
            public Vector2Int[] parcels;

            public List<ContentMapping> contents;
            public Dictionary<string, string> fileToHash;

            [Serializable]
            public class ContentMapping
            {
                public string file;
                public string hash;
            }

            public ContentMapping GetMappingForHash(string hash)
            {
                if (contents == null) return null;

                return contents.FirstOrDefault((x) => x.hash == hash);
            }

            public void BakeHashes()
            {
                if (contents == null) return;

                if (VERBOSE) Debug.Log("Baking hashes...");

                fileToHash = new Dictionary<string, string>(contents.Count);

                for (int i = 0; i < contents.Count; i++)
                {
                    ContentMapping m = contents[i];
                    fileToHash.Add(m.file, m.hash);

                    if (VERBOSE) Debug.Log(string.Format("found file = {0} ... hash = {1}", m.file, m.hash));
                }
            }

            public bool HasTestSchema(string url)
            {
                if (url.StartsWith("file://"))
                    return true;

                if (url.StartsWith(TestHelpers.GetTestsAssetsPath()))
                    return true;

                return false;
            }

            public bool HasContentsUrl(string url)
            {
                if (HasTestSchema(url))
                    return true;

                if (fileToHash == null)
                    return false;

                return fileToHash.ContainsKey(url);
            }

            public string GetContentsUrl(string url)
            {
                string result = "";

                if (TryGetContentsUrl(url, out result))
                    return result;

                return null;
            }

            public bool TryGetContentsUrl(string url, out string result)
            {
                url = url.ToLower();
                result = url;

                if (HasTestSchema(url)) return true;

                if (fileToHash != null)
                {
                    if (!fileToHash.ContainsKey(url))
                    {
                        Debug.LogError(string.Format("GetContentsUrl >>> File {0} not found!!!", url));
                        return false;
                    }

                    result = baseUrl + fileToHash[url];
                }
                else
                {
                    result = baseUrl + "/" + url;
                }

                if (VERBOSE)
                    Debug.Log($">>> GetContentsURL from ... {url} ... RESULTING URL... = {result}");

                return true;
            }
        }
    }
}
