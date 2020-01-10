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
        static int concurrentRequests = 0;
        bool mustDecrementRequest = false;

        static float maxLoadBudgetTime = 0.032f;
        static float currentLoadBudgetTime = 0;
        public static bool limitTimeBudget = false;
        Coroutine loadCoroutine;
        static HashSet<string> failedRequestUrls = new HashSet<string>();
        UnityWebRequest assetBundleRequest;


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

            if (asset != null)
            {
                asset.CancelShow();
            }

            if (mustDecrementRequest)
                concurrentRequests--;
        }

        protected override void OnAfterLoadOrReuse()
        {
        }

        protected override void OnBeforeLoadOrReuse()
        {
        }

        protected IEnumerator LoadAssetBundleWithDeps(string baseUrl, string hash, Action OnSuccess, Action OnFail)
        {
            yield return DependencyMapLoadHelper.GetDepMap(baseUrl, hash);

            if (DependencyMapLoadHelper.dependenciesMap.ContainsKey(hash))
            {
                foreach (string dep in DependencyMapLoadHelper.dependenciesMap[hash])
                {
                    var promise = new AssetPromise_AB(baseUrl, dep);
                    AssetPromiseKeeper_AB.i.Keep(promise);
                    yield return promise;
                }
            }

            if (concurrentRequests >= 25)
                yield return new WaitUntil(() => concurrentRequests < 25);

            concurrentRequests++;
            mustDecrementRequest = true;
            yield return LoadAssetBundle(baseUrl + hash, OnSuccess, OnFail);
            concurrentRequests--;
            mustDecrementRequest = false;
        }

        IEnumerator LoadAssetBundle(string finalUrl, Action OnSuccess, Action OnFail)
        {
            if (failedRequestUrls.Contains(finalUrl))
            {
                OnFail?.Invoke();
                yield break;
            }

            using (UnityWebRequest assetBundleRequest = UnityWebRequestAssetBundle.GetAssetBundle(finalUrl))
            {
                yield return assetBundleRequest.SendWebRequest();

                if (!assetBundleRequest.WebRequestSucceded())
                {
                    Debug.Log($"request failed? {assetBundleRequest.error} ... {finalUrl}");
                    assetBundleRequest.Abort();
                    OnFail?.Invoke();

                    failedRequestUrls.Add(finalUrl);
                    yield break;
                }

                AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(assetBundleRequest);

                if (assetBundle == null || asset == null)
                {
                    assetBundleRequest.Abort();
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


                foreach (string assetName in assetsToLoad)
                {
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
            }

            OnSuccess?.Invoke();
        }

        protected override void OnLoad(Action OnSuccess, Action OnFail)
        {
            loadCoroutine = CoroutineStarter.Start(LoadAssetBundleWithDeps(contentUrl, hash, OnSuccess, OnFail));
        }
    }
}
