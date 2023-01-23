using Cysharp.Threading.Tasks;
using DCL;
using System.Threading;

namespace DCLServices.WearablesCatalogService
{
    public interface IWearablesCatalogService : IService
    {
        UniTask<WearableItem> GetWearablesById(string wearableId, int pageNumber, int pageSize, CancellationToken ct);
        UniTask<WearableItem[]> GetWearablesByCollection(string collectionId, int pageNumber, int pageSize, CancellationToken ct);
        UniTask<WearableItem[]> GetWearablesByOwner(string userId, int pageNumber, int pageSize, CancellationToken ct);
    }
}
