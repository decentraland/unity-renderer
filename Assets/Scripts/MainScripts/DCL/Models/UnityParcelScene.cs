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
            public static bool VERBOSE = true;
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
                if (contents == null)
                {
                    return null;
                }

                return contents.FirstOrDefault((x) => x.hash == hash);
            }

            public void BakeHashes()
            {
                if (contents == null)
                {
                    return;
                }

                if (VERBOSE)
                {
                    Debug.Log("Baking hashes...");
                }

                fileToHash = new Dictionary<string, string>(contents.Count);

                for (int i = 0; i < contents.Count; i++)
                {
                    ContentMapping m = contents[i];
                    fileToHash.Add(m.file.ToLower(), m.hash);

                    if (VERBOSE)
                    {
                        Debug.Log($"found file = {m.file} ... hash = {m.hash}\nfull url = {baseUrl}\\{m.hash}");
                    }
                }
            }
        }
    }
}
