using System;
using System.Collections;
using System.Linq;
using DCL.Helpers;
using UnityEngine;
using System.Collections.Generic;
using DCL.Models;
using MainScripts.DCL.Analytics.PerformanceAnalytics;

namespace DCL
{
    public class AssetPromise_AB_GameObject : AssetPromise_WithUrl<Asset_AB_GameObject>
    {
        public AssetPromiseSettings_Rendering settings = new AssetPromiseSettings_Rendering();
        AssetPromise_AB subPromise;
        Coroutine loadingCoroutine;

        private BaseVariable<FeatureFlag> featureFlags => DataStore.i.featureFlags.flags;
        private const string AB_LOAD_ANIMATION = "ab_load_animation";
        private const string GPU_ONLY_MESHES = "use_gpu_only_meshes_variant:enabled";
        private bool doTransitionAnimation;

        public AssetPromise_AB_GameObject(string contentUrl, string hash) : base(contentUrl, hash)
        {
            featureFlags.OnChange += OnFeatureFlagChange;
            OnFeatureFlagChange(featureFlags.Get(), null);
        }

        private void OnFeatureFlagChange(FeatureFlag current, FeatureFlag previous) { doTransitionAnimation = current.IsFeatureEnabled(AB_LOAD_ANIMATION); }

        protected override void OnLoad(Action OnSuccess, Action<Exception> OnFail) { loadingCoroutine = CoroutineStarter.Start(LoadingCoroutine(OnSuccess, OnFail)); }

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
            asset.renderers = MeshesInfoUtils.ExtractUniqueRenderers(asset.container);
            asset.Show(OnSuccess);
        }

        protected override void OnAfterLoadOrReuse()
        {
            asset.renderers = MeshesInfoUtils.ExtractUniqueRenderers(asset.container);
            settings.ApplyAfterLoad(asset.container.transform);
        }

        protected override void OnBeforeLoadOrReuse() { settings.ApplyBeforeLoad(asset.container.transform); }

        protected override void OnCancelLoading()
        {
            if (loadingCoroutine != null)
            {
                PerformanceAnalytics.ABTracker.TrackCancelled();
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
            PerformanceAnalytics.ABTracker.TrackLoading();
            subPromise = new AssetPromise_AB(contentUrl, hash, asset.container.transform);
            bool success = false;
            Exception loadingException = null;
            subPromise.OnSuccessEvent += (x) => success = true;

            subPromise.OnFailEvent += ( ab,  exception) =>
            {
                loadingException = exception;
                success = false;
            };

            asset.ownerPromise = subPromise;
            AssetPromiseKeeper_AB.i.Keep(subPromise);

            yield return subPromise;

            if (success)
            {
                yield return InstantiateABGameObjects();

                if (subPromise.asset == null || asset.container == null)
                    success = false;
            }

            loadingCoroutine = null;

            if (success)
            {
                PerformanceAnalytics.ABTracker.TrackLoaded();
                OnSuccess?.Invoke();
            }
            else
            {
                PerformanceAnalytics.ABTracker.TrackFailed();
                loadingException ??= new Exception($"AB sub-promise asset or container is null. Asset: {subPromise.asset}, container: {asset.container}");
                Debug.LogException(loadingException);
                OnFail?.Invoke(loadingException);
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

                asset.renderers = MeshesInfoUtils.ExtractUniqueRenderers(assetBundleModelGO);
                asset.materials = MeshesInfoUtils.ExtractUniqueMaterials(asset.renderers);
                asset.SetTextures(MeshesInfoUtils.ExtractUniqueTextures(asset.materials));

                UploadMeshesToGPU(MeshesInfoUtils.ExtractUniqueMeshes(asset.renderers));
                asset.totalTriangleCount = MeshesInfoUtils.ComputeTotalTriangles(asset.renderers, asset.meshToTriangleCount);

                //NOTE(Brian): Renderers are enabled in settings.ApplyAfterLoad
                yield return MaterialCachingHelper.Process(asset.renderers.ToList(), enableRenderers: false, settings.cachingFlags);

                var animators = MeshesInfoUtils.ExtractUniqueAnimations(assetBundleModelGO);
                asset.animationClipSize = subPromise.asset.metrics.animationsEstimatedSize;
                asset.meshDataSize = subPromise.asset.metrics.meshesEstimatedSize;

                foreach (var animator in animators)
                {
                    animator.cullingType = AnimationCullingType.AlwaysAnimate;
                }

#if UNITY_EDITOR
                assetBundleModelGO.name = subPromise.asset.GetName();
#endif
                assetBundleModelGO.transform.ResetLocalTRS();

                yield return null;

                yield return SetMaterialTransition();
            }
        }

        private void UploadMeshesToGPU(HashSet<Mesh> meshesList)
        {
            var uploadToGPU = featureFlags.Get().IsFeatureEnabled(GPU_ONLY_MESHES);
            
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
                return ((AssetLibrary_AB_GameObject) library).GetCopyFromOriginal(id);
            }
            else
            {
                return base.GetAsset(id);
            }
        }

        internal override void OnForget()
        {
            base.OnForget();
            featureFlags.OnChange -= OnFeatureFlagChange;
        }

        IEnumerator SetMaterialTransition(Action OnSuccess = null)
        {
            if (settings.visibleFlags != AssetPromiseSettings_Rendering.VisibleFlags.INVISIBLE && doTransitionAnimation)
            {
                MaterialTransitionController[] materialTransitionControllers = new MaterialTransitionController[asset.renderers.Count];
                int index = 0;
                foreach (Renderer assetRenderer in asset.renderers)
                {
                    MaterialTransitionController transition = assetRenderer.gameObject.AddComponent<MaterialTransitionController>();
                    materialTransitionControllers[index] = transition;
                    transition.delay = 0;
                    transition.OnDidFinishLoading(assetRenderer.sharedMaterial);

                    index++;
                }
                // Wait until MaterialTransitionController finishes its effect
                yield return new WaitUntil(() => IsTransitionFinished(materialTransitionControllers));
            }
            OnSuccess?.Invoke();
        }

        private bool IsTransitionFinished(MaterialTransitionController[] matTransitions)
        {
            bool finishedTransition = true;

            for (int i = 0; i < matTransitions.Length; i++)
            {
                if (matTransitions[i] != null)
                {
                    finishedTransition = false;

                    break;
                }
            }

            return finishedTransition;
        }

    }

}