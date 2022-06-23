using System;
using System.Collections;
using UnityEngine;
using MainScripts.DCL.Analytics.PerformanceAnalytics;

namespace DCL
{
    public class AssetPromise_GLTFast_GameObject : AssetPromise_WithUrl<Asset_GLTFast_GameObject>
    {
        private readonly ContentProvider contentProvider;
        public AssetPromiseSettings_Rendering settings = new AssetPromiseSettings_Rendering();
        AssetPromise_GLTFast subPromise;
        Coroutine loadingCoroutine;

        public AssetPromise_GLTFast_GameObject( string contentUrl, string hash, ContentProvider contentProvider = null) : base(contentUrl, hash)
        {
            this.contentProvider = contentProvider ?? new ContentProvider_Dummy();
        }

        protected override void OnLoad(Action OnSuccess, Action<Exception> OnFail) { loadingCoroutine = CoroutineStarter.Start(LoadingCoroutine(OnSuccess, OnFail)); }

        protected override bool AddToLibrary()
        {
            if (!library.Add(asset))
            {
                return false;
            }

            if (settings.forceNewInstance)
            {
                asset = (library as AssetLibrary_GLTFast_GameObject).GetCopyFromOriginal(asset.id);
            }
            else
            {
                asset = library.Get(asset.id);
            }
            
            settings.ApplyBeforeLoad(asset.container.transform);

            return true;
        }

        protected override void OnReuse(Action OnSuccess)
        {
            asset.Show(OnSuccess);
        }

        protected override void OnAfterLoadOrReuse()
        {
            settings.ApplyAfterLoad(asset.container.transform);
        }

        protected override void OnBeforeLoadOrReuse()
        {
            settings.ApplyBeforeLoad(asset.container.transform);
        }

        protected override void OnCancelLoading()
        {
            if (loadingCoroutine != null)
            {
                PerformanceAnalytics.GLTFTracker.TrackCancelled();
                CoroutineStarter.Stop(loadingCoroutine);
                loadingCoroutine = null;
            }

            if (asset != null)
                UnityEngine.Object.Destroy(asset.container);

            AssetPromiseKeeper_GLTFast.i.Forget(subPromise);
        }

        public override string ToString()
        {
            if (subPromise != null)
                return $"{subPromise.ToString()} ... GLTFast_GameObject state = {state}";
            else
                return $"subPromise == null? state = {state}";
        }

        public IEnumerator LoadingCoroutine(Action OnSuccess, Action<Exception> OnFail)
        {
            PerformanceAnalytics.ABTracker.TrackLoading();
            subPromise = new AssetPromise_GLTFast(contentUrl, hash, asset.container.transform, contentProvider);
            bool success = false;
            Exception loadingException = null;
            subPromise.OnSuccessEvent += (x) => success = true;
            subPromise.OnFailEvent += ( ab,  exception) =>
            {
                loadingException = exception;
                success = false;
            };

            asset.ownerPromise = subPromise;
            AssetPromiseKeeper_GLTFast.i.Keep(subPromise);

            yield return subPromise;
            
            if (success)
            {
                subPromise.asset.Instantiate(asset.container.transform);
            }

            loadingCoroutine = null;

            if (success)
            {
                PerformanceAnalytics.GLTFTracker.TrackLoaded();
                OnSuccess?.Invoke();
            }
            else
            {
                PerformanceAnalytics.GLTFTracker.TrackFailed();
                loadingException ??= new Exception($"GLTFast sub-promise asset or container is null. Asset: {subPromise.asset}, container: {asset.container}");
                Debug.LogException(loadingException);
                OnFail?.Invoke(loadingException);
            }
        }

        protected override Asset_GLTFast_GameObject GetAsset(object id)
        {
            if (settings.forceNewInstance)
            {
                return ((AssetLibrary_GLTFast_GameObject) library).GetCopyFromOriginal(id);
            }
            else
            {
                return base.GetAsset(id);
            }
        }
    }

}