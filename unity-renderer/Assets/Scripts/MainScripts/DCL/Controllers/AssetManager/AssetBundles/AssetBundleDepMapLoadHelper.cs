using Cysharp.Threading.Tasks;
using DCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public static class AssetBundleDepMapLoadHelper
{
    static bool VERBOSE = false;
    private const string MAIN_SHADER_FILENAME = "mainshader";

    public static Dictionary<string, List<string>> dependenciesMap = new ();

    static HashSet<string> failedRequests = new ();
    static HashSet<string> downloadingDepmap = new ();

    public static async UniTask<IReadOnlyList<string>> GetDependenciesAsync(this AssetBundle assetBundle, string baseUrl, string hash, CancellationToken cancellationToken)
    {
        // Check internal metadata file (dependencies, version, timestamp) and if it doesn't exist, fetch the external depmap file (old way of handling ABs dependencies)
        TextAsset metadata = Asset_AB.GetMetadata(assetBundle);

        if (metadata != null)
            LoadDepMapFromJSON(metadata.text, hash);
        else
        {
            if (!dependenciesMap.ContainsKey(hash))
                await LoadExternalDepMap(baseUrl, hash, cancellationToken);
        }

        if (!dependenciesMap.TryGetValue(hash, out List<string> dependencies))
            dependencies = new List<string>();

        return dependencies;
    }

    public static UniTask WaitUntilExternalDepMapIsResolved(string hash)
    {
        return UniTask.WaitUntil(() => !downloadingDepmap.Contains(hash) && (dependenciesMap.ContainsKey(hash) || failedRequests.Contains(hash)));
    }

    public static async UniTask LoadExternalDepMap(string baseUrl, string hash, CancellationToken cancellationToken)
    {
        if (dependenciesMap.ContainsKey(hash))
            return;

        if (failedRequests.Contains(hash))
            return;

        if (downloadingDepmap.Contains(hash))
        {
            await WaitUntilExternalDepMapIsResolved(hash);
            return;
        }

        string url = baseUrl + hash + ".depmap";

        downloadingDepmap.Add(hash);

        await DCL.Environment.i.platform.webRequest.Get(
            url: url,
            OnSuccess: (depmapRequest) =>
            {
                LoadDepMapFromJSON(depmapRequest.webRequest.downloadHandler.text, hash);

                downloadingDepmap.Remove(hash);
            },
            OnFail: _ =>
            {
                failedRequests.Add(hash);
                downloadingDepmap.Remove(hash);
            }).WithCancellation(cancellationToken);
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
