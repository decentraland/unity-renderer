using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using DCL.Shaders;
using System.Threading;
using UnityEngine;

namespace DCL
{
    public class AssetBundlesLoader
    {
        private const float MAX_LOAD_BUDGET_TIME = 0.05f;
        private const int SKIPPED_FRAMES_AFTER_BUDGET_TIME_IS_REACHED_FOR_NEARBY_ASSETS = 1;
        private const int SKIPPED_FRAMES_AFTER_BUDGET_TIME_IS_REACHED_FOR_DISTANT_ASSETS = 5;
        private const float MAX_SQR_DISTANCE_FOR_QUICK_LOADING = 6000f;
        private const float TIME_BETWEEN_REPRIORITIZATIONS = 1f;

        private struct AssetBundleInfo
        {
            public Asset_AB asset;
            public Transform containerTransform;
            public Action onSuccess;
            public Action<Exception> onFail;

            public AssetBundleInfo(Asset_AB asset, Transform containerTransform, Action onSuccess, Action<Exception> onFail)
            {
                this.asset = asset;
                this.containerTransform = containerTransform;
                this.onSuccess = onSuccess;
                this.onFail = onFail;
            }
        }

        private Queue<AssetBundleInfo> highPriorityLoadQueue = new Queue<AssetBundleInfo>();
        private Queue<AssetBundleInfo> lowPriorityLoadQueue = new Queue<AssetBundleInfo>();

        private Dictionary<string, int> loadOrderByExtension = new Dictionary<string, int>()
        {
            { "png", 0 },
            { "jpg", 1 },
            { "peg", 2 },
            { "bmp", 3 },
            { "psd", 4 },
            { "iff", 5 },
            { "mat", 6 },
            { "nim", 7 },
            { "ltf", 8 },
            { "glb", 9 }
        };

        private List<UnityEngine.Object> loadedAssetsByName = new List<UnityEngine.Object>();
        private float currentLoadBudgetTime = 0;
        private AssetBundleInfo assetBundleInfoToLoad;
        private float lastQueuesReprioritizationTime = 0;

        private CancellationTokenSource cancellationTokenSource;

        private bool limitTimeBudget => CommonScriptableObjects.rendererState.Get();

        public void Start()
        {
            if (cancellationTokenSource != null)
                return;

            cancellationTokenSource = new CancellationTokenSource();
            LoadAssetBundlesAsync(cancellationTokenSource.Token).SuppressCancellationThrow().Forget();
        }

        public void Stop()
        {
            if (cancellationTokenSource == null)
                return;

            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;

            highPriorityLoadQueue.Clear();
            lowPriorityLoadQueue.Clear();
        }

        public void MarkAssetBundleForLoad(Asset_AB asset, Transform containerTransform, Action onSuccess, Action<Exception> onFail)
        {
            CheckForReprioritizeAwaitingAssets();

            AssetBundleInfo assetBundleToLoad = new AssetBundleInfo(asset, containerTransform, onSuccess, onFail);

            float distanceFromPlayer = GetDistanceFromPlayer(containerTransform);

            if (distanceFromPlayer <= MAX_SQR_DISTANCE_FOR_QUICK_LOADING)
                highPriorityLoadQueue.Enqueue(assetBundleToLoad);
            else
                lowPriorityLoadQueue.Enqueue(assetBundleToLoad);
        }

        private async UniTask LoadAssetBundlesAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                while (highPriorityLoadQueue.Count > 0)
                {
                    float time = Time.realtimeSinceStartup;

                    assetBundleInfoToLoad = highPriorityLoadQueue.Dequeue();
                    await LoadAssetBundleAsync(assetBundleInfoToLoad, cancellationToken);

                    if (IsLoadBudgetTimeReached(time))
                    {
                        await WaitForSkippedFrames(SKIPPED_FRAMES_AFTER_BUDGET_TIME_IS_REACHED_FOR_NEARBY_ASSETS);
                    }
                }

                while (lowPriorityLoadQueue.Count > 0 && highPriorityLoadQueue.Count == 0)
                {
                    float time = Time.realtimeSinceStartup;

                    assetBundleInfoToLoad = lowPriorityLoadQueue.Dequeue();
                    await LoadAssetBundleAsync(assetBundleInfoToLoad, cancellationToken);

                    if (IsLoadBudgetTimeReached(time))
                    {
                        await WaitForSkippedFrames(SKIPPED_FRAMES_AFTER_BUDGET_TIME_IS_REACHED_FOR_DISTANT_ASSETS);
                    }
                }

                await UniTask.Yield();
            }
        }

        private async UniTask LoadAssetBundleAsync(AssetBundleInfo assetBundleInfo, CancellationToken ct)
        {
            if (!assetBundleInfo.asset.IsValid())
            {
                assetBundleInfo.onFail?.Invoke(new Exception("Asset bundle is null"));
                return;
            }

            AssetBundleRequest abRequest = assetBundleInfo.asset.LoadAllAssetsAsync();
            await abRequest.WithCancellation(ct);

            loadedAssetsByName = abRequest.allAssets.ToList();

            foreach (var loadedAsset in loadedAssetsByName)
            {
                string ext = "any";

                if (loadedAsset is Texture) { ext = "png"; }
                else if (loadedAsset is Material material)
                {
                    ShaderUtils.UpgradeMaterial_2020_To_2021(material);
                    ext = "mat";
                }
                else if (loadedAsset is Animation || loadedAsset is AnimationClip) { ext = "nim"; }
                else if (loadedAsset is GameObject) { ext = "glb"; }

                assetBundleInfo.asset.AddAssetByExtension(ext, loadedAsset);
            }

            loadedAssetsByName.Clear();
            assetBundleInfo.onSuccess?.Invoke();
        }

        private bool IsLoadBudgetTimeReached(float startTime)
        {
            if (limitTimeBudget)
            {
                currentLoadBudgetTime += Time.realtimeSinceStartup - startTime;

                if (currentLoadBudgetTime > MAX_LOAD_BUDGET_TIME)
                {
                    currentLoadBudgetTime = 0f;
                    return true;
                }
            }

            return false;
        }

        private UniTask WaitForSkippedFrames(int skippedFramesBetweenLoadings) =>
            UniTask.DelayFrame(skippedFramesBetweenLoadings);

        private void CheckForReprioritizeAwaitingAssets()
        {
            if (lowPriorityLoadQueue.Count == 0 ||
                (Time.realtimeSinceStartup - lastQueuesReprioritizationTime) < TIME_BETWEEN_REPRIORITIZATIONS)
                return;

            while (lowPriorityLoadQueue.Count > 0 && GetDistanceFromPlayer(lowPriorityLoadQueue.Peek().containerTransform) <= MAX_SQR_DISTANCE_FOR_QUICK_LOADING)
            {
                highPriorityLoadQueue.Enqueue(lowPriorityLoadQueue.Dequeue());
                lastQueuesReprioritizationTime = Time.realtimeSinceStartup;
            }
        }

        private float GetDistanceFromPlayer(Transform containerTransform)
        {
            return (containerTransform != null && limitTimeBudget) ? Vector3.SqrMagnitude(containerTransform.position - CommonScriptableObjects.playerUnityPosition.Get()) : 0f;
        }
    }
}
