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
        public BaseDictionary<string, WearableItem> WearablesCatalog =>
            wearablesCatalogServiceInUse.WearablesCatalog;

        private IWearablesCatalogService wearablesCatalogServiceInUse;
        private readonly IWearablesCatalogService lambdasWearablesCatalogService;
        private readonly WebInterfaceWearablesCatalogService webInterfaceWearablesCatalogService;
        private readonly BaseDictionary<string, WearableItem> wearablesCatalog;
        private readonly KernelConfig kernelConfig;
        private readonly WearablesWebInterfaceBridge wearablesWebInterfaceBridge;
        private bool isInitialized;

        public WearablesCatalogServiceProxy(
            IWearablesCatalogService lambdasWearablesCatalogService,
            WebInterfaceWearablesCatalogService webInterfaceWearablesCatalogService,
            BaseDictionary<string, WearableItem> wearablesCatalog,
            KernelConfig kernelConfig,
            WearablesWebInterfaceBridge wearablesWebInterfaceBridge)
        {
            this.lambdasWearablesCatalogService = lambdasWearablesCatalogService;
            this.webInterfaceWearablesCatalogService = webInterfaceWearablesCatalogService;
            this.wearablesCatalog = wearablesCatalog;
            this.kernelConfig = kernelConfig;
            this.wearablesWebInterfaceBridge = wearablesWebInterfaceBridge;
        }

        public void Initialize()
        {
            kernelConfig.EnsureConfigInitialized().Then(SetServiceInUse);
        }

        public void Dispose()
        {
            wearablesCatalogServiceInUse?.Dispose();
        }

        public async UniTask<IReadOnlyList<WearableItem>> RequestOwnedWearablesAsync(string userId, int pageNumber, int pageSize, bool cleanCachedPages, CancellationToken ct)
        {
            await UniTask.WaitUntil(() => isInitialized, cancellationToken: ct);
            return await wearablesCatalogServiceInUse.RequestOwnedWearablesAsync(userId, pageNumber, pageSize, cleanCachedPages, ct);
        }

        public async UniTask<IReadOnlyList<WearableItem>> RequestBaseWearablesAsync(CancellationToken ct)
        {
            await UniTask.WaitUntil(() => isInitialized, cancellationToken: ct);
            return await wearablesCatalogServiceInUse.RequestBaseWearablesAsync(ct);
        }

        public async UniTask<IReadOnlyList<WearableItem>> RequestThirdPartyWearablesByCollectionAsync(string userId, string collectionId, int pageNumber, int pageSize, bool cleanCachedPages, CancellationToken ct)
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

        public void RemoveWearablesInUse(IEnumerable<string> wearablesInUseToRemove) =>
            wearablesCatalogServiceInUse?.RemoveWearablesInUse(wearablesInUseToRemove);

        public void EmbedWearables(IEnumerable<WearableItem> wearables) =>
            wearablesCatalogServiceInUse?.EmbedWearables(wearables);

        public void Clear() =>
            wearablesCatalogServiceInUse?.Clear();

        public bool IsValidWearable(string wearableId) =>
            wearablesCatalogServiceInUse.IsValidWearable(wearableId);

        private void SetServiceInUse(KernelConfigModel currentKernelConfig)
        {
            if (currentKernelConfig.urlParamsForWearablesDebug)
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
