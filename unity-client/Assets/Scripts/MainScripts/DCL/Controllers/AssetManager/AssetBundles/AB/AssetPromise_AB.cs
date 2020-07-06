using DCL.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL
{
    public class AssetPromise_AB : AssetPromise_WithUrl<Asset_AB>
    {
        public static bool VERBOSE = false;

        public static int MAX_CONCURRENT_REQUESTS = 30;

        public static int concurrentRequests = 0;
        public static event Action OnDownloadingProgressUpdate;

        bool requestRegistered = false;

        public static int downloadingCount => concurrentRequests;
        public static int queueCount => AssetPromiseKeeper_AB.i.waitingPromisesCount;

        static readonly float maxLoadBudgetTime = 0.032f;
        static float currentLoadBudgetTime = 0;

        public static bool limitTimeBudget => CommonScriptableObjects.rendererState.Get();

        Coroutine loadCoroutine;
        static HashSet<string> failedRequestUrls = new HashSet<string>();

        List<AssetPromise_AB> dependencyPromises = new List<AssetPromise_AB>();
        UnityWebRequest assetBundleRequest = null;

        static Dictionary<string, int> loadOrderByExtension = new Dictionary<string, int>()
        {
            {"png", 0},
            {"jpg", 1},
            {"peg", 2},
            {"bmp", 3},
            {"psd", 4},
            {"iff", 5},
            {"mat", 6},
            {"nim", 7},
            {"ltf", 8},
            {"glb", 9}
        };

        private IOrderedEnumerable<string> assetsToLoad;

        public AssetPromise_AB(string contentUrl, string hash) : base(contentUrl, hash)
        {
        }

        protected override bool AddToLibrary()
        {
            if (!library.Add(asset))
            {
                Debug.Log("add to library fail?");
                return false;
            }

            asset = library.Get(asset.id);
            return true;
        }

        protected override void OnCancelLoading()
        {
            if (loadCoroutine != null)
            {
                CoroutineStarter.Stop(loadCoroutine);
                loadCoroutine = null;
            }

            if (assetBundleRequest != null && !assetBundleRequest.isDone)
            {
                assetBundleRequest.Abort();
                assetBundleRequest.Dispose();
                assetBundleRequest = null;
            }

            for (int i = 0; i < dependencyPromises.Count; i++)
            {
                dependencyPromises[i].Unload();
            }

            dependencyPromises.Clear();

            if (asset != null)
            {
                asset.CancelShow();
            }

            UnregisterConcurrentRequest();
        }

        protected override void OnAfterLoadOrReuse()
        {
        }

        protected override void OnBeforeLoadOrReuse()
        {
        }

        protected IEnumerator LoadAssetBundleWithDeps(string baseUrl, string hash, Action OnSuccess, Action OnFail)
        {
            if (!DependencyMapLoadHelper.dependenciesMap.ContainsKey(hash))
                CoroutineStarter.Start(DependencyMapLoadHelper.GetDepMap(baseUrl, hash));

            yield return DependencyMapLoadHelper.WaitUntilDepMapIsResolved(hash);

            if (DependencyMapLoadHelper.dependenciesMap.ContainsKey(hash))
            {
                using (var it = DependencyMapLoadHelper.dependenciesMap[hash].GetEnumerator())
                {
                    while (it.MoveNext())
                    {
                        var dep = it.Current;
                        var promise = new AssetPromise_AB(baseUrl, dep);
                        AssetPromiseKeeper_AB.i.Keep(promise);
                        dependencyPromises.Add(promise);
                    }
                }

                foreach (var promise in dependencyPromises)
                {
                    yield return promise;
                }
            }

            yield return WaitForConcurrentRequestsSlot();

            RegisterConcurrentRequest();
            yield return LoadAssetBundle(baseUrl + hash, OnSuccess, OnFail);
            UnregisterConcurrentRequest();
        }

        public override string ToString()
        {
            string result = $"AB request... loadCoroutine = {loadCoroutine} ... state = {state}\n";

            if (assetBundleRequest != null)
                result += $"url = {assetBundleRequest.url} ... code = {assetBundleRequest.responseCode} ... progress = {assetBundleRequest.downloadProgress}\n";
            else
                result += $"null request for url: {contentUrl + hash}\n";


            if (dependencyPromises != null && dependencyPromises.Count > 0)
            {
                result += "Dependencies:\n\n";
                foreach (var p in dependencyPromises)
                {
                    result += p.ToString() + "\n\n";
                }
            }

            result += "Concurrent requests = " + concurrentRequests;

            return result;
        }

        IEnumerator LoadAssetBundle(string finalUrl, Action OnSuccess, Action OnFail)
        {
            if (failedRequestUrls.Contains(finalUrl))
            {
                OnFail?.Invoke();
                yield break;
            }

            assetBundleRequest = UnityWebRequestAssetBundle.GetAssetBundle(finalUrl, Hash128.Compute(hash));

            var asyncOp = assetBundleRequest.SendWebRequest();

            while (!asyncOp.isDone)
            {
                yield return null;
            }

            //NOTE(Brian): For some reason, another coroutine iteration can be triggered after Cleanup().
            //             So assetBundleRequest can be null here.
            if (assetBundleRequest == null)
            {
                OnFail?.Invoke();
                yield break;
            }

            if (!assetBundleRequest.WebRequestSucceded())
            {
                Debug.Log($"Request failed? {assetBundleRequest.error} ... {finalUrl}");
                failedRequestUrls.Add(finalUrl);
                assetBundleRequest.Abort();
                assetBundleRequest = null;
                OnFail?.Invoke();
                yield break;
            }

            AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(assetBundleRequest);

            if (assetBundle == null || asset == null)
            {
                assetBundleRequest.Abort();
                assetBundleRequest = null;
                OnFail?.Invoke();

                failedRequestUrls.Add(finalUrl);
                yield break;
            }

            asset.ownerAssetBundle = assetBundle;
            asset.assetBundleAssetName = assetBundle.name;

            List<UnityEngine.Object> loadedAssetsList = new List<UnityEngine.Object>();

            yield return LoadAssetsInOrder(assetBundle, loadedAssetsList);

            if (loadCoroutine == null)
            {
                OnFail?.Invoke();
                yield break;
            }

            foreach (var loadedAsset in loadedAssetsList)
            {
                string ext = "any";

                if (loadedAsset is Texture)
                {
                    ext = "png";
                }
                else if (loadedAsset is Material)
                {
                    ext = "mat";
                }
                else if (loadedAsset is Animation || loadedAsset is AnimationClip)
                {
                    ext = "nim";
                }
                else if (loadedAsset is GameObject)
                {
                    ext = "glb";
                }

                if (!asset.assetsByExtension.ContainsKey(ext))
                    asset.assetsByExtension.Add(ext, new List<UnityEngine.Object>());

                asset.assetsByExtension[ext].Add(loadedAsset);
            }

            OnSuccess?.Invoke();
        }


        private IEnumerator LoadAssetsInOrder(AssetBundle assetBundle, List<UnityEngine.Object> loadedAssetByName)
        {
            string[] assets = assetBundle.GetAllAssetNames();

            assetsToLoad = assets.OrderBy(
                (x) =>
                {
                    string ext = x.Substring(x.Length - 3);

                    if (loadOrderByExtension.ContainsKey(ext))
                        return loadOrderByExtension[ext];
                    else
                        return 99;
                });

            foreach (string assetName in assetsToLoad)
            {
                //NOTE(Brian): For some reason, another coroutine iteration can be triggered after Cleanup().
                //             To handle this case we exit using this.
                if (loadCoroutine == null)
                {
                    yield break;
                }

                if (asset == null)
                    break;

                float time = 0;

                if (limitTimeBudget)
                    time = Time.realtimeSinceStartup;

#if UNITY_EDITOR
                if (VERBOSE)
                    Debug.Log("loading asset = " + assetName);
#endif
                UnityEngine.Object loadedAsset = assetBundle.LoadAsset(assetName);

                if (loadedAsset is Material loadedMaterial)
                    loadedMaterial.shader = null;

                loadedAssetByName.Add(loadedAsset);

                if (!limitTimeBudget)
                    continue;

                currentLoadBudgetTime += Time.realtimeSinceStartup - time;

                if (currentLoadBudgetTime > maxLoadBudgetTime)
                {
                    currentLoadBudgetTime = 0;
                    yield return null;
                }
            }
        }

        protected override void OnLoad(Action OnSuccess, Action OnFail)
        {
            loadCoroutine = CoroutineStarter.Start(LoadAssetBundleWithDeps(contentUrl, hash, OnSuccess, OnFail));
        }

        IEnumerator WaitForConcurrentRequestsSlot()
        {
            while (concurrentRequests >= MAX_CONCURRENT_REQUESTS)
            {
                yield return null;
            }
        }

        void RegisterConcurrentRequest()
        {
            if (requestRegistered)
                return;

            concurrentRequests++;
            OnDownloadingProgressUpdate?.Invoke();
            requestRegistered = true;
        }

        void UnregisterConcurrentRequest()
        {
            if (!requestRegistered)
                return;

            concurrentRequests--;
            OnDownloadingProgressUpdate?.Invoke();
            requestRegistered = false;
        }
    }
}
