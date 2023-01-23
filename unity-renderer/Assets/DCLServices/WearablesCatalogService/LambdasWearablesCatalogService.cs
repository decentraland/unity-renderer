using Cysharp.Threading.Tasks;
using DCL;
using DCLServices.Lambdas;
using System.Collections.Generic;
using System.Threading;

namespace DCLServices.WearablesCatalogService
{
    public class LambdasWearablesCatalogService : IWearablesCatalogService, ILambdaServiceConsumer<WearableResponse>
    {
        private const string WEARABLES_BY_ID_END_POINT = "collections/wearables?wearableId={wearableId}/";
        private const string WEARABLES_BY_COLLECTION_END_POINT = "collections/wearables?collectionId={collectionId}/";
        private const string WEARABLES_BY_OWNER_END_POINT = "nfts/wearables/{userId}/";
        private const int TIMEOUT = ILambdasService.DEFAULT_TIMEOUT;
        private const int ATTEMPTS_NUMBER = ILambdasService.DEFAULT_ATTEMPTS_NUMBER;

        private readonly BaseDictionary<string, WearableItem> wearablesCatalog;
        private Service<ILambdasService> lambdasService;
        private Dictionary<string, LambdaResponsePagePointer<WearableResponse>> getWearablesByIdPagePointer;
        private Dictionary<string, LambdaResponsePagePointer<WearableResponse>> getWearablesByCollectionPointer;
        private Dictionary<string, LambdaResponsePagePointer<WearableResponse>> getWearablesByOwnerPointer;

        public LambdasWearablesCatalogService(BaseDictionary<string, WearableItem> wearablesCatalog)
        {
            this.wearablesCatalog = wearablesCatalog;
        }

        public void Initialize() { }

        public void Dispose() { }

        public async UniTask<WearableItem> GetWearablesById(string wearableId, int pageNumber, int pageSize, CancellationToken ct)
        {
            if (!getWearablesByIdPagePointer.ContainsKey(wearableId))
            {
                getWearablesByIdPagePointer[wearableId] = new LambdaResponsePagePointer<WearableResponse>(
                    WEARABLES_BY_ID_END_POINT.Replace("{wearableId}", wearableId),
                    pageSize, ct, this);
            }

            var pageResponse = await getWearablesByIdPagePointer[wearableId].GetPageAsync(pageNumber, ct);

            return pageResponse.response.wearables.Count > 0 ? pageResponse.response.wearables[0] : null;
        }

        public async UniTask<WearableItem[]> GetWearablesByCollection(string collectionId, int pageNumber, int pageSize, CancellationToken ct)
        {
            if (!getWearablesByCollectionPointer.ContainsKey(collectionId))
            {
                getWearablesByCollectionPointer[collectionId] = new LambdaResponsePagePointer<WearableResponse>(
                    WEARABLES_BY_COLLECTION_END_POINT.Replace("{collectionId}", collectionId),
                    pageSize, ct, this);
            }

            var pageResponse = await getWearablesByCollectionPointer[collectionId].GetPageAsync(pageNumber, ct);

            return pageResponse.response.wearables.ToArray();
        }

        public async UniTask<WearableItem[]> GetWearablesByOwner(string userId, int pageNumber, int pageSize, CancellationToken ct)
        {
            if (!getWearablesByOwnerPointer.ContainsKey(userId))
            {
                getWearablesByOwnerPointer[userId] = new LambdaResponsePagePointer<WearableResponse>(
                    WEARABLES_BY_OWNER_END_POINT.Replace("{userId}", userId),
                    pageSize, ct, this);
            }

            var pageResponse = await getWearablesByOwnerPointer[userId].GetPageAsync(pageNumber, ct);

            return pageResponse.response.wearables.ToArray();
        }

        UniTask<(WearableResponse response, bool success)> ILambdaServiceConsumer<WearableResponse>.CreateRequest
            (string endPoint, int pageSize, int pageNumber, CancellationToken cancellationToken) =>
            lambdasService.Ref.Get<WearableResponse>(endPoint,
                TIMEOUT,
                ATTEMPTS_NUMBER,
                cancellationToken,
                LambdaPaginatedResponseHelper.GetPageSizeParam(pageSize), LambdaPaginatedResponseHelper.GetPageNumParam(pageNumber));
    }
}
