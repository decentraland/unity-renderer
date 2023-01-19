using DCL;
using System.Threading;

namespace DCLServices.Lambdas.WearablesCatalogService
{
    public interface IWearablesCatalogService : IService
    {
        LambdaResponsePagePointer<WearableResponse> GetWearablesByIdPaginationPointer(string wearableId, int pageSize, CancellationToken ct);
        LambdaResponsePagePointer<WearableResponse> GetWearablesByCollectionPaginationPointer(string collectionId, int pageSize, CancellationToken ct);
        LambdaResponsePagePointer<WearableResponse> GetWearablesByOwnerPaginationPointer(string userId, int pageSize, CancellationToken ct);
        LambdaResponsePagePointer<WearableResponse> GetThirdPartyWearablesPaginationPointer(string userId, string collectionId, int pageSize, CancellationToken ct);
    }
}
