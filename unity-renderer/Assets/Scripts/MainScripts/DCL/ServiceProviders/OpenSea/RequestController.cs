using DCL;
using MainScripts.DCL.ServiceProviders.OpenSea.RequestHandlers;
using MainScripts.DCL.ServiceProviders.OpenSea.Requests;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Environment = DCL.Environment;

namespace MainScripts.DCL.ServiceProviders.OpenSea
{
    public class RequestController : IDisposable
    {
        private readonly KernelConfig kernelConfig;
        private const bool VERBOSE = false;

        private const string EDITOR_USER_AGENT =
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11_5) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36";

        private const string EDITOR_REFERRER = "https://play.decentraland.org";

        internal readonly RequestScheduler.RequestScheduler requestScheduler = new ();
        private readonly Dictionary<string, RequestBase<OpenSeaNftDto>> cacheAssetResponses = new ();
        private readonly Dictionary<string, RequestBase<OpenSeaManyNftDto>> cacheSeveralAssetsResponse = new ();

        public RequestController(KernelConfig kernelConfig)
        {
            this.kernelConfig = kernelConfig;
            requestScheduler.OnRequestReadyToSend += SendRequest;
        }

        public void Dispose() { requestScheduler.OnRequestReadyToSend -= SendRequest; }

        public RequestBase<OpenSeaNftDto> FetchNFT(string contractAddress, string tokenId)
        {
            if (cacheAssetResponses.TryGetValue(RequestAssetSingle.GetId(contractAddress, tokenId), out RequestBase<OpenSeaNftDto> request))
                return request;

            var newRequest = new RequestAssetSingle(contractAddress, tokenId);

            AddToCache(newRequest);

            newRequest.OnFail += OnRequestFailed;

            var requestHandler = new SingleAssetRequestHandler(newRequest, this, kernelConfig);
            requestScheduler.EnqueueRequest(requestHandler);

            return newRequest;
        }

        public RequestBase<OpenSeaManyNftDto> FetchOwnedNFT(string address)
        {
            if (cacheSeveralAssetsResponse.TryGetValue(RequestOwnedNFTs.GetId(address), out RequestBase<OpenSeaManyNftDto> request))
            {
                return request;
            }

            var newRequest = new RequestOwnedNFTs(address);

            AddToCache(newRequest);

            newRequest.OnFail += OnRequestFailed;

            var requestHandler = new OwnedNFTRequestHandler(newRequest, this, kernelConfig);
            requestScheduler.EnqueueRequest(requestHandler);

            return newRequest;
        }

        private void SendRequest(IRequestHandler requestHandler)
        {
            string url = requestHandler.GetUrl();

            if (VERBOSE)
                Debug.Log($"RequestController: Send Request: {url}");

            Dictionary<string, string> headers = new Dictionary<string, string>();

#if (UNITY_EDITOR) || (UNITY_STANDALONE)
            headers.Add("User-Agent", EDITOR_USER_AGENT);
            headers.Add("referrer", EDITOR_REFERRER);
#endif

            // NOTE: In this case, as this code is implementing a very specific retries system (including delays), we use our
            //              custom WebRequest system without retries (requestAttemps = 1) and let the current code to apply the retries.
            WebRequestAsyncOperation asyncOp = (WebRequestAsyncOperation) Environment.i.platform.webRequest.Get(
                url,
                requestAttemps: 1,
                disposeOnCompleted: true,
                headers: headers);

            asyncOp.completed += operation =>
            {
                UnityWebRequest unityWebRequest = operation.webRequest;
                bool shouldTryToRetry = true;
                string serverResponse = unityWebRequest.downloadHandler.text;

                if (VERBOSE)
                    Debug.Log($"RequestController: Request completed: success? {unityWebRequest.result == UnityWebRequest.Result.Success} {serverResponse}");

                if (unityWebRequest.result == UnityWebRequest.Result.Success)
                {
                    requestHandler.SetApiResponse(serverResponse,
                        () =>
                        {
                            if (VERBOSE)
                                Debug.Log($"RequestController: Request Succeeded: {url}");

                            shouldTryToRetry = false;
                        },
                        error =>
                        {
                            serverResponse = $"RequestController: Parse Request Error: {url}: {error}";

                            if (VERBOSE)
                                Debug.Log(serverResponse);
                        });
                }

                if (shouldTryToRetry)
                {
                    if (requestHandler.CanRetry())
                    {
                        requestHandler.Retry();
                    }
                    else
                    {
                        requestHandler.SetApiResponseError($"[{url}] {serverResponse}");
                    }
                }

                unityWebRequest.Dispose();
            };
        }

        private void OnRequestFailed(RequestBase<OpenSeaManyNftDto> request)
        {
            request.OnFail -= OnRequestFailed;

            RemoveFromCache(request);
        }

        private void OnRequestFailed(RequestBase<OpenSeaNftDto> request)
        {
            request.OnFail -= OnRequestFailed;

            RemoveFromCache(request);
        }

        private void AddToCache(RequestBase<OpenSeaNftDto> request)
        {
            if (cacheAssetResponses.ContainsKey(request.requestId))
                return;

            cacheAssetResponses.Add(request.requestId, request);
        }

        private void AddToCache(RequestBase<OpenSeaManyNftDto> request)
        {
            if (cacheSeveralAssetsResponse.ContainsKey(request.requestId))
                return;

            cacheSeveralAssetsResponse.Add(request.requestId, request);
        }

        private void RemoveFromCache(RequestBase<OpenSeaNftDto> request)
        {
            cacheAssetResponses.Remove(request.requestId);
        }

        private void RemoveFromCache(RequestBase<OpenSeaManyNftDto> request)
        {
            cacheSeveralAssetsResponse.Remove(request.requestId);
        }
    }
}
