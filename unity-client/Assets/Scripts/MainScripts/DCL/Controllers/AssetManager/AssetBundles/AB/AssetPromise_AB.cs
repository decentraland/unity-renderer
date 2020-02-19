using DCL.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL
{
    public class AssetPromise_AB : AssetPromise_WithUrl<Asset_AB>
    {
        public static bool VERBOSE = false;

        public static readonly int MAX_CONCURRENT_REQUESTS = 30;
        static int concurrentRequests = 0;
        bool requestRegistered = false;

        static readonly float maxLoadBudgetTime = 0.032f;
        static float currentLoadBudgetTime = 0;
        public static bool limitTimeBudget = false;

        Coroutine loadCoroutine;
        static HashSet<string> failedRequestUrls = new HashSet<string>();

        List<AssetPromise_AB> dependencyPromises = new List<AssetPromise_AB>();
        UnityWebRequest assetBundleRequest = null;

        static Dictionary<string, int> loadOrderByExtension = new Dictionary<string, int>()
        {
            { "png", 0 },
            { "jpg", 1 },
            { "peg", 2 },
            { "bmp", 3 },
            { "psd", 4 },
            { "iff", 5 },
            { "mat", 6 },
            { "nim", 7 },
            { "ltf", 8 },
            { "glb", 9 }
        };

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

        internal override object GetId()
        {
            return hash;
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
                        yield return promise;
                    }
                }
            }

            yield return WaitForConcurrentRequestsSlot();

            RegisterConcurrentRequest();
            yield return LoadAssetBundle(baseUrl + hash, OnSuccess, OnFail);
            UnregisterConcurrentRequest();
        }

        IEnumerator LoadAssetBundle(string finalUrl, Action OnSuccess, Action OnFail)
        {
            if (failedRequestUrls.Contains(finalUrl))
            {
                OnFail?.Invoke();
                yield break;
            }

            assetBundleRequest = UnityWebRequestAssetBundle.GetAssetBundle(finalUrl);

            yield return assetBundleRequest.SendWebRequest();

            //NOTE(Brian): For some reason, another coroutine iteration can be triggered after Cleanup().
            //             So assetBundleRequest can be null here.
            if (assetBundleRequest == null)
                yield break;

            if (!assetBundleRequest.WebRequestSucceded())
            {
                Debug.Log($"request failed? {assetBundleRequest.error} ... {finalUrl}");
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

            string[] assets = assetBundle.GetAllAssetNames();
            List<string> assetsToLoad = new List<string>();

            assetsToLoad = assets.OrderBy(
                (x) =>
                {
                    string ext = x.Substring(x.Length - 3);

                    if (loadOrderByExtension.ContainsKey(ext))
                        return loadOrderByExtension[ext];
                    else
                        return 99;
                }).ToList();


            for (int i = 0; i < assetsToLoad.Count; i++)
            {
                string assetName = assetsToLoad[i];
                //NOTE(Brian): For some reason, another coroutine iteration can be triggered after Cleanup().
                //             To handle this case we exit using this.
                if (loadCoroutine == null)
                    yield break;

                if (asset == null)
                    break;

                float time = Time.realtimeSinceStartup;

#if UNITY_EDITOR
                if (VERBOSE)
                    Debug.Log("loading asset = " + assetName);
#endif
                string ext = assetName.Substring(assetName.Length - 3);

                UnityEngine.Object loadedAsset = assetBundle.LoadAsset(assetName);

                if (!asset.assetsByName.ContainsKey(assetName))
                    asset.assetsByName.Add(assetName, loadedAsset);

                if (!asset.assetsByExtension.ContainsKey(ext))
                    asset.assetsByExtension.Add(ext, new List<UnityEngine.Object>());

                asset.assetsByExtension[ext].Add(loadedAsset);

                if (limitTimeBudget)
                {
                    currentLoadBudgetTime += Time.realtimeSinceStartup - time;

                    if (currentLoadBudgetTime > maxLoadBudgetTime)
                    {
                        currentLoadBudgetTime = 0;
                        yield return null;
                    }
                }
            }

            OnSuccess?.Invoke();
        }

        protected override void OnLoad(Action OnSuccess, Action OnFail)
        {
            loadCoroutine = CoroutineStarter.Start(LoadAssetBundleWithDeps(contentUrl, hash, OnSuccess, OnFail));
        }

        IEnumerator WaitForConcurrentRequestsSlot()
        {
            if (concurrentRequests >= MAX_CONCURRENT_REQUESTS)
            {
                yield return new WaitUntil(() => concurrentRequests < MAX_CONCURRENT_REQUESTS);
            }
        }

        void RegisterConcurrentRequest()
        {
            if (requestRegistered)
                return;

            concurrentRequests++;
            requestRegistered = true;
        }

        void UnregisterConcurrentRequest()
        {
            if (!requestRegistered)
                return;

            concurrentRequests--;
            requestRegistered = false;
        }
    }
}
