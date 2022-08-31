using System;
using System.Collections;
using DCL.Helpers;
using UnityEngine;
using MainScripts.DCL.Analytics.PerformanceAnalytics;
using Object = UnityEngine.Object;

namespace DCL
{
    public class AssetPromise_GLTFast_GameObject : AssetPromise_WithUrl<Asset_GLTFast_GameObject>
    {
        private readonly IWebRequestController webRequestController;
        private readonly ContentProvider contentProvider;
        public AssetPromiseSettings_Rendering settings = new AssetPromiseSettings_Rendering();
        AssetPromise_GLTFast subPromise;
        Coroutine loadingCoroutine;

        public AssetPromise_GLTFast_GameObject(string contentUrl, string hash, IWebRequestController webRequestController, ContentProvider contentProvider = null) : base(contentUrl, hash)
        {
            this.webRequestController = webRequestController;
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
            subPromise = new AssetPromise_GLTFast(contentUrl, hash, webRequestController, contentProvider);
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
                Debug.LogException(loadingException);
                OnFail?.Invoke(loadingException);
            }
        }
        
        private IEnumerator RemoveColliderRenderers(Transform rootGameObject)
        {
            Utils.InverseTransformChildTraversal<Renderer>(renderer =>
            {
                if (IsCollider(renderer.transform))
                {
                    if (renderer.name.Contains("meteor"))
                    {
                        Debug.Log("<color=red>THIS IS WHERE THE RENDERER GETS DELETED</color>");
                    }
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