using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL.Helpers.NFT.Markets.OpenSea_Internal
{
    internal class BatchAssetsRequestHandler : IRequestHandler
    {
        const bool VERBOSE = false;
        const float BATCH_OPEN_TIME = 2;
        const float QUERY_MAX_LENGTH = 1085; // maxUrl(1279) - apiUrl(37) - longestPossibleRequest (78 tokenId + 42 contractAddress + 37 urlParams)

        public bool isOpen { private set; get; }

        SchedulableRequestHandler IRequestHandler.schedulableRequestHandler => schedulableHandler;

        private readonly List<RequestAssetInBatch> requests = new List<RequestAssetInBatch>();
        private readonly RequestController requestController;
        private readonly SchedulableRequestHandler schedulableHandler = new SchedulableRequestHandler();

        private string query = string.Empty;
        private int retryCount = 0;

        public BatchAssetsRequestHandler(RequestController requestController, bool autocloseBatch = true)
        {
            if (VERBOSE)
                Debug.Log($"BatchAssetsRequestHandler: ({GetHashCode()}) created");

            this.requestController = requestController;
            this.isOpen = true;

            if (autocloseBatch)
                CoroutineStarter.Start(BatchOpenTimeOut(BATCH_OPEN_TIME));
        }

        public void AddRequest(RequestAssetInBatch request)
        {
            requests.Add(request);
            query += $"asset_contract_addresses={request.contractAddress}&token_ids={request.tokenId}&";

            if (VERBOSE)
                Debug.Log($"BatchAssetsRequestHandler: ({GetHashCode()}) adding {request.contractAddress}/{request.tokenId}");

            if (query.Length >= QUERY_MAX_LENGTH)
            {
                CloseBatch();
            }
        }

        void CloseBatch()
        {
            if (VERBOSE)
                Debug.Log($"BatchAssetsRequestHandler: ({GetHashCode()}) closed {query}");

            isOpen = false;

            if (string.IsNullOrEmpty(query))
                return;

            schedulableHandler.SetReadyToBeScheduled(this);
        }

        IEnumerator BatchOpenTimeOut(float openTime)
        {
            yield return WaitForSecondsCache.Get(openTime);
            CloseBatch();
        }

        void TryRetrySingleAsset()
        {
            if (VERBOSE)
                Debug.Log($"BatchAssetsRequestHandler: ({GetHashCode()}) retry single asset {query}");

            requestController.requestScheduler.EnqueueRequest(this);
            retryCount ++;
        }

        void TryRetryBatchAssets()
        {
            if (VERBOSE)
                Debug.Log($"BatchAssetsRequestHandler: ({GetHashCode()}) retry splitting batch {query}");

            // Split requests in new batches and retry
            int counter = 0;
            var batches = requests.GroupBy(x => counter++ % 2);

            foreach (var group in batches)
            {
                RetryBatch(group.ToList());
            }
        }

        void RetryBatch(List<RequestAssetInBatch> batchedRequests)
        {
            var newHandler = new BatchAssetsRequestHandler(requestController, false);
            for (int i = 0; i < batchedRequests.Count; i++)
            {
                newHandler.AddRequest(batchedRequests[i]);
            }

            requestController.requestScheduler.EnqueueRequest(newHandler);
            newHandler.schedulableHandler.SetReadyToBeScheduled(newHandler);
        }

        string IRequestHandler.GetUrl() { return $"{Constants.MULTIPLE_ASSETS_URL}?{query}"; }

        void IRequestHandler.SetApiResponse(string responseJson, Action onSuccess, Action<string> onError)
        {
            AssetsResponse response = null;

            try
            {
                response = Utils.FromJsonWithNulls<AssetsResponse>(responseJson);
            }
            catch (Exception e)
            {
                onError?.Invoke(e.Message);
            }

            if (response == null)
                return;

            RequestAssetInBatch request = null;
            AssetResponse asset = null;

            int batchCount = requests.Count;
            for (int i = batchCount - 1; i >= 0; i--)
            {
                request = requests[i];
                for (int j = 0; j < response.assets.Length; j++)
                {
                    asset = response.assets[j];
                    bool isMatch = asset.token_id == request.tokenId
                                   && String.Equals(asset.asset_contract.address, request.contractAddress, StringComparison.OrdinalIgnoreCase);

                    if (isMatch)
                    {
                        if (VERBOSE)
                            Debug.Log($"BatchAssetsRequestHandler: ({GetHashCode()}) resolved {request.contractAddress}/{request.tokenId}");

                        request.Resolve(asset);
                        requests.RemoveAt(i);
                        break;
                    }
                }
            }

            // Retry possible unresolved requests
            if (requests.Count > 0)
            {
                RetryBatch(requests);
            }

            onSuccess?.Invoke();
        }

        bool IRequestHandler.CanRetry()
        {
            if (requests.Count == 1)
                return retryCount < Constants.REQUESTS_RETRY_ATTEMPS;
            return true;
        }

        void IRequestHandler.Retry()
        {
            if (requests.Count == 0)
                return;

            if (requests.Count == 1)
            {
                TryRetrySingleAsset();
            }
            else
            {
                TryRetryBatchAssets();
            }
        }

        void IRequestHandler.SetApiResponseError(string error)
        {
            if (VERBOSE)
                Debug.Log($"BatchAssetsRequestHandler: ({GetHashCode()}) rejecting {error} {query}");

            for (int i = 0; i < requests.Count; i++)
            {
                requests[i].Reject(error);
            }
        }
    }
}