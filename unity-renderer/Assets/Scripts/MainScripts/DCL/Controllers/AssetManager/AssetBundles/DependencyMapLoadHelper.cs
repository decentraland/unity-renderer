using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DependencyMapLoadHelper
{
    static bool VERBOSE = false;
    private const string MAIN_SHADER_FILENAME = "mainshader";

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
        if (dependenciesMap.ContainsKey(hash))
            yield break;

        if (failedRequests.Contains(hash))
            yield break;

        if (downloadingDepmap.Contains(hash))
        {
            yield return WaitUntilDepMapIsResolved(hash);
            yield break;
        }
        
        string url = baseUrl + hash + ".depmap";

        downloadingDepmap.Add(hash);
        yield return DCL.Environment.i.platform.webRequest.Get(
            url: url,
            OnSuccess: (depmapRequest) =>
            {
                AssetDependencyMap map = JsonUtility.FromJson<AssetDependencyMap>(depmapRequest.webRequest.downloadHandler.text);
                map.dependencies = map.dependencies.Where(x => !x.Contains(MAIN_SHADER_FILENAME)).ToArray();

                dependenciesMap.Add(hash, new List<string>(map.dependencies));

                downloadingDepmap.Remove(hash);
            },
            OnFail: (depmapRequest) =>
            {
                failedRequests.Add(hash);
                downloadingDepmap.Remove(hash);
            });
    }
}