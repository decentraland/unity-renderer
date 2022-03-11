using System;
using System.Collections;
using System.Linq;
using DCL.Helpers;
using UnityEngine;
using System.Collections.Generic;
using DCL.Models;

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

        protected override void OnLoad(Action OnSuccess, Action<Exception> OnFail)
        {
            if(isDebug)
                Debug.Log($"{GetType()}.OnLoad() - {GetId()}");
            
            loadingCoroutine = CoroutineStarter.Start(LoadingCoroutine(OnSuccess, OnFail));
        }

        protected override bool AddToLibrary()
        {
            if(isDebug)
                Debug.Log($"{GetType()}.AddToLibrary() - {GetId()} - 1");
            
            if (!library.Add(asset))
            {
                return false;
            }
            
            if(isDebug)
                Debug.Log($"{GetType()}.AddToLibrary() - {GetId()} - 2");

            if (settings.forceNewInstance)
            {
                asset = (library as AssetLibrary_AB_GameObject).GetCopyFromOriginal(asset.id);
            }
            else
            {
                asset = library.Get(asset.id);
            }
            
            if(isDebug)
                Debug.Log($"{GetType()}.AddToLibrary() - {GetId()} - 3");

            //NOTE(Brian): Call again this method because we are replacing the asset.
            settings.ApplyBeforeLoad(asset.container.transform);
            
            if(isDebug)
                Debug.Log($"{GetType()}.AddToLibrary() - {GetId()} - 4");

            return true;
        }

        protected override void OnReuse(Action OnSuccess)
        {
            if(isDebug)
                Debug.Log($"{GetType()}.OnReuse() - {GetId()}");
            asset.renderers = MeshesInfoUtils.ExtractUniqueRenderers(asset.container);
            asset.Show(OnSuccess);
        }

        protected override void OnAfterLoadOrReuse()
        {
            if(isDebug)
                Debug.Log($"{GetType()}.OnAfterLoadOrReuse() - {GetId()}");
            
            asset.renderers = MeshesInfoUtils.ExtractUniqueRenderers(asset.container);
            settings.ApplyAfterLoad(asset.container.transform);
        }

        protected override void OnBeforeLoadOrReuse()
        {
            if(isDebug)
                Debug.Log($"{GetType()}.OnBeforeLoadOrReuse() - {GetId()}");
            
#if UNITY_EDITOR
            asset.container.name = "AB: " + hash;
#endif
            settings.ApplyBeforeLoad(asset.container.transform);
        }

        protected override void OnCancelLoading()
        {
            if(isDebug)
                Debug.Log($"{GetType()}.OnCancelLoading() - {GetId()}");
            
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

        public IEnumerator LoadingCoroutine(Action OnSuccess, Action<Exception> OnFail)
        {
            if(isDebug)
                Debug.Log($"{GetType()}.LoadingCoroutine() - {GetId()} - 1");
            
            subPromise = new AssetPromise_AB(contentUrl, hash, asset.container.transform);
            bool success = false;
            subPromise.OnSuccessEvent += (x) => success = true;
            asset.ownerPromise = subPromise;
            subPromise.isDebug = isDebug;
            AssetPromiseKeeper_AB.i.Keep(subPromise);
            
            if(isDebug)
                Debug.Log($"{GetType()}.LoadingCoroutine() - {GetId()} - 2");

            yield return subPromise;
            
            if(isDebug)
                Debug.Log($"{GetType()}.LoadingCoroutine() - {GetId()} - 3");

            if (success)
            {
                if(isDebug)
                    Debug.Log($"{GetType()}.LoadingCoroutine() - {GetId()} - SUCCESS - 4");
                yield return InstantiateABGameObjects();

                if (subPromise.asset == null || asset.container == null)
                    success = false;
            }
            
            if(isDebug)
                Debug.Log($"{GetType()}.LoadingCoroutine() - {GetId()} - 5");

            if (success)
            {
                if(isDebug)
                    Debug.Log($"{GetType()}.LoadingCoroutine() - {GetId()} - 6A");
                OnSuccess?.Invoke();
            }
            else
            {
                if(isDebug)
                    Debug.Log($"{GetType()}.LoadingCoroutine() - {GetId()} - 6B");
                OnFail?.Invoke(new Exception($"AB sub-promise asset or container is null. Asset: {subPromise.asset}, container: {asset.container}"));
            }
        }

        public IEnumerator InstantiateABGameObjects()
        {
            if(isDebug)
                Debug.Log($"{GetType()}.InstantiateABGameObjects() - {GetId()} - 1");
            
            var goList = subPromise.asset.GetAssetsByExtensions<GameObject>("glb", "ltf");

            if (goList.Count == 0)
            {
                if (asset.container != null)
                    UnityEngine.Object.Destroy(asset.container);

                asset.container = null;

                AssetPromiseKeeper_AB.i.Forget(subPromise);

                yield break;
            }
            
            if(isDebug)
                Debug.Log($"{GetType()}.InstantiateABGameObjects() - {GetId()} - 2");

            for (int i = 0; i < goList.Count; i++)
            {
                if(isDebug)
                    Debug.Log($"{GetType()}.InstantiateABGameObjects() - {GetId()} - 3...{i}");
                
                if (loadingCoroutine == null)
                    break;
                
                if(isDebug)
                    Debug.Log($"{GetType()}.InstantiateABGameObjects() - {GetId()} - 4...{i}");

                if (asset.container == null)
                    break;
                
                if(isDebug)
                    Debug.Log($"{GetType()}.InstantiateABGameObjects() - {GetId()} - 5...{i}");

                GameObject assetBundleModelGO = UnityEngine.Object.Instantiate(goList[i], asset.container.transform);

                asset.renderers = MeshesInfoUtils.ExtractUniqueRenderers(assetBundleModelGO);
                asset.materials = MeshesInfoUtils.ExtractUniqueMaterials(asset.renderers);
                asset.textures = MeshesInfoUtils.ExtractUniqueTextures(asset.materials);
                UploadMeshesToGPU(MeshesInfoUtils.ExtractUniqueMeshes(asset.renderers));
                asset.totalTriangleCount = MeshesInfoUtils.ComputeTotalTriangles(asset.renderers, asset.meshToTriangleCount);

                if(isDebug)
                    Debug.Log($"{GetType()}.InstantiateABGameObjects() - {GetId()} - 6...{i}");
                
                //NOTE(Brian): Renderers are enabled in settings.ApplyAfterLoad
                yield return MaterialCachingHelper.Process(asset.renderers.ToList(), enableRenderers: false, settings.cachingFlags);

                var animators = assetBundleModelGO.GetComponentsInChildren<Animation>(true);

                for (int animIndex = 0; animIndex < animators.Length; animIndex++)
                {
                    animators[animIndex].cullingType = AnimationCullingType.AlwaysAnimate;
                }

#if UNITY_EDITOR
                assetBundleModelGO.name = subPromise.asset.assetBundleAssetName;
#endif
                assetBundleModelGO.transform.ResetLocalTRS();
                yield return null;
            }
            
            if(isDebug)
                Debug.Log($"{GetType()}.InstantiateABGameObjects() - {GetId()} - 7");
        }

        private void UploadMeshesToGPU(HashSet<Mesh> meshesList)
        {
            if(isDebug)
                Debug.Log($"{GetType()}.UploadMeshesToGPU() - {GetId()}");
            
            var uploadToGPU = DataStore.i.featureFlags.flags.Get().IsFeatureEnabled(FeatureFlag.GPU_ONLY_MESHES);
            foreach ( Mesh mesh in meshesList )
            {
                if ( !mesh.isReadable )
                    continue;

                asset.meshToTriangleCount[mesh] = mesh.triangles.Length;
                asset.meshes.Add(mesh);
                if (uploadToGPU)
                {
                    Physics.BakeMesh(mesh.GetInstanceID(), false);
                    mesh.UploadMeshData(true);
                }
            }
        }

        protected override Asset_AB_GameObject GetAsset(object id)
        {
            if (settings.forceNewInstance)
            {
                if(isDebug)
                    Debug.Log($"{GetType()}.GetAsset() - {GetId()} - 1");
                
                return ((AssetLibrary_AB_GameObject) library).GetCopyFromOriginal(id);
            }
            else
            {
                if(isDebug)
                    Debug.Log($"{GetType()}.GetAsset() - {GetId()} - 2");
                
                return base.GetAsset(id);
            }
        }
    }
}