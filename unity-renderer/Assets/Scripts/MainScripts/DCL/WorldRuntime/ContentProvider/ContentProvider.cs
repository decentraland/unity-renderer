using System;
using DCL.Helpers;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MappingPair = DCL.ContentServerUtils.MappingPair;

namespace DCL
{
    public class ContentProvider
    {
        public static bool VERBOSE = false;

        public string baseUrl;
        public List<MappingPair> contents = new List<MappingPair>();
        public Dictionary<string, string> fileToHash = new Dictionary<string, string>();

        public override string ToString()
        {
            string result = $"baseUrl: {baseUrl}\n";

            foreach (var pair in contents)
            {
                result += $"file: {pair.file} ... hash: {pair.hash}\n";
            }

            return result;
        }

        public ContentProvider() { }

        public ContentProvider(ContentProvider toCopy)
        {
            baseUrl = toCopy.baseUrl;
            contents = new List<MappingPair>(toCopy.contents);
            fileToHash = new Dictionary<string, string>(toCopy.fileToHash);
        }

        public MappingPair GetMappingForHash(string hash)
        {
            if (contents == null)
            {
                return null;
            }

            return contents.FirstOrDefault((x) => x.hash == hash);
        }

        //todo: thread this
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

            if (fileToHash == null)
            {
                fileToHash = new Dictionary<string, string>(contents.Count);
            }

            for (int i = 0; i < contents.Count; i++)
            {
                MappingPair m = contents[i];
                var key = m.file.ToLower();
                if (fileToHash.ContainsKey(key))
                {
                    Debug.Log($"Hash key: {key} already exists in the map");
                    continue;
                }
                fileToHash.Add(key, m.hash);

                if (VERBOSE)
                {
                    Debug.Log($"found file = {m.file} ... hash = {m.hash}\nfull url = {baseUrl}\\{m.hash}");
                }
            }
        }

        public virtual bool HasContentsUrl(string url)
        {
            url = url.ToLower();

            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

#if UNITY_EDITOR
            if (HasTestSchema(url))
            {
                return true;
            }
#endif
            if (fileToHash == null)
            {
                return false;
            }

            return fileToHash.ContainsKey(url);
        }

        public virtual string GetContentsUrl(string url)
        {
            string result = "";
            url = url.ToLower();

            if (TryGetContentsUrl(url, out result))
            {
                return result;
            }

            return null;
        }

        public virtual bool TryGetContentsUrl_Raw(string url, out string result)
        {
            url = url.ToLower();
            result = url;

#if UNITY_EDITOR
            if (HasTestSchema(url))
            {
                return true;
            }
#endif
            if (fileToHash != null)
            {
                if (!fileToHash.ContainsKey(url))
                {
                    Debug.LogError($"GetContentsUrl_Raw >>> File {url} not found!!!");
                    return false;
                }

                result = fileToHash[url];
            }
            else
            {
                result = url;
            }

            return true;
        }

        public virtual bool TryGetContentsUrl(string url, out string result)
        {
            url = url.ToLower();
            result = url;

            if (HasTestSchema(url))
            {
                return true;
            }

            if (fileToHash != null)
            {
                if (!fileToHash.ContainsKey(url))
                {
                    Debug.LogError($"GetContentsUrl >>> File {url} not found!!!");
                    return false;
                }

                result = baseUrl + fileToHash[url];
            }
            else
            {
                result = baseUrl + url;
            }

            if (VERBOSE)
            {
                Debug.Log($"GetContentsURL >>> from ... {url} ... RESULTING URL... = {result}");
            }

            return true;
        }

        public bool HasTestSchema(string url)
        {
            url = url.ToLower();

#if UNITY_EDITOR
            if (url.StartsWith("file://"))
            {
                return true;
            }

            if (url.StartsWith(TestAssetsUtils.GetPath()))
            {
                return true;
            }
#endif
            return false;
        }

        public bool TryGetContentHash(string file, out string result)
        {
            file = file.ToLower();
            result = string.Empty;

            if (fileToHash == null)
            {
                result = file;
                return true;
            }

            if (fileToHash.TryGetValue(file.ToLower(), out result))
            {
                return true;
            }

            return false;
        }
    }
}