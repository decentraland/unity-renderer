using Cysharp.Threading.Tasks;
using DCL;
using System.Collections.Generic;
using System.Threading;

namespace DCLServices.WearablesCatalogService
{
    public interface IWearablesCatalogService : IService
    {
        BaseDictionary<string, WearableItem> WearablesCatalog { get; }

        UniTask<IReadOnlyList<WearableItem>> RequestOwnedWearablesAsync(string userId, int pageNumber, int pageSize, CancellationToken ct);
        UniTask<IReadOnlyList<WearableItem>> RequestBaseWearablesAsync(CancellationToken ct);
        UniTask<IReadOnlyList<WearableItem>> RequestThirdPartyWearablesByCollectionAsync(string userId, string collectionId, CancellationToken ct);
        UniTask<IReadOnlyList<WearableItem>> RequestWearablesAsync(string[] wearableIds, CancellationToken ct);
        UniTask<WearableItem> RequestWearableAsync(string wearableId, CancellationToken ct);
        void AddWearablesToCatalog(IReadOnlyList<WearableItem> wearableItems);
        void RemoveWearablesFromCatalog(IEnumerable<string> wearableIds);
        void RemoveWearablesInUse(IEnumerable<string> wearablesInUseToRemove);
        void EmbedWearables(IEnumerable<WearableItem> wearables); // This temporary until the emotes are in the content server
        void Clear();
    }
}
