using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using DCL.Helpers;
using UnityEngine;
using MainScripts.DCL.Analytics.PerformanceAnalytics;
using Object = UnityEngine.Object;

namespace DCL
{
    public class AssetPromise_GLTFast_Instance : AssetPromise_WithUrl<Asset_GLTFast_Instance>
    {
        private readonly IWebRequestController webRequestController;
        private readonly ContentProvider contentProvider;
        private readonly AssetPromiseSettings_Rendering settings;
        private AssetPromise_GLTFast_Loader subPromise;
        private Coroutine loadingCoroutine;

        public AssetPromise_GLTFast_Instance(string contentUrl, string hash, IWebRequestController webRequestController, ContentProvider contentProvider = null, AssetPromiseSettings_Rendering settings = default)
            : base(contentUrl, hash)
        {
            this.webRequestController = webRequestController;
            this.contentProvider = contentProvider ?? new ContentProvider_Dummy();
            this.settings = settings ?? new AssetPromiseSettings_Rendering();
        }

        protected override void OnLoad(Action onSuccess, Action<Exception> onFail)
        {
            loadingCoroutine = CoroutineStarter.Start(LoadingCoroutine(onSuccess, onFail));
        }

        protected override bool AddToLibrary()
        {
            if (!library.Add(asset))
                return false;

            asset = settings.forceNewInstance ? ((AssetLibrary_GLTFast_Instance)library).GetCopyFromOriginal(asset.id) : library.Get(asset.id);

            settings.ApplyBeforeLoad(asset.container.transform);

            return true;
        }

        protected override void OnReuse(Action onSuccess)
        {
            asset.Show(onSuccess);
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
                Object.Destroy(asset.container);

            AssetPromiseKeeper_GLTFast.i.Forget(subPromise);
        }

        public override string ToString()
        {
            if (subPromise != null)
                return $"{subPromise.ToString()} ... GLTFast_GameObject state = {state}";
            else
                return $"subPromise == null? state = {state}";
        }

        public override bool keepWaiting => loadingCoroutine != null;

        private IEnumerator LoadingCoroutine(Action OnSuccess, Action<Exception> OnFail)
        {
            PerformanceAnalytics.GLTFTracker.TrackLoading();

            // Since GLTFast give us an "instantiator" we create another asset promise to create the main and only instantiator for this object
            subPromise = new AssetPromise_GLTFast_Loader(contentUrl, hash, webRequestController, contentProvider);

            var success = false;
            Exception loadingException = null;

            subPromise.OnSuccessEvent += (x) => success = true;

            subPromise.OnFailEvent += (ab, exception) =>
            {
                loadingException = exception;
                success = false;
            };

            asset.ownerPromise = subPromise;
            AssetPromiseKeeper_GLTFast.i.Keep(subPromise);

            asset.container.SetActive(false);

            yield return subPromise;

            if (success)
            {
                if (!asset.container)
                {
                    OnFail?.Invoke(new Exception("Object was destroyed during loading"));
                    yield break;
                }

                yield return subPromise.asset.InstantiateAsync(asset.container.transform).ToCoroutine(e =>
                {
                    if (e is not OperationCanceledException)
                        throw e;
                });
                yield return RemoveCollidersFromRenderers(asset.container.transform);
            }

            SetupAssetSettings();

            asset.container.SetActive(true);

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
                OnFail?.Invoke(loadingException);
            }
        }

        private void SetupAssetSettings()
        {
            if (settings.visibleFlags is AssetPromiseSettings_Rendering.VisibleFlags.INVISIBLE)
            {
                var renderers = asset.container.GetComponentsInChildren<Renderer>(true);

                foreach (Renderer renderer in renderers)
                    renderer.enabled = false;
            }
        }

        /// <summary>
        /// Our GLTFs come with visible colliders that we should get rid of, this is a requirement for our systems
        /// </summary>
        /// <param name="rootGameObject"></param>
        /// <returns></returns>
        private IEnumerator RemoveCollidersFromRenderers(Transform rootGameObject)
        {
            Utils.InverseTransformChildTraversal<Renderer>(renderer =>
            {
                if (IsCollider(renderer.transform))
                    Utils.SafeDestroy(renderer);
            }, rootGameObject.transform);

            // we wait a single frame until the collider renderers are deleted because some systems might be able to get a reference to them before this is done and we dont want that
            yield return null;
        }

        private static bool IsCollider(Transform transform)
        {
            bool transformName = transform.name.ToLower().Contains("_collider");
            bool parentName = transform.parent.name.ToLower().Contains("_collider");

            return parentName || transformName;
        }

        protected override Asset_GLTFast_Instance GetAsset(object id) =>
            settings.forceNewInstance ? ((AssetLibrary_GLTFast_Instance)library).GetCopyFromOriginal(id) : base.GetAsset(id);

        public void OverrideInitialPosition(Vector3 pos)
        {
            settings.initialLocalPosition = pos;
        }
    }
}
