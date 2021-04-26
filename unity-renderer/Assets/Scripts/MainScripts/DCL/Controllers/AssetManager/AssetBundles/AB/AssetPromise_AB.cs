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

        public static AssetBundlesLoader assetBundlesLoader = new AssetBundlesLoader();
        private Transform containerTransform;
        private WebRequestAsyncOperation asyncOp;

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

            if (asyncOp != null)
            {
                asyncOp.Dispose();
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

        protected override void OnAfterLoadOrReuse() { }

        protected override void OnBeforeLoadOrReuse() { }

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
            asyncOp = WebRequestController.i.GetAssetBundle(url: finalUrl, hash: Hash128.Compute(hash), disposeOnCompleted: false);
#else
            //NOTE(Brian): Disable in build because using the asset bundle caching uses IDB.
            asyncOp = WebRequestController.i.GetAssetBundle(url: finalUrl, disposeOnCompleted: false);
#endif
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

            yield return asyncOp;

            if (asyncOp.isDisposed)
            {
                OnFail?.Invoke();
                yield break;
            }

            if (!asyncOp.isSucceded)
            {
                if (VERBOSE)
                    Debug.Log($"Request failed? {asyncOp.webRequest.error} ... {finalUrl}");
                failedRequestUrls.Add(finalUrl);
                OnFail?.Invoke();
                asyncOp.Dispose();
                yield break;
            }

            UnregisterConcurrentRequest();

            foreach (var promise in dependencyPromises)
            {
                yield return promise;
            }

            AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(asyncOp.webRequest);
            asyncOp.Dispose();

            if (assetBundle == null || asset == null)
            {
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

            if (asyncOp.webRequest != null)
                result += $"url = {asyncOp.webRequest.url} ... code = {asyncOp.webRequest.responseCode} ... progress = {asyncOp.webRequest.downloadProgress}\n";
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

        protected override void OnLoad(Action OnSuccess, Action OnFail) { loadCoroutine = CoroutineStarter.Start(LoadAssetBundleWithDeps(contentUrl, hash, OnSuccess, OnFail)); }

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