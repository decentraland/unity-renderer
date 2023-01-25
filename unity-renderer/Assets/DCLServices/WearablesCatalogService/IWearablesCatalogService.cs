using Cysharp.Threading.Tasks;
using DCL;
using System.Collections.Generic;
using System.Threading;

namespace DCLServices.WearablesCatalogService
{
    public interface IWearablesCatalogService : IService
    {
        BaseDictionary<string, WearableItem> WearablesCatalog { get; }

        UniTask<WearableItem> RequestWearableAsync(string wearableId, int pageNumber, int pageSize, CancellationToken ct);
        UniTask<WearableItem[]> RequestWearablesByOwnerAsync(string userId, int pageNumber, int pageSize, CancellationToken ct);
        UniTask<WearableItem[]> RequestWearablesByCollectionAsync(string collectionId, int pageNumber, int pageSize, CancellationToken ct);
        void AddWearablesToCatalog(WearableItem[] wearableItems);
        void RemoveWearablesFromCatalog(IEnumerable<string> wearableIds);
        void RemoveWearablesInUse(IEnumerable<string> wearablesInUseToRemove);
        void EmbedWearables(IEnumerable<WearableItem> wearables); //This temporary until the emotes are in the content server
        void Clear();
    }
}
