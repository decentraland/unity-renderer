using DCL.Helpers;
using MainScripts.DCL.ServiceProviders.OpenSea.Requests;
using MainScripts.DCL.ServiceProviders.OpenSea.RequestScheduler;
using System;
using UnityEngine;

namespace MainScripts.DCL.ServiceProviders.OpenSea.RequestHandlers
{
    internal class SingleAssetRequestHandler : IRequestHandler
    {
        const bool VERBOSE = false;

        private readonly SchedulableRequestHandler schedulableHandler = new SchedulableRequestHandler();
        private readonly RequestAssetSingle request;
        private readonly RequestController requestController;

        private int retryCount = 0;

        SchedulableRequestHandler IRequestHandler.schedulableRequestHandler => schedulableHandler;

        public SingleAssetRequestHandler(RequestAssetSingle request, RequestController requestController)
        {
            if (VERBOSE)
                Debug.Log($"SingleAssetRequestHandler: ({GetHashCode()}) {request.requestId} created");

            this.request = request;
            this.requestController = requestController;
            schedulableHandler.SetReadyToBeScheduled(this);
        }

        string IRequestHandler.GetUrl() =>
            OpenSeaAPI.GetSingleAssetUrl(request.contractAddress, request.tokenId);

        void IRequestHandler.SetApiResponse(string responseJson, Action onSuccess, Action<string> onError)
        {
            OpenSeaNftDto response = null;

            try
            {
                response = Utils.FromJsonWithNulls<OpenSeaNftDto>(responseJson);
            }
            catch (Exception e)
            {
                onError?.Invoke(e.Message);
            }

            if (response == null)
                return;

            if (VERBOSE)
                Debug.Log($"SingleAssetRequestHandler: ({GetHashCode()}) {request.requestId} resolved");

            request.Resolve(response);
            onSuccess?.Invoke();
        }

        void IRequestHandler.SetApiResponseError(string error)
        {
            if (VERBOSE)
                Debug.Log($"SingleAssetRequestHandler: ({GetHashCode()}) {request.requestId} rejected {error}");

            request.Reject(error);
        }

        bool IRequestHandler.CanRetry() =>
            retryCount < OpenSeaAPI.REQUESTS_RETRY_ATTEMPS;

        void IRequestHandler.Retry()
        {
            if (VERBOSE)
                Debug.Log($"SingleAssetRequestHandler: ({GetHashCode()}) {request.requestId} retry {retryCount}");

            requestController.requestScheduler.EnqueueRequest(this);
            retryCount ++;
        }
    }
}
