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
            this.settings = settings;
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

        private IEnumerator LoadingCoroutine(Action OnSuccess, Action<Exception> OnFail)
        {
            PerformanceAnalytics.GLTFTracker.TrackLoading();
            
            // Since GLTFast give us an "instantiator" we create another asset promise to create the main and only instantiator for this object
            subPromise = new AssetPromise_GLTFast_Loader(contentUrl, hash, webRequestController, contentProvider);
            
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
                if (asset.container == null)
                {
                    Debug.LogError("this should not happen");
                }
                else
                {
                    subPromise.asset.Instantiate(asset.container.transform);

                    yield return RemoveColliderRenderers(asset.container.transform);
                }
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
                OnFail?.Invoke(loadingException);
            }
        }
        
        /// <summary>
        /// Our GLTFs come with visible colliders that we should get rid of, this is a requirement for our systems
        /// </summary>
        /// <param name="rootGameObject"></param>
        /// <returns></returns>
        private IEnumerator RemoveColliderRenderers(Transform rootGameObject)
        {
            Utils.InverseTransformChildTraversal<Renderer>(renderer =>
            {
                if (IsCollider(renderer.transform))
                {
                    Object.Destroy(renderer);
                }
            }, rootGameObject.transform);

            // we wait a single frame until the collider renderers are deleted because some systems might be able to get a reference to them before this is done and we dont want that
            yield return new WaitForEndOfFrame();
        }
        
        private static bool IsCollider(Transform transform)
        {
            bool transformName = transform.name.ToLower().Contains("_collider");
            bool parentName = transform.parent.name.ToLower().Contains("_collider");

            return parentName || transformName;
        }

        protected override Asset_GLTFast_Instance GetAsset(object id)
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
        public void OverrideInitialPosition(Vector3 pos)
        {
            settings.initialLocalPosition = pos;
        }
    }

}