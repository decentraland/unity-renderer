using System;
using DCL.Helpers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public static class DependencyMapLoadHelper
{
    static bool VERBOSE = false;
    
    private const string PERSISTENT_CACHE_KEY_FISRT_PART = "DepMapCache_V";

    private static string persistentCacheKey = null;
    private static bool persistentCacheLoaded = false;
    private static int cacheVersion = 0;

    public static Dictionary<string, List<string>> dependenciesMap = new Dictionary<string, List<string>>();

    static HashSet<string> failedRequests = new HashSet<string>();
    static HashSet<string> downloadingDepmap = new HashSet<string>();

    [System.Serializable]
    public class AssetDependencyMap
    {
        public string[] dependencies;
    }

    public static IEnumerator WaitUntilDepMapIsResolved(string hash)
    {
        while (true)
        {
            bool depmapBeingDownloaded = downloadingDepmap.Contains(hash);
            bool depmapRequestIsDone = dependenciesMap.ContainsKey(hash) || failedRequests.Contains(hash);

            if (!depmapBeingDownloaded && depmapRequestIsDone)
                break;

            yield return null;
        }
    }

    public static IEnumerator GetDepMap(string baseUrl, string hash)
    {
        string url = baseUrl + hash + ".depmap";

        SetupCacheVersion(baseUrl);
        
        LoadPersistentCache();

        if (dependenciesMap.ContainsKey(hash))
            yield break;

        if (failedRequests.Contains(hash))
            yield break;

        if (downloadingDepmap.Contains(hash))
        {
            yield return WaitUntilDepMapIsResolved(hash);
            yield break;
        }

        downloadingDepmap.Add(hash);
        yield return DCL.Environment.i.platform.webRequest.Get(
            url: url,
            OnSuccess: (depmapRequest) =>
            {
                AssetDependencyMap map = JsonUtility.FromJson<AssetDependencyMap>(depmapRequest.downloadHandler.text);
                map.dependencies = map.dependencies.Where(x => !x.Contains("mainshader")).ToArray();

                dependenciesMap.Add(hash, new List<string>(map.dependencies));

                downloadingDepmap.Remove(hash);

                SavePersistentCache();
            },
            OnFail: (depmapRequest) =>
            {
                failedRequests.Add(hash);
                downloadingDepmap.Remove(hash);
            });
    }

    private static void SetupCacheVersion(string baseUrl)
    {
        // The asset bundles base url follows the format "https://url/v4324234/" indicating the cache version at the end.
        int versionIndex = baseUrl.LastIndexOf('v')+1;
        int versionLength = baseUrl.Length - versionIndex - 1;
        string versionString = baseUrl.Substring(versionIndex, versionLength);
        cacheVersion = Int32.Parse(versionString);
        if (cacheVersion < 0)
            cacheVersion = 0;
        
        persistentCacheKey = PERSISTENT_CACHE_KEY_FISRT_PART + cacheVersion;
    }

    private static void SavePersistentCache()
    {
        // As a new dependenciesMap will be stored in playerprefs, we delete the old ones that won't be used anymore.
        ClearOldCachedDevmaps();
        
        //NOTE(Brian): Use JsonConvert because unity JsonUtility doesn't support dictionaries
        string cacheJson = JsonConvert.SerializeObject(dependenciesMap);
        PlayerPrefsUtils.SetString(persistentCacheKey, cacheJson);
    }

    private static void ClearOldCachedDevmaps()
    {
        int previousCacheToCleanAmount = 2;
        for (int i = cacheVersion-1; i >= cacheVersion-previousCacheToCleanAmount; i--)
        {
            PlayerPrefs.DeleteKey(PERSISTENT_CACHE_KEY_FISRT_PART + i);   
        }
    }

    private static void LoadPersistentCache()
    {
        if (persistentCacheLoaded)
            return;
        persistentCacheLoaded = true;
        
        // Beware that if a wrongly-constructed depmap was created previously, this will always load that depmap
        // when testing a new dump process locally it's safer to first run Edit->ClearAllPlayerPrefs from UnityEditor
        string depMapCache = PlayerPrefs.GetString(persistentCacheKey, String.Empty);

        if (!string.IsNullOrEmpty(depMapCache))
        {
            dependenciesMap = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(depMapCache);
        }
    }
}