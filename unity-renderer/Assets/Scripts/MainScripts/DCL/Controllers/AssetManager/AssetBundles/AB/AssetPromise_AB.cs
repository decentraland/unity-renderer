using DCL.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL
{
    public class AssetPromise_AB : AssetPromise_WithUrl<Asset_AB>
    {
        public static bool VERBOSE = false;
        public static int MAX_CONCURRENT_REQUESTS => CommonScriptableObjects.rendererState.Get() ? 30 : 256;

        public static int concurrentRequests = 0;
        public static event Action OnDownloadingProgressUpdate;

        bool requestRegistered = false;

        public static int downloadingCount => concurrentRequests;
        public static int queueCount => AssetPromiseKeeper_AB.i.waitingPromisesCount;

        Coroutine loadCoroutine;
        static HashSet<string> failedRequestUrls = new HashSet<string>();

        List<AssetPromise_AB> dependencyPromises = new List<AssetPromise_AB>();
        UnityWebRequest assetBundleRequest = null;

        public static AssetBundlesLoader assetBundlesLoader = new AssetBundlesLoader();
        private Transform containerTransform;

        public AssetPromise_AB(string contentUrl, string hash, Transform containerTransform = null) : base(contentUrl, hash)
        {
            this.containerTransform = containerTransform;
            assetBundlesLoader.Start();
        }

        protected override bool AddToLibrary()
        {
            if (!library.Add(asset))
            {
                Debug.Log("add to library fail?");
                return false;
            }

            if (asset == null)
            {
                Debug.LogWarning($"Asset is null when trying to add it to the library: hash == {this.GetId()}");
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

        private UnityWebRequestAsyncOperation asyncOp;

        protected IEnumerator LoadAssetBundleWithDeps(string baseUrl, string hash, Action OnSuccess, Action OnFail)
        {
            string finalUrl = baseUrl + hash;

            if (failedRequestUrls.Contains(finalUrl))
            {
                OnFail?.Invoke();
                yield break;
            }

            yield return WaitForConcurrentRequestsSlot();

            RegisterConcurrentRequest();
#if UNITY_EDITOR
            assetBundleRequest = UnityWebRequestAssetBundle.GetAssetBundle(finalUrl, Hash128.Compute(hash));
#else
            //NOTE(Brian): Disable in build because using the asset bundle caching uses IDB.
            assetBundleRequest = UnityWebRequestAssetBundle.GetAssetBundle(finalUrl);
#endif
            asyncOp = assetBundleRequest.SendWebRequest();

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
                        var promise = new AssetPromise_AB(baseUrl, dep, containerTransform);
                        AssetPromiseKeeper_AB.i.Keep(promise);
                        dependencyPromises.Add(promise);
                    }
                }
            }

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
                if (VERBOSE)
                    Debug.Log($"Request failed? {assetBundleRequest.error} ... {finalUrl}");
                failedRequestUrls.Add(finalUrl);
                assetBundleRequest.Abort();
                assetBundleRequest = null;
                OnFail?.Invoke();
                yield break;
            }

            UnregisterConcurrentRequest();

            foreach (var promise in dependencyPromises)
            {
                yield return promise;
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

            assetBundlesLoader.MarkAssetBundleForLoad(asset, assetBundle, containerTransform, OnSuccess, OnFail);
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