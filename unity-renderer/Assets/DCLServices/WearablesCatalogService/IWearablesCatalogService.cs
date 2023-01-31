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

        UniTask<IReadOnlyList<WearableItem>> RequestOwnedWearablesAsync(string userId, int pageNumber, int pageSize, CancellationToken ct);
        UniTask<IReadOnlyList<WearableItem>> RequestBaseWearablesAsync(CancellationToken ct);
        UniTask<IReadOnlyList<WearableItem>> RequestThirdPartyWearablesByCollectionAsync(string userId, string collectionId, int pageNumber, int pageSize, CancellationToken ct);
        UniTask<IReadOnlyList<WearableItem>> RequestWearablesAsync(IReadOnlyList<string> wearableIds, CancellationToken ct);
        UniTask<WearableItem> RequestWearableAsync(string wearableId, CancellationToken ct);
        void AddWearablesToCatalog(IReadOnlyList<WearableItem> wearableItems);
        void RemoveWearablesFromCatalog(IReadOnlyList<string> wearableIds);
        void RemoveWearablesInUse(IReadOnlyList<string> wearablesInUseToRemove);
        [Obsolete("Will be removed in the future, when emotes are in the content server.")]
        void EmbedWearables(IReadOnlyList<WearableItem> wearables);
        void Clear();
    }
}
