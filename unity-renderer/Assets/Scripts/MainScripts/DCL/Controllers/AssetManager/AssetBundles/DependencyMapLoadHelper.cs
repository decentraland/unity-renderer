using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using WaitUntil = DCL.WaitUntil;



public static class DependencyMapLoadHelper
{
    static bool VERBOSE = false;

    public static Dictionary<string, List<string>> dependenciesMap = new Dictionary<string, List<string>>();
    static HashSet<string> failedRequests = new HashSet<string>();
    static List<string> downloadingDepmap = new List<string>();

    [System.Serializable]
    public class AssetDependencyMap
    {
        public string[] dependencies;
    }

    public static IEnumerator GetDepMap(string baseUrl, string hash)
    {
        string url = baseUrl + hash + ".depmap";

        if (failedRequests.Contains(url))
            yield break;

        if (dependenciesMap.ContainsKey(hash))
            yield break;

        if (downloadingDepmap.Contains(url))
        {
            yield return new WaitUntil(() => !downloadingDepmap.Contains(url), 20);
            Debug.Log($"Waiting too long for {url}?");
            yield return new WaitUntil(() => !downloadingDepmap.Contains(url));
            yield break;
        }

        using (UnityWebRequest depmapRequest = UnityWebRequest.Get(url))
        {
            downloadingDepmap.Add(url);
            yield return depmapRequest.SendWebRequest();

            if (depmapRequest.isHttpError || depmapRequest.isNetworkError)
            {
                failedRequests.Add(url);
                downloadingDepmap.Remove(url);
                yield break;
            }

            AssetDependencyMap map = JsonUtility.FromJson<AssetDependencyMap>(depmapRequest.downloadHandler.text);

            dependenciesMap.Add(hash, new List<string>(map.dependencies));
            downloadingDepmap.Remove(url);
        }
    }
}
