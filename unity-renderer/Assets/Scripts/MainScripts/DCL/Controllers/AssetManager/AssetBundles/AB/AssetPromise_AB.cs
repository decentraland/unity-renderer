using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL
{
    public class AssetPromise_AB : AssetPromise_WithUrl<Asset_AB>
    {
        const string METADATA_FILENAME = "metadata.json";

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

        public AssetPromise_AB(string contentUrl, string hash,
            Transform containerTransform = null) : base(contentUrl,
            hash)
        {
            this.containerTransform = containerTransform;
            assetBundlesLoader.Start();
        }

        protected override bool AddToLibrary()
        {
            if(isDebug)
                Debug.Log($"AssetPromise_AB.AddToLibrary() - {GetId()} - 1");
            
            if (!library.Add(asset))
            {
                Debug.Log("add to library fail?");
                return false;
            }
            
            if(isDebug)
                Debug.Log($"AssetPromise_AB.AddToLibrary() - {GetId()} - 2");

            if (asset == null)
            {
                Debug.LogWarning($"Asset is null when trying to add it to the library: hash == {this.GetId()}");
                return false;
            }
            
            if(isDebug)
                Debug.Log($"AssetPromise_AB.AddToLibrary() - {GetId()} - 3");

            asset = library.Get(asset.id);
            return true;
        }

        protected override void OnCancelLoading()
        {
            if(isDebug)
                Debug.Log($"AssetPromise_AB.OnCancelLoading() - {GetId()}");
            
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

        protected override void OnAfterLoadOrReuse()
        {
        }

        protected override void OnBeforeLoadOrReuse()
        {
        }

        protected IEnumerator LoadAssetBundleWithDeps(string baseUrl, string hash, Action OnSuccess, Action<Exception> OnFail)
        {
            string finalUrl = baseUrl + hash;
            
            if(isDebug)
                Debug.Log($"AssetPromise_AB.LoadAssetBundleWithDeps() - {GetId()} - 1 - final URL: {finalUrl}");

            if (failedRequestUrls.Contains(finalUrl))
            {
                OnFail?.Invoke(new Exception($"The url {finalUrl} has failed"));
                yield break;
            }
            
            if(isDebug)
                Debug.Log($"AssetPromise_AB.LoadAssetBundleWithDeps() - {GetId()} - 2");

            yield return WaitForConcurrentRequestsSlot();
            
            if(isDebug)
                Debug.Log($"AssetPromise_AB.LoadAssetBundleWithDeps() - {GetId()} - 3");

            RegisterConcurrentRequest();
#if (UNITY_EDITOR || UNITY_STANDALONE)
            asyncOp = Environment.i.platform.webRequest.GetAssetBundle(url: finalUrl, hash: Hash128.Compute(hash),
                disposeOnCompleted: false, timeout:60);
#else
            //NOTE(Brian): Disable in build because using the asset bundle caching uses IDB.
            asyncOp = Environment.i.platform.webRequest.GetAssetBundle(url: finalUrl, disposeOnCompleted: false);
#endif

            if(isDebug)
                Debug.Log($"AssetPromise_AB.LoadAssetBundleWithDeps() - {GetId()} - 4 - Will download AB without loading deps yet...");
            
            // 1. Download asset bundle, but don't load its objects yet
            yield return asyncOp;
            
            if(isDebug)
                Debug.Log($"AssetPromise_AB.LoadAssetBundleWithDeps() - {GetId()} - 5 - Downloaded.");

            if (asyncOp.isDisposed)
            {
                OnFail?.Invoke(new Exception("Operation is disposed"));
                yield break;
            }
            
            if(isDebug)
                Debug.Log($"AssetPromise_AB.LoadAssetBundleWithDeps() - {GetId()} - 6");

            if (!asyncOp.isSucceded)
            {
                if(isDebug)
                    Debug.Log($"AssetPromise_AB.LoadAssetBundleWithDeps() - {GetId()} - 7 - Request failed? {asyncOp.webRequest.error}");
                
                if (VERBOSE)
                    Debug.Log($"Request failed? {asyncOp.webRequest.error} ... {finalUrl}");
                failedRequestUrls.Add(finalUrl);
                OnFail?.Invoke(new Exception($"Request failed? {asyncOp.webRequest.error} ... {finalUrl}"));
                asyncOp.Dispose();
                yield break;
            }
            
            if(isDebug)
                Debug.Log($"AssetPromise_AB.LoadAssetBundleWithDeps() - {GetId()} - 8");

            AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(asyncOp.webRequest);
            asyncOp.Dispose();

            if (assetBundle == null || asset == null)
            {
                if(isDebug)
                    Debug.Log($"AssetPromise_AB.LoadAssetBundleWithDeps() - {GetId()} - 9 - AB or Asset is null!");
                
                OnFail?.Invoke(new Exception("Asset bundle or asset is null"));
                failedRequestUrls.Add(finalUrl);
                yield break;
            }
            
            if(isDebug)
                Debug.Log($"AssetPromise_AB.LoadAssetBundleWithDeps() - {GetId()} - 10");

            asset.ownerAssetBundle = assetBundle;
            asset.assetBundleAssetName = assetBundle.name;

            // 2. Check internal metadata file (dependencies, version, timestamp) and if it doesn't exist, fetch the external depmap file (old way of handling ABs dependencies)
            TextAsset metadata = assetBundle.LoadAsset<TextAsset>(METADATA_FILENAME);

            if (metadata != null)
            {
                if(isDebug)
                    Debug.Log($"AssetPromise_AB.LoadAssetBundleWithDeps() - {GetId()} - 11A - will load depmap from JSON");
                
                AssetBundleDepMapLoadHelper.LoadDepMapFromJSON(metadata.text, hash);
            }
            else
            {
                if(isDebug)
                    Debug.Log($"AssetPromise_AB.LoadAssetBundleWithDeps() - {GetId()} - 11B - will load external depmap");
                
                if (!AssetBundleDepMapLoadHelper.dependenciesMap.ContainsKey(hash))
                    CoroutineStarter.Start(AssetBundleDepMapLoadHelper.LoadExternalDepMap(baseUrl, hash));

                yield return AssetBundleDepMapLoadHelper.WaitUntilExternalDepMapIsResolved(hash);
            }
            
            if(isDebug)
                Debug.Log($"AssetPromise_AB.LoadAssetBundleWithDeps() - {GetId()} - 12 - Will resolve dependencies...");

            // 3. Resolve dependencies
            if (AssetBundleDepMapLoadHelper.dependenciesMap.ContainsKey(hash))
            {
                using (var it = AssetBundleDepMapLoadHelper.dependenciesMap[hash].GetEnumerator())
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
            
            if(isDebug)
                Debug.Log($"AssetPromise_AB.LoadAssetBundleWithDeps() - {GetId()} - 13 - Will wait for dependency promises...");

            UnregisterConcurrentRequest();

            foreach (var promise in dependencyPromises)
            {
                yield return promise;
            }

            assetBundlesLoader.MarkAssetBundleForLoad(asset, assetBundle, containerTransform, OnSuccess, OnFail);
            
            if(isDebug)
                Debug.Log($"AssetPromise_AB.LoadAssetBundleWithDeps() - {GetId()} - 14 - Marked asset bundle for load.");
        }

        public override string ToString()
        {
            string result = $"AB request... loadCoroutine = {loadCoroutine} ... state = {state}\n";

            if (asyncOp.webRequest != null)
                result +=
                    $"url = {asyncOp.webRequest.url} ... code = {asyncOp.webRequest.responseCode} ... progress = {asyncOp.webRequest.downloadProgress}\n";
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

        protected override void OnLoad(Action OnSuccess, Action<Exception> OnFail)
        {
            if(isDebug)
                Debug.Log($"AssetPromise_AB.OnLoad() - {GetId()}");
            
            loadCoroutine = CoroutineStarter.Start(DCLCoroutineRunner.Run(
                LoadAssetBundleWithDeps(contentUrl, hash, OnSuccess, OnFail),
                exception => OnFail?.Invoke(exception)));
        }

        IEnumerator WaitForConcurrentRequestsSlot()
        {
            if(isDebug)
                Debug.Log($"AssetPromise_AB.WaitForConcurrentRequestsSlot() - {GetId()} - 1");
            
            while (concurrentRequests >= MAX_CONCURRENT_REQUESTS)
            {
                yield return null;
            }
            
            if(isDebug)
                Debug.Log($"AssetPromise_AB.WaitForConcurrentRequestsSlot() - {GetId()} - 2");
        }

        void RegisterConcurrentRequest()
        {
            if(isDebug)
                Debug.Log($"AssetPromise_AB.RegisterConcurrentRequest() - {GetId()} - already registered? {requestRegistered}");
            
            if (requestRegistered)
                return;

            concurrentRequests++;
            OnDownloadingProgressUpdate?.Invoke();
            requestRegistered = true;
        }

        void UnregisterConcurrentRequest()
        {
            if(isDebug)
                Debug.Log($"AssetPromise_AB.RegisterConcurrentRequest() - {GetId()} - already UNregistered? {!requestRegistered}");
            
            if (!requestRegistered)
                return;

            concurrentRequests--;
            OnDownloadingProgressUpdate?.Invoke();
            requestRegistered = false;
        }
    }
}