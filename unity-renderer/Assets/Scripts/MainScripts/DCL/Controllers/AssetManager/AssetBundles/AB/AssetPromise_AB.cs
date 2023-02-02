using Cysharp.Threading.Tasks;
using DCL.Providers;
using MainScripts.DCL.Controllers.AssetManager;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCL
{
    public class AssetPromise_AB : AssetPromise_WithUrl<Asset_AB>
    {
        public static event Action OnDownloadingProgressUpdate;
        public static int queueCount => AssetPromiseKeeper_AB.i.waitingPromisesCount;

        static HashSet<string> failedRequestUrls = new ();

        List<AssetPromise_AB> dependencyPromises = new ();

        public static AssetBundlesLoader assetBundlesLoader = new ();

        private readonly Transform containerTransform;
        private readonly AssetSource permittedSources;

        private CancellationTokenSource cancellationTokenSource;

        private Service<IAssetBundleResolver> assetBundleResolver;

        public AssetPromise_AB(string contentUrl, string hash,
            Transform containerTransform = null, AssetSource permittedSources = AssetSource.ALL) : base(contentUrl,
            hash)
        {
            this.containerTransform = containerTransform;
            this.permittedSources = permittedSources;
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
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = null;

            foreach (var t in dependencyPromises)
                t.Unload();

            dependencyPromises.Clear();

            asset?.CancelShow();
        }

        protected override void OnBeforeLoadOrReuse() { }

        protected override void OnAfterLoadOrReuse() { }

        private async UniTask LoadAssetBundleWithDeps(string baseUrl, string hash, Action onSuccess, Action<Exception> onFail, CancellationToken cancellationToken)
        {
            var finalUrl = baseUrl + hash;

            if (failedRequestUrls.Contains(finalUrl))
            {
                onFail?.Invoke(new Exception($"The url {finalUrl} has failed"));
                return;
            }

            try
            {
                var bundle = await assetBundleResolver.Ref.GetAssetBundleAsync(permittedSources, baseUrl, hash, cancellationToken);
                SetAssetBundle(bundle);

                var dependencies = await bundle.GetDependenciesAsync(baseUrl, hash, cancellationToken);

                foreach (string dependency in dependencies)
                {
                    var promise = new AssetPromise_AB(baseUrl, dependency, containerTransform, permittedSources);
                    AssetPromiseKeeper_AB.i.Keep(promise);
                    dependencyPromises.Add(promise);
                }

                foreach (var dependencyPromise in dependencyPromises)
                    await dependencyPromise;

                assetBundlesLoader.MarkAssetBundleForLoad(asset, containerTransform, onSuccess, onFail);
            }
            catch (Exception e)
            {
                if (e is not OperationCanceledException)
                    failedRequestUrls.Add(finalUrl);

                onFail?.Invoke(e);
            }
        }

        private void SetAssetBundle(AssetBundle assetBundle)
        {
            asset.SetAssetBundle(assetBundle);
            asset.LoadMetrics();
        }

        protected override void OnLoad(Action OnSuccess, Action<Exception> OnFail)
        {
            if (cancellationTokenSource != null)
                return;

            cancellationTokenSource = new CancellationTokenSource();
            OnDownloadingProgressUpdate?.Invoke();

            LoadAssetBundleWithDeps(contentUrl, hash, OnSuccess, OnFail, cancellationTokenSource.Token).Forget();
        }
    }
}
