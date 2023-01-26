using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace DCLServices.WearablesCatalogService
{
    public class WearablesCatalogService : IWearablesCatalogService
    {
        public BaseDictionary<string, WearableItem> WearablesCatalog =>
            wearablesCatalogServiceInUse.WearablesCatalog;

        private readonly IWearablesCatalogService lambdasWearablesCatalogService;
        private readonly IWearablesCatalogService webInterfaceWearablesCatalogService;
        private readonly BaseDictionary<string, WearableItem> wearablesCatalog;
        private IWearablesCatalogService wearablesCatalogServiceInUse;

        public WearablesCatalogService(
            IWearablesCatalogService lambdasWearablesCatalogService,
            IWearablesCatalogService webInterfaceWearablesCatalogService,
            BaseDictionary<string, WearableItem> wearablesCatalog)
        {
            this.lambdasWearablesCatalogService = lambdasWearablesCatalogService;
            this.webInterfaceWearablesCatalogService = webInterfaceWearablesCatalogService;
            this.wearablesCatalog = wearablesCatalog;
        }

        public void Initialize()
        {
            KernelConfig.i.EnsureConfigInitialized().Then(config => OnKernelConfigChanged(config, null));
            KernelConfig.i.OnChange += OnKernelConfigChanged;
        }

        public void Dispose()
        {
            KernelConfig.i.OnChange -= OnKernelConfigChanged;
            wearablesCatalogServiceInUse?.Dispose();
        }

        public UniTask<WearableItem[]> RequestOwnedWearablesAsync(string userId, int pageNumber, int pageSize, CancellationToken ct) =>
            wearablesCatalogServiceInUse.RequestOwnedWearablesAsync(userId, pageNumber, pageSize, ct);

        public UniTask<WearableItem[]> RequestBaseWearablesAsync(CancellationToken ct) =>
            wearablesCatalogServiceInUse.RequestBaseWearablesAsync(ct);

        public UniTask<WearableItem[]> RequestThirdPartyWearablesByCollectionAsync(string userId, string collectionId, CancellationToken ct) =>
            wearablesCatalogServiceInUse.RequestThirdPartyWearablesByCollectionAsync(userId, collectionId, ct);

        public UniTask<WearableItem[]> RequestWearablesAsync(string[] wearableIds, CancellationToken ct) =>
            wearablesCatalogServiceInUse.RequestWearablesAsync(wearableIds, ct);

        public UniTask<WearableItem> RequestWearableAsync(string wearableId, CancellationToken ct) =>
            wearablesCatalogServiceInUse.RequestWearableAsync(wearableId, ct);

        public void AddWearablesToCatalog(WearableItem[] wearableItems) =>
            wearablesCatalogServiceInUse.AddWearablesToCatalog(wearableItems);

        public void RemoveWearablesFromCatalog(IEnumerable<string> wearableIds) =>
            wearablesCatalogServiceInUse.RemoveWearablesFromCatalog(wearableIds);

        public void RemoveWearablesInUse(IEnumerable<string> wearablesInUseToRemove) =>
            wearablesCatalogServiceInUse.RemoveWearablesInUse(wearablesInUseToRemove);

        public void EmbedWearables(IEnumerable<WearableItem> wearables) =>
            wearablesCatalogServiceInUse.EmbedWearables(wearables);

        public void Clear() =>
            wearablesCatalogServiceInUse.Clear();

        private void OnKernelConfigChanged(KernelConfigModel currentKernelConfig, KernelConfigModel previous)
        {
            KernelConfig.i.OnChange -= OnKernelConfigChanged;

            if (currentKernelConfig.usingUrlParamsForDebug)
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
        }
    }
}
