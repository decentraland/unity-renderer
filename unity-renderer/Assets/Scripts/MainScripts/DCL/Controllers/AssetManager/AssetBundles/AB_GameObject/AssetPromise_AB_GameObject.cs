using System;
using System.Collections;
using System.Linq;
using DCL.Helpers;
using DCL.Configuration;
using UnityEngine;
using System.Collections.Generic;

namespace DCL
{
    public class AssetPromise_AB_GameObject : AssetPromise_WithUrl<Asset_AB_GameObject>
    {
        public AssetPromiseSettings_Rendering settings = new AssetPromiseSettings_Rendering();
        AssetPromise_AB subPromise;
        Coroutine loadingCoroutine;

        public AssetPromise_AB_GameObject(string contentUrl, string hash) : base(contentUrl, hash)
        {
        }

        protected override void OnLoad(Action OnSuccess, Action OnFail)
        {
            loadingCoroutine = CoroutineStarter.Start(LoadingCoroutine(OnSuccess, OnFail));
        }

        protected override bool AddToLibrary()
        {
            if (!library.Add(asset))
            {
                return false;
            }

            if (settings.forceNewInstance)
            {
                asset = (library as AssetLibrary_AB_GameObject).GetCopyFromOriginal(asset.id);
            }
            else
            {
                asset = library.Get(asset.id);
            }

            //NOTE(Brian): Call again this method because we are replacing the asset.
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
#if UNITY_EDITOR
            asset.container.name = "AB: " + hash;
#endif
            settings.ApplyBeforeLoad(asset.container.transform);
        }

        protected override void OnCancelLoading()
        {
            if (loadingCoroutine != null)
            {
                CoroutineStarter.Stop(loadingCoroutine);
                loadingCoroutine = null;
            }

            if (asset != null)
                UnityEngine.Object.Destroy(asset.container);

            AssetPromiseKeeper_AB.i.Forget(subPromise);
        }


        public override string ToString()
        {
            if (subPromise != null)
                return $"{subPromise.ToString()} ... AB_GameObject state = {state}";
            else
                return $"subPromise == null? state = {state}";
        }

        public IEnumerator LoadingCoroutine(Action OnSuccess, Action OnFail)
        {
            subPromise = new AssetPromise_AB(contentUrl, hash, asset.container.transform);
            bool success = false;
            subPromise.OnSuccessEvent += (x) => success = true;
            asset.ownerPromise = subPromise;
            AssetPromiseKeeper_AB.i.Keep(subPromise);

            yield return subPromise;

            if (success)
            {
                yield return InstantiateABGameObjects();

                if (subPromise.asset == null || asset.container == null)
                    success = false;
            }

            if (success)
            {
                OnSuccess?.Invoke();
            }
            else
            {
                OnFail?.Invoke();
            }
        }


        public IEnumerator InstantiateABGameObjects()
        {
            var goList = subPromise.asset.GetAssetsByExtensions<GameObject>("glb", "ltf");

            if (goList.Count == 0)
            {
                if (asset.container != null)
                    UnityEngine.Object.Destroy(asset.container);

                asset.container = null;

                AssetPromiseKeeper_AB.i.Forget(subPromise);

                yield break;
            }

            for (int i = 0; i < goList.Count; i++)
            {
                if (loadingCoroutine == null)
                    break;

                if (asset.container == null)
                    break;

                GameObject assetBundleModelGO = UnityEngine.Object.Instantiate(goList[i], asset.container.transform);
                var list = new List<Renderer>(assetBundleModelGO.GetComponentsInChildren<Renderer>(true));

                //NOTE(Brian): Renderers are enabled in settings.ApplyAfterLoad
                yield return MaterialCachingHelper.Process(list, enableRenderers: false, settings.cachingFlags);

                var animators = assetBundleModelGO.GetComponentsInChildren<Animation>(true);

                for (int animIndex = 0; animIndex < animators.Length; animIndex++)
                {
                    animators[animIndex].cullingType = AnimationCullingType.AlwaysAnimate;
                }

#if UNITY_EDITOR
                assetBundleModelGO.name = subPromise.asset.assetBundleAssetName;
#endif
                //assetBundleModelGO.transform.SetParent(asset.container.transform, false);
                assetBundleModelGO.transform.ResetLocalTRS();
                yield return null;
            }
        }

        protected override Asset_AB_GameObject GetAsset(object id)
        {
            if (settings.forceNewInstance)
            {
                return ((AssetLibrary_AB_GameObject) library).GetCopyFromOriginal(id);
            }
            else
            {
                return base.GetAsset(id);
            }
        }
    }
}