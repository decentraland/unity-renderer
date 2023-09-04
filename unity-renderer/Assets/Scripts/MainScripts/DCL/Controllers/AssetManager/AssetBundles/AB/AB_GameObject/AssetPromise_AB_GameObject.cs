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
        public AssetPromiseSettings_Rendering settings = new ();
        private AssetPromise_AB subPromise;
        private Coroutine loadingCoroutine;

        private BaseVariable<FeatureFlag> featureFlags => DataStore.i.featureFlags.flags;
        private const string FEATURE_AB_MESH_GPU = "ab-mesh-gpu";

        public AssetPromise_AB_GameObject(string contentUrl, string hash) : base(contentUrl, hash) { }

        protected override void OnLoad(Action OnSuccess, Action<Exception> OnFail)
        {
            loadingCoroutine = CoroutineStarter.Start(LoadingCoroutine(OnSuccess, OnFail));
        }

        protected override bool AddToLibrary()
        {
            if (!library.Add(asset))
                return false;

            asset = settings.forceNewInstance
                ? ((AssetLibrary_AB_GameObject)library).GetCopyFromOriginal(asset.id)
                : library.Get(asset.id);

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

        protected override void OnBeforeLoadOrReuse()
        {
            settings.ApplyBeforeLoad(asset.container.transform);
        }

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

        public override string ToString() =>
            subPromise != null ? $"{subPromise} ... AB_GameObject state = {state}" : $"subPromise == null? state = {state}";

        private IEnumerator LoadingCoroutine(Action OnSuccess, Action<Exception> OnFail)
        {
            PerformanceAnalytics.ABTracker.TrackLoading();
            subPromise = new AssetPromise_AB(contentUrl, hash, asset.container.transform);
            var success = false;
            Exception loadingException = null;
            subPromise.OnSuccessEvent += (x) => success = true;

            subPromise.OnFailEvent += (ab, exception) =>
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
                OnFail?.Invoke(loadingException);
            }
        }

        private IEnumerator InstantiateABGameObjects()
        {
            var gameObjects = subPromise.asset.GetAssetsByExtensions<GameObject>("glb", "ltf");

            if (gameObjects.Count == 0)
            {
                if (asset.container != null)
                    UnityEngine.Object.Destroy(asset.container);

                asset.container = null;

                AssetPromiseKeeper_AB.i.Forget(subPromise);

                yield break;
            }

            foreach (var gameObject in gameObjects)
            {
                if (loadingCoroutine == null)
                    break;

                if (asset.container == null)
                    break;

                GameObject bundledGameObject = UnityEngine.Object.Instantiate(gameObject, asset.container.transform);

                asset.renderers = MeshesInfoUtils.ExtractUniqueRenderers(bundledGameObject);
                asset.materials = MeshesInfoUtils.ExtractUniqueMaterials(asset.renderers);
                asset.SetTextures(MeshesInfoUtils.ExtractUniqueTextures(asset.materials));
                OptimizeMaterials(MeshesInfoUtils.ExtractUniqueMaterials(asset.renderers));
                UploadMeshesToGPU(MeshesInfoUtils.ExtractUniqueMeshes(asset.renderers));
                asset.totalTriangleCount = MeshesInfoUtils.ComputeTotalTriangles(asset.renderers, asset.meshToTriangleCount);

                var animators = MeshesInfoUtils.ExtractUniqueAnimations(bundledGameObject);
                asset.animationClipSize = subPromise.asset.metrics.animationsEstimatedSize;
                asset.meshDataSize = subPromise.asset.metrics.meshesEstimatedSize;

                foreach (var animator in animators) { animator.cullingType = AnimationCullingType.AlwaysAnimate; }

#if UNITY_EDITOR
                bundledGameObject.name = subPromise.asset.GetName();
#endif
            }
        }

        private void OptimizeMaterials(HashSet<Material> materials)
        {
            var litMat = Resources.Load<Material>("LitMaterial");
            foreach (Material material in materials)
                material.shader = litMat.shader;
        }

        private void UploadMeshesToGPU(HashSet<Mesh> meshesList)
        {
            bool uploadMesh = IsUploadMeshToGPUEnabled();

            foreach (Mesh mesh in meshesList)
            {
                if (!mesh.isReadable)
                    continue;

                asset.meshToTriangleCount[mesh] = mesh.triangles.Length;
                asset.meshes.Add(mesh);

                if (!uploadMesh) continue;

                bool isCollider = mesh.name.Contains("_collider", StringComparison.OrdinalIgnoreCase);

                // colliders will fail to be created if they are not readable on WebGL
                if (!isCollider)
                {
                    Physics.BakeMesh(mesh.GetInstanceID(), false);
                    mesh.UploadMeshData(true);
                }
            }
        }

        protected override Asset_AB_GameObject GetAsset(object id) =>
            settings.forceNewInstance ? ((AssetLibrary_AB_GameObject)library).GetCopyFromOriginal(id) : base.GetAsset(id);

        private bool IsUploadMeshToGPUEnabled() =>
            featureFlags.Get().IsFeatureEnabled(FEATURE_AB_MESH_GPU);
    }
}
