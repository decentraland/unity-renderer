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
        private IWearablesCatalogService wearablesCatalogServiceInUse;

        public WearablesCatalogService(
            IWearablesCatalogService lambdasWearablesCatalogService,
            IWearablesCatalogService webInterfaceWearablesCatalogService)
        {
            this.lambdasWearablesCatalogService = lambdasWearablesCatalogService;
            this.webInterfaceWearablesCatalogService = webInterfaceWearablesCatalogService;
        }

        public void Initialize()
        {
            KernelConfig.i.EnsureConfigInitialized().Then(config => OnKernelConfigChanged(config, null));
            KernelConfig.i.OnChange += OnKernelConfigChanged;
        }

        private void OnKernelConfigChanged(KernelConfigModel currentKernelConfig, KernelConfigModel previous)
        {
            KernelConfig.i.OnChange -= OnKernelConfigChanged;

            if (currentKernelConfig.usingUrlParamsForDebug)
            {
                wearablesCatalogServiceInUse = webInterfaceWearablesCatalogService;
                lambdasWearablesCatalogService.Dispose();
            }
            else
            {
                wearablesCatalogServiceInUse = lambdasWearablesCatalogService;
                webInterfaceWearablesCatalogService.Dispose();
            }
        }

        public void Dispose()
        {
            KernelConfig.i.OnChange -= OnKernelConfigChanged;
        }

        public UniTask<WearableItem> RequestWearable(string wearableId, int pageNumber, int pageSize, CancellationToken ct) =>
            wearablesCatalogServiceInUse.RequestWearable(wearableId, pageNumber, pageSize, ct);

        public UniTask<WearableItem[]> RequestWearablesByOwner(string userId, int pageNumber, int pageSize, CancellationToken ct) =>
            wearablesCatalogServiceInUse.RequestWearablesByOwner(userId, pageNumber, pageSize, ct);

        public UniTask<WearableItem[]> RequestWearablesByCollection(string collectionId, int pageNumber, int pageSize, CancellationToken ct) =>
            wearablesCatalogServiceInUse.RequestWearablesByCollection(collectionId, pageNumber, pageSize, ct);

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
    }
}
