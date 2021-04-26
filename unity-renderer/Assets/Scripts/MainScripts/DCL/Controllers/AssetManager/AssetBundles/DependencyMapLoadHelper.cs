using System;
using DCL.Helpers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using WaitUntil = DCL.WaitUntil;
using DCL;

public static class DependencyMapLoadHelper
{
    static bool VERBOSE = false;

    private const string PERSISTENT_CACHE_KEY = "DepMapCache_V2";
    private const float MIN_TIME_BETWEEN_SAVING_PERSISTENT_CACHE = 300.0f;

    private static bool persistentCacheLoaded = false;
    private static float lastTimeSavedPersistentCache = 0;

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
        yield return WebRequestController.i.Get(
            url: url,
            OnSuccess: (depmapRequest) =>
            {
                AssetDependencyMap map = JsonUtility.FromJson<AssetDependencyMap>(depmapRequest.downloadHandler.text);
                map.dependencies = map.dependencies.Where(x => !x.Contains("mainshader")).ToArray();

                dependenciesMap.Add(hash, new List<string>(map.dependencies));

                downloadingDepmap.Remove(hash);

                if (DCLTime.realtimeSinceStartup - lastTimeSavedPersistentCache >= MIN_TIME_BETWEEN_SAVING_PERSISTENT_CACHE)
                {
                    SavePersistentCache();
                }
            },
            OnFail: (depmapRequest) =>
            {
                failedRequests.Add(hash);
                downloadingDepmap.Remove(hash);
            });
    }

    private static void SavePersistentCache()
    {
        lastTimeSavedPersistentCache = DCLTime.realtimeSinceStartup;

        //NOTE(Brian): Use JsonConvert because unity JsonUtility doesn't support dictionaries
        string cacheJson = JsonConvert.SerializeObject(dependenciesMap);
        PlayerPrefsUtils.SetString(PERSISTENT_CACHE_KEY, cacheJson);
    }

    private static void LoadPersistentCache()
    {
        if (persistentCacheLoaded)
            return;

        persistentCacheLoaded = true;
        CommonScriptableObjects.rendererState.OnChange += RendererState_OnChange;

        // Beware that if a wrongly-constructed depmap was created previously, this will always load that depmap
        // when testing a new dump process locally it's safer to first run Edit->ClearAllPlayerPrefs from UnityEditor
        string depMapCache = PlayerPrefs.GetString(PERSISTENT_CACHE_KEY, String.Empty);

        if (!string.IsNullOrEmpty(depMapCache))
        {
            dependenciesMap = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(depMapCache);
        }
    }

    private static void RendererState_OnChange(bool current, bool previous)
    {
        if (persistentCacheLoaded)
        {
            // Once the persistent cache has been loaded the first time, it will go being saved each time the RendererState be changed
            SavePersistentCache();
        }
    }
}