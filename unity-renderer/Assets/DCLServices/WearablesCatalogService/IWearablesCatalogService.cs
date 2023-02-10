using Cysharp.Threading.Tasks;
using DCL;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DCLServices.WearablesCatalogService
{
    public interface IWearablesCatalogService : IService
    {
        BaseDictionary<string, WearableItem> WearablesCatalog { get; }

        UniTask<IReadOnlyList<WearableItem>> RequestOwnedWearablesAsync(string userId, int pageNumber, int pageSize, bool cleanCachedPages, CancellationToken ct);
        UniTask<IReadOnlyList<WearableItem>> RequestBaseWearablesAsync(CancellationToken ct);
        UniTask<IReadOnlyList<WearableItem>> RequestThirdPartyWearablesByCollectionAsync(string userId, string collectionId, int pageNumber, int pageSize, bool cleanCachedPages, CancellationToken ct);
        UniTask<WearableItem> RequestWearableAsync(string wearableId, CancellationToken ct);
        void AddWearablesToCatalog(IEnumerable<WearableItem> wearableItems);
        void RemoveWearablesFromCatalog(IEnumerable<string> wearableIds);
        void RemoveWearablesInUse(IEnumerable<string> wearablesInUseToRemove);
        [Obsolete("Will be removed in the future, when emotes are in the content server.")]
        void EmbedWearables(IEnumerable<WearableItem> wearables);
        void Clear();
        bool IsValidWearable(string wearableId);
    }
}
