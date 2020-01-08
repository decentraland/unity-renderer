using System;
using System.Collections;
using System.Linq;
using DCL.Helpers;
using DCL.Configuration;
using UnityEngine;

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
            asset.container.name = "AB: " + hash;
            settings.ApplyBeforeLoad(asset.container.transform);
        }

        protected override void OnCancelLoading()
        {
            CoroutineStarter.Stop(loadingCoroutine);
            AssetPromiseKeeper_AB.i.Forget(subPromise);
        }

        public IEnumerator LoadingCoroutine(Action OnSuccess, Action OnFail)
        {
            subPromise = new AssetPromise_AB(contentUrl, hash);
            bool success = false;
            subPromise.OnSuccessEvent += (x) => success = true;
            asset.ownerPromise = subPromise;
            AssetPromiseKeeper_AB.i.Keep(subPromise);

            yield return subPromise;

            if (success)
            {
                yield return InstantiateABGameObjects(subPromise.asset.ownerAssetBundle);

                if (subPromise.asset == null || subPromise.asset.ownerAssetBundle == null || asset.container == null)
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


        public IEnumerator InstantiateABGameObjects(AssetBundle bundle)
        {
            var goList = subPromise.asset.GetAssetsByExtensions<GameObject>("glb", "ltf");

            for (int i = 0; i < goList.Count; i++)
            {
                if (asset.container == null)
                    break;

                GameObject assetBundleModelGO = UnityEngine.Object.Instantiate(goList[i]);

                // Hide gameobject until it's been correctly processed, otherwise it flashes at 0,0,0
                assetBundleModelGO.transform.position = EnvironmentSettings.MORDOR;

                yield return MaterialCachingHelper.UseCachedMaterials(assetBundleModelGO);

                assetBundleModelGO.name = subPromise.asset.assetBundleAssetName;
#if UNITY_EDITOR
                assetBundleModelGO.GetComponentsInChildren<Renderer>().ToList().ForEach(ResetShader);
#endif
                assetBundleModelGO.transform.parent = asset.container.transform;
                assetBundleModelGO.transform.ResetLocalTRS();
                yield return null;
            }

            yield break;
        }

        protected override Asset_AB_GameObject GetAsset(object id)
        {
            if (settings.forceNewInstance)
            {
                return ((AssetLibrary_AB_GameObject)library).GetCopyFromOriginal(id);
            }
            else
            {
                return base.GetAsset(id);
            }
        }


#if UNITY_EDITOR
        private static void ResetShader(Renderer renderer)
        {
            if (renderer.sharedMaterials == null) return;

            for (int i = 0; i < renderer.sharedMaterials.Length; i++)
            {
                if (renderer == null || renderer.sharedMaterials[i] == null)
                    continue;

                renderer.sharedMaterials[i].shader = Shader.Find(renderer.sharedMaterials[i].shader.name);
            }
        }
#endif
    }
}
