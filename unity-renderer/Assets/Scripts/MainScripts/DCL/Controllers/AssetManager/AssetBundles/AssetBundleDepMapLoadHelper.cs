using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class AssetBundleDepMapLoadHelper
{
    static bool VERBOSE = false;
    private const string MAIN_SHADER_FILENAME = "mainshader";

    public static Dictionary<string, List<string>> dependenciesMap = new Dictionary<string, List<string>>();

    static HashSet<string> failedRequests = new HashSet<string>();
    static HashSet<string> downloadingDepmap = new HashSet<string>();

    public static IEnumerator WaitUntilExternalDepMapIsResolved(string hash)
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

    public static IEnumerator LoadExternalDepMap(string baseUrl, string hash)
    {
        if (dependenciesMap.ContainsKey(hash))
            yield break;

        if (failedRequests.Contains(hash))
            yield break;

        if (downloadingDepmap.Contains(hash))
        {
            yield return WaitUntilExternalDepMapIsResolved(hash);
            yield break;
        }
        
        string url = baseUrl + hash + ".depmap";

        downloadingDepmap.Add(hash);
        yield return DCL.Environment.i.platform.webRequest.Get(
            url: url,
            OnSuccess: (depmapRequest) =>
            {
                LoadDepMapFromJSON(depmapRequest.webRequest.downloadHandler.text, hash);

                downloadingDepmap.Remove(hash);
            },
            OnFail: (depmapRequest) =>
            {
                failedRequests.Add(hash);
                downloadingDepmap.Remove(hash);
            });
    }

    public static void LoadDepMapFromJSON(string metadataJSON, string hash)
    {
        AssetBundleMetadata metadata = JsonUtility.FromJson<AssetBundleMetadata>(metadataJSON);

        if (VERBOSE)
        {
            Debug.Log($"DependencyMapLoadHelper: {hash} asset bundle version: " + metadata.version);
            Debug.Log($"DependencyMapLoadHelper: {hash} asset bundle timestamp: " + (metadata.timestamp > 0 ? new DateTime(metadata.timestamp).ToString() : metadata.timestamp.ToString()));
        }
        
        metadata.dependencies = metadata.dependencies.Where(x => !x.Contains(MAIN_SHADER_FILENAME)).ToArray();

        dependenciesMap[hash] = new List<string>(metadata.dependencies);
    }
}