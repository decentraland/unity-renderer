using Cysharp.Threading.Tasks;
using System.Threading;

namespace DCLServices.WearablesCatalogService
{
    public class WearablesCatalogService : IWearablesCatalogService
    {
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

            if (!currentKernelConfig.usingUrlParamsForDebug)
            {
                wearablesCatalogServiceInUse = lambdasWearablesCatalogService;
                webInterfaceWearablesCatalogService.Dispose();
            }
            else
            {
                wearablesCatalogServiceInUse = webInterfaceWearablesCatalogService;
                lambdasWearablesCatalogService.Dispose();
            }
        }

        public void Dispose() =>
            KernelConfig.i.OnChange -= OnKernelConfigChanged;

        public UniTask<WearableItem> GetWearablesById(string wearableId, int pageNumber, int pageSize, CancellationToken ct) =>
            wearablesCatalogServiceInUse.GetWearablesById(wearableId, pageNumber, pageSize, ct);

        public UniTask<WearableItem[]> GetWearablesByCollection(string collectionId, int pageNumber, int pageSize, CancellationToken ct) =>
            wearablesCatalogServiceInUse.GetWearablesByCollection(collectionId, pageNumber, pageSize, ct);

        public UniTask<WearableItem[]> GetWearablesByOwner(string userId, int pageNumber, int pageSize, CancellationToken ct) =>
            wearablesCatalogServiceInUse.GetWearablesByOwner(userId, pageNumber, pageSize, ct);
    }
}
