﻿using Cysharp.Threading.Tasks;
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
        private readonly IWearablesCatalogService webInterfaceWearablesCatalogService;
        private readonly BaseDictionary<string, WearableItem> wearablesCatalog;
        private readonly KernelConfig kernelConfig;
        private bool isInitialized;

        public WearablesCatalogServiceProxy(
            IWearablesCatalogService lambdasWearablesCatalogService,
            IWearablesCatalogService webInterfaceWearablesCatalogService,
            BaseDictionary<string, WearableItem> wearablesCatalog,
            KernelConfig kernelConfig)
        {
            this.lambdasWearablesCatalogService = lambdasWearablesCatalogService;
            this.webInterfaceWearablesCatalogService = webInterfaceWearablesCatalogService;
            this.wearablesCatalog = wearablesCatalog;
            this.kernelConfig = kernelConfig;
        }

        public void Initialize()
        {
            kernelConfig.EnsureConfigInitialized().Then(config => OnKernelConfigChanged(config, null));
        }

        public void Dispose()
        {
            wearablesCatalogServiceInUse?.Dispose();
        }

        public async UniTask<IReadOnlyList<WearableItem>> RequestOwnedWearablesAsync(string userId, int pageNumber, int pageSize, CancellationToken ct)
        {
            await UniTask.WaitUntil(() => isInitialized, cancellationToken: ct);
            return await wearablesCatalogServiceInUse.RequestOwnedWearablesAsync(userId, pageNumber, pageSize, ct);
        }

        public async UniTask<IReadOnlyList<WearableItem>> RequestBaseWearablesAsync(CancellationToken ct)
        {
            await UniTask.WaitUntil(() => isInitialized, cancellationToken: ct);
            return await wearablesCatalogServiceInUse.RequestBaseWearablesAsync(ct);
        }

        public async UniTask<IReadOnlyList<WearableItem>> RequestThirdPartyWearablesByCollectionAsync(string userId, string collectionId, int pageNumber, int pageSize, CancellationToken ct)
        {
            await UniTask.WaitUntil(() => isInitialized, cancellationToken: ct);
            return await wearablesCatalogServiceInUse.RequestThirdPartyWearablesByCollectionAsync(userId, collectionId, pageNumber, pageSize, ct);
        }

        public async UniTask<IReadOnlyList<WearableItem>> RequestWearablesAsync(IReadOnlyList<string> wearableIds, CancellationToken ct)
        {
            await UniTask.WaitUntil(() => isInitialized, cancellationToken: ct);
            return await wearablesCatalogServiceInUse.RequestWearablesAsync(wearableIds, ct);
        }

        public async UniTask<WearableItem> RequestWearableAsync(string wearableId, CancellationToken ct)
        {
            await UniTask.WaitUntil(() => isInitialized, cancellationToken: ct);
            return await wearablesCatalogServiceInUse.RequestWearableAsync(wearableId, ct);
        }

        public void AddWearablesToCatalog(IReadOnlyList<WearableItem> wearableItems) =>
            wearablesCatalogServiceInUse?.AddWearablesToCatalog(wearableItems);

        public void RemoveWearablesFromCatalog(IReadOnlyList<string> wearableIds) =>
            wearablesCatalogServiceInUse?.RemoveWearablesFromCatalog(wearableIds);

        public void RemoveWearablesInUse(IReadOnlyList<string> wearablesInUseToRemove) =>
            wearablesCatalogServiceInUse?.RemoveWearablesInUse(wearablesInUseToRemove);

        public void EmbedWearables(IReadOnlyList<WearableItem> wearables) =>
            wearablesCatalogServiceInUse?.EmbedWearables(wearables);

        public void Clear() =>
            wearablesCatalogServiceInUse?.Clear();

        private void OnKernelConfigChanged(KernelConfigModel currentKernelConfig, KernelConfigModel previous)
        {
            if (currentKernelConfig.urlParamsForWearablesDebug)
            {
                WebInterfaceWearablesCatalogService.Instance.Initialize(new WearablesWebInterfaceBridge(), wearablesCatalog);
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
