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
        private const string NOT_INITIALIZED_EXCEPTION_MESSAGE = "The proxy was not yet initialized!";

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
            kernelConfig.OnChange += OnKernelConfigChanged;
        }

        public void Dispose()
        {
            kernelConfig.OnChange -= OnKernelConfigChanged;
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

        public async UniTask<IReadOnlyList<WearableItem>> RequestWearablesAsync(string[] wearableIds, CancellationToken ct)
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

        public void RemoveWearablesFromCatalog(IEnumerable<string> wearableIds) =>
            wearablesCatalogServiceInUse?.RemoveWearablesFromCatalog(wearableIds);

        public void RemoveWearablesInUse(IEnumerable<string> wearablesInUseToRemove) =>
            wearablesCatalogServiceInUse?.RemoveWearablesInUse(wearablesInUseToRemove);

        public void EmbedWearables(IEnumerable<WearableItem> wearables) =>
            wearablesCatalogServiceInUse?.EmbedWearables(wearables);

        public void Clear() =>
            wearablesCatalogServiceInUse?.Clear();

        private void OnKernelConfigChanged(KernelConfigModel currentKernelConfig, KernelConfigModel previous)
        {
            kernelConfig.OnChange -= OnKernelConfigChanged;

            if (currentKernelConfig.urlParamsForWearablesDebug)
            {
                WebInterfaceWearablesCatalogService.Instance.Initialize(new WearablesWebInterfaceBridge(), wearablesCatalog);
                wearablesCatalogServiceInUse = webInterfaceWearablesCatalogService;
                lambdasWearablesCatalogService?.Dispose();
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
