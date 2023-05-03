using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DCLServices.WearablesCatalogService
{
    /// <summary>
    /// This service will choose LambdasWearablesCatalogService or WebInterfaceWearablesCatalogService for requesting
    /// wearables depending on the flag 'usingUrlParamsForDebug' received in the kernel configuration.
    /// This will be temporal while we have to live with both services.
    /// </summary>
    public class WearablesCatalogServiceProxy : IWearablesCatalogService
    {
        private const string FORCE_TO_REQUEST_WEARABLES_THROUGH_KERNEL_FF = "force_to_request_wearables_through_kernel";

        public BaseDictionary<string, WearableItem> WearablesCatalog =>
            wearablesCatalogServiceInUse?.WearablesCatalog;

        private IWearablesCatalogService wearablesCatalogServiceInUse;
        private readonly IWearablesCatalogService lambdasWearablesCatalogService;
        private readonly WebInterfaceWearablesCatalogService webInterfaceWearablesCatalogService;
        private readonly BaseDictionary<string, WearableItem> wearablesCatalog;
        private readonly KernelConfig kernelConfig;
        private readonly WearablesWebInterfaceBridge wearablesWebInterfaceBridge;
        private readonly BaseVariable<FeatureFlag> featureFlags;
        private bool isInitialized;

        public WearablesCatalogServiceProxy(
            IWearablesCatalogService lambdasWearablesCatalogService,
            WebInterfaceWearablesCatalogService webInterfaceWearablesCatalogService,
            BaseDictionary<string, WearableItem> wearablesCatalog,
            KernelConfig kernelConfig,
            WearablesWebInterfaceBridge wearablesWebInterfaceBridge,
            BaseVariable<FeatureFlag> featureFlags)
        {
            this.lambdasWearablesCatalogService = lambdasWearablesCatalogService;
            this.webInterfaceWearablesCatalogService = webInterfaceWearablesCatalogService;
            this.wearablesCatalog = wearablesCatalog;
            this.kernelConfig = kernelConfig;
            this.wearablesWebInterfaceBridge = wearablesWebInterfaceBridge;
            this.featureFlags = featureFlags;
        }

        public void Initialize()
        {
            if (!featureFlags.Get().IsInitialized)
                featureFlags.OnChange += CheckFeatureFlag;
            else
                CheckFeatureFlag(featureFlags.Get());
        }

        public void Dispose()
        {
            wearablesCatalogServiceInUse?.Dispose();
        }

        public async UniTask<(IReadOnlyList<WearableItem> wearables, int totalAmount)> RequestOwnedWearablesAsync(string userId, int pageNumber, int pageSize, CancellationToken cancellationToken, string category = null,
            NftRarity rarity = NftRarity.None, ICollection<string> collectionIds = null, string name = null,
            (NftOrderByOperation type, bool directionAscendent)? orderBy = null)
        {
            await UniTask.WaitUntil(() => isInitialized, cancellationToken: cancellationToken);

            return await lambdasWearablesCatalogService.RequestOwnedWearablesAsync(userId, pageNumber, pageSize,
                cancellationToken, category, rarity, collectionIds, name, orderBy);
        }

        public async UniTask<(IReadOnlyList<WearableItem> wearables, int totalAmount)> RequestOwnedWearablesAsync(string userId, int pageNumber, int pageSize, bool cleanCachedPages, CancellationToken ct)
        {
            await UniTask.WaitUntil(() => isInitialized, cancellationToken: ct);
            return await wearablesCatalogServiceInUse.RequestOwnedWearablesAsync(userId, pageNumber, pageSize, cleanCachedPages, ct);
        }

        public async UniTask<IReadOnlyList<WearableItem>> RequestBaseWearablesAsync(CancellationToken ct)
        {
            await UniTask.WaitUntil(() => isInitialized, cancellationToken: ct);
            return await wearablesCatalogServiceInUse.RequestBaseWearablesAsync(ct);
        }

        public async UniTask<(IReadOnlyList<WearableItem> wearables, int totalAmount)> RequestThirdPartyWearablesByCollectionAsync(string userId, string collectionId, int pageNumber, int pageSize, bool cleanCachedPages, CancellationToken ct)
        {
            await UniTask.WaitUntil(() => isInitialized, cancellationToken: ct);
            return await wearablesCatalogServiceInUse.RequestThirdPartyWearablesByCollectionAsync(userId, collectionId, pageNumber, pageSize, cleanCachedPages, ct);
        }

        public async UniTask<WearableItem> RequestWearableAsync(string wearableId, CancellationToken ct)
        {
            await UniTask.WaitUntil(() => isInitialized, cancellationToken: ct);
            return await wearablesCatalogServiceInUse.RequestWearableAsync(wearableId, ct);
        }

        public void AddWearablesToCatalog(IEnumerable<WearableItem> wearableItems) =>
            wearablesCatalogServiceInUse?.AddWearablesToCatalog(wearableItems);

        public void RemoveWearablesFromCatalog(IEnumerable<string> wearableIds) =>
            wearablesCatalogServiceInUse?.RemoveWearablesFromCatalog(wearableIds);

        public void RemoveWearableFromCatalog(string wearableId) =>
            wearablesCatalogServiceInUse?.RemoveWearableFromCatalog(wearableId);

        public void RemoveWearablesInUse(IEnumerable<string> wearablesInUseToRemove) =>
            wearablesCatalogServiceInUse?.RemoveWearablesInUse(wearablesInUseToRemove);

        public void EmbedWearables(IEnumerable<WearableItem> wearables) =>
            wearablesCatalogServiceInUse?.EmbedWearables(wearables);

        public void Clear() =>
            wearablesCatalogServiceInUse?.Clear();

        public bool IsValidWearable(string wearableId) =>
            wearablesCatalogServiceInUse.IsValidWearable(wearableId);

        private void CheckFeatureFlag(FeatureFlag currentFeatureFlags, FeatureFlag _ = null)
        {
            async UniTaskVoid SetServiceInUseDependingOnKernelConfig()
            {
                var currentKernelConfig = kernelConfig.EnsureConfigInitialized();
                await currentKernelConfig;
                SetCurrentService(currentKernelConfig.value.urlParamsForWearablesDebug);
            }

            featureFlags.OnChange -= CheckFeatureFlag;

            if (currentFeatureFlags.IsFeatureEnabled(FORCE_TO_REQUEST_WEARABLES_THROUGH_KERNEL_FF))
                SetCurrentService(true);
            else
                SetServiceInUseDependingOnKernelConfig().Forget();
        }

        private void SetCurrentService(bool useKernel)
        {
            if (useKernel)
            {
                webInterfaceWearablesCatalogService.Initialize(wearablesWebInterfaceBridge, wearablesCatalog);
                wearablesCatalogServiceInUse = webInterfaceWearablesCatalogService;
            }
            else
            {
                lambdasWearablesCatalogService.Initialize();
                wearablesCatalogServiceInUse = lambdasWearablesCatalogService;
                webInterfaceWearablesCatalogService?.Dispose();
            }

            isInitialized = true;
        }
    }
}
