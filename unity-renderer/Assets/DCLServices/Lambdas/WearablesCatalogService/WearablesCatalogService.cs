using Cysharp.Threading.Tasks;
using DCL;
using System.Threading;

namespace DCLServices.Lambdas.WearablesCatalogService
{
    public class WearablesCatalogService : IWearablesCatalogService, ILambdaServiceConsumer<WearableResponse>
    {
        internal const string WEARABLES_BY_ID_END_POINT = "collections/wearables?wearableId={wearableId}/";
        internal const string WEARABLES_BY_COLLECTION_END_POINT = "collections/wearables?collectionId={collectionId}/";
        internal const string WEARABLES_BY_OWNER_END_POINT = "nfts/wearables/{userId}/";
        internal const int TIMEOUT = ILambdasService.DEFAULT_TIMEOUT;
        internal const int ATTEMPTS_NUMBER = ILambdasService.DEFAULT_ATTEMPTS_NUMBER;

        private Service<ILambdasService> lambdasService;

        public LambdaResponsePagePointer<WearableResponse> GetWearablesByIdPaginationPointer(string wearableId, int pageSize, CancellationToken ct) =>
            new(WEARABLES_BY_ID_END_POINT.Replace("{wearableId}", wearableId), pageSize, ct, this);

        public LambdaResponsePagePointer<WearableResponse> GetWearablesByCollectionPaginationPointer(string collectionId, int pageSize, CancellationToken ct) =>
            new(WEARABLES_BY_COLLECTION_END_POINT.Replace("{collectionId}", collectionId), pageSize, ct, this);

        public LambdaResponsePagePointer<WearableResponse> GetWearablesByOwnerPaginationPointer(string address, int pageSize, CancellationToken ct) =>
            new(WEARABLES_BY_OWNER_END_POINT.Replace("{userId}", address), pageSize, ct, this);

        public LambdaResponsePagePointer<WearableResponse> GetThirdPartyWearablesPaginationPointer(string userId, string collectionId, int pageSize, CancellationToken ct) =>
            throw new System.NotImplementedException();

        UniTask<(WearableResponse response, bool success)> ILambdaServiceConsumer<WearableResponse>.CreateRequest
            (string endPoint, int pageSize, int pageNumber, CancellationToken cancellationToken) =>
            lambdasService.Ref.Get<WearableResponse>(endPoint,
                TIMEOUT,
                ATTEMPTS_NUMBER,
                cancellationToken,
                LambdaPaginatedResponseHelper.GetPageSizeParam(pageSize), LambdaPaginatedResponseHelper.GetPageNumParam(pageNumber));

        public void Initialize() { }

        public void Dispose() { }
    }
}
