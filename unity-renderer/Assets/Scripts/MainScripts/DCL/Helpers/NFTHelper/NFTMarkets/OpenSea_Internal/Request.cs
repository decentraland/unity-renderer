using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable CS0162

namespace DCL.Helpers.NFT.Markets.OpenSea_Internal
{
    internal class OpenSeaRequestController
    {
        internal const bool VERBOSE = false;

        internal const float MIN_REQUEST_DELAY = 0.4f; // max ~2 requests per second

        List<OpenSeaRequestGroup> requestGroup = new List<OpenSeaRequestGroup>();
        float lastApiRequestTime = 0;

        public OpenSeaRequest AddRequest(string assetContractAddress, string tokenId)
        {
            OpenSeaRequestGroup group = null;
            for (int i = 0; i < requestGroup.Count; i++)
            {
                if (requestGroup[i].isOpen)
                {
                    group = requestGroup[i];
                    break;
                }
            }

            if (group == null)
            {
                group = CreateNewGroup();
            }
            return group.AddRequest(assetContractAddress, tokenId);
        }

        OpenSeaRequestGroup CreateNewGroup()
        {
            float delayRequest = IncrementApiRequestDelay();
            OpenSeaRequestGroup group = new OpenSeaRequestGroup(delayRequest, OnGroupClosed, IncrementApiRequestDelay);
            requestGroup.Add(group);
            if (VERBOSE)
                Debug.Log($"RequestController: RequestGroup created to request at {lastApiRequestTime}");
            return group;
        }

        void OnGroupClosed(OpenSeaRequestGroup group)
        {
            if (VERBOSE)
                Debug.Log("RequestController: RequestGroup closed");
            if (requestGroup.Contains(group))
            {
                requestGroup.Remove(group);
            }
        }

        internal float IncrementApiRequestDelay()
        {
            float delayRequest = MIN_REQUEST_DELAY;
            float timeSinceLastApiRequest = Time.unscaledTime - lastApiRequestTime;

            if (timeSinceLastApiRequest < 0)
            {
                delayRequest += Math.Abs(timeSinceLastApiRequest);
            }
            lastApiRequestTime = Time.unscaledTime + delayRequest;
            return delayRequest;
        }
    }

    class OpenSeaRequestGroup : IDisposable
    {
        const string API_URL_ASSETS = "https://api.opensea.io/api/v1/assets?";
        const float URL_PARAMS_MAX_LENGTH = 1085; // maxUrl(1279) - apiUrl(37) - longestPossibleRequest (78 tokenId + 42 contractAddress + 37 urlParams)
        const int REQUEST_RETRIES = 3;

        public bool isOpen { private set; get; }

        Dictionary<string, OpenSeaRequest> requests = new Dictionary<string, OpenSeaRequest>();
        string requestUrl = "";

        Coroutine fetchRoutine = null;

        Action<OpenSeaRequestGroup> onGroupClosed = null;
        Func<float> getRetryDelay = null;

        public OpenSeaRequestGroup(float delayRequest, Action<OpenSeaRequestGroup> onGroupClosed, Func<float> getRetryDelay)
        {
            isOpen = true;
            this.onGroupClosed = onGroupClosed;
            this.getRetryDelay = getRetryDelay;
            fetchRoutine = CoroutineStarter.Start(Fetch(delayRequest));
        }

        private OpenSeaRequestGroup(float delayRequest, IEnumerable<KeyValuePair<string, OpenSeaRequest>> existentRequests, Func<float> getRetryDelay)
        {
            isOpen = false;
            foreach ((string key, OpenSeaRequest value) in existentRequests)
            {
                requests.Add(key, value);
                requestUrl += value.ToString() + "&";
            }
            this.getRetryDelay = getRetryDelay;
            fetchRoutine = CoroutineStarter.Start(Fetch(delayRequest));
        }

        public OpenSeaRequest AddRequest(string assetContractAddress, string tokenId)
        {
            string nftId = $"{assetContractAddress}/{tokenId}";

            OpenSeaRequest request = null;
            if (requests.TryGetValue(nftId, out request))
            {
                return request;
            }

            request = new OpenSeaRequest(assetContractAddress, tokenId);
            requests.Add(nftId, request);
            requestUrl += request.ToString() + "&";

            if (requestUrl.Length >= URL_PARAMS_MAX_LENGTH)
            {
                CloseGroup();
            }

            return request;
        }

        IEnumerator Fetch(float delayRequest)
        {
            yield return new WaitForSeconds(delayRequest);
            CloseGroup();

            bool shouldRetry = false;
            int retryCount = 0;

            string url = API_URL_ASSETS + requestUrl;

            do
            {
                if (OpenSeaRequestController.VERBOSE)
                    Debug.Log($"RequestGroup: Request to OpenSea {url}");

                // NOTE(Santi): In this case, as this code is implementing a very specific retries system (including delays), we use our
                //              custom WebRequest system without retries (requestAttemps = 1) and let the current code to apply the retries.
                WebRequestAsyncOperation asyncOp = WebRequestController.i.Get(url: url, requestAttemps: 1, disposeOnCompleted: false);
                yield return asyncOp;

                AssetsResponse response = null;
                if (asyncOp.isSucceded)
                {
                    response = Utils.FromJsonWithNulls<AssetsResponse>(asyncOp.webRequest.downloadHandler.text);
                }

                if (OpenSeaRequestController.VERBOSE)
                    Debug.Log($"RequestGroup: Request resolving {response != null} {asyncOp.webRequest.error} {url}");

                if (response != null)
                {
                    shouldRetry = false;
                    //if we have one element in the group, is the one failing and we dont group it again
                    if (response.assets.Length != 0 || requests.Count <= 1)
                    {
                        using (var iterator = requests.GetEnumerator())
                        {
                            while (iterator.MoveNext())
                            {
                                iterator.Current.Value.Resolve(response);
                            }
                        }
                    }
                    else
                    {
                        //There are invalids NFTs to fetch, we split the request into 2 smaller groups to find the ofender
                        SplitGroup();
                    }

                    asyncOp.Dispose();
                }
                else
                {
                    shouldRetry = retryCount < REQUEST_RETRIES;
                    if (!shouldRetry)
                    {
                        using (var iterator = requests.GetEnumerator())
                        {
                            while (iterator.MoveNext())
                            {
                                iterator.Current.Value.Resolve(asyncOp.webRequest.error);
                            }
                        }

                        asyncOp.Dispose();
                    }
                    else
                    {
                        retryCount++;

                        if (OpenSeaRequestController.VERBOSE)
                            Debug.Log($"RequestGroup: Request retrying {url}");

                        asyncOp.Dispose();

                        yield return new WaitForSeconds(GetRetryDelay());
                    }
                }
            } while (shouldRetry);
        }

        void SplitGroup()
        {
            int counter = 0;
            var groups = requests.GroupBy(x => counter++ % 2);
            foreach (var group in groups)
            {
                new OpenSeaRequestGroup(getRetryDelay(), group, getRetryDelay );
            }
        }

        void CloseGroup()
        {
            isOpen = false;
            this.onGroupClosed?.Invoke(this);
            this.onGroupClosed = null;
        }

        float GetRetryDelay()
        {
            if (getRetryDelay != null)
                return getRetryDelay();
            return OpenSeaRequestController.MIN_REQUEST_DELAY;
        }

        public void Dispose()
        {
            if (fetchRoutine != null)
                CoroutineStarter.Stop(fetchRoutine);
            fetchRoutine = null;
            CloseGroup();
        }
    }

    internal class OpenSeaRequestAllNFTs
    {
        const string API_URL_ASSETS = "https://api.opensea.io/api/v1/assets?owner=";
        const int REQUEST_RETRIES = 3;

        string assetContractAddress;

        Coroutine fetchRoutine = null;

        Action<AssetsResponse> OnSuccess;
        Action<string> OnError;

        public OpenSeaRequestAllNFTs(string assetContractAddress, Action<AssetsResponse> OnSuccess, Action<string> OnError)
        {
            this.assetContractAddress = assetContractAddress;
            if (OpenSeaRequestController.VERBOSE)
                Debug.Log($"Request: created {this.ToString()}");

            this.OnSuccess = OnSuccess;
            this.OnError = OnError;

            if (fetchRoutine != null)
                CoroutineStarter.Stop(fetchRoutine);
            fetchRoutine = CoroutineStarter.Start(Fetch());
        }

        IEnumerator Fetch()
        {
            bool shouldRetry = true;
            int retryCount = 0;

            string url = API_URL_ASSETS + assetContractAddress;

            do
            {
                if (OpenSeaRequestController.VERBOSE)
                    Debug.Log($"RequestGroup: Request to OpenSea {url}");

                // NOTE(Santi): In this case, as this code is implementing a very specific retries system (including delays), we use our
                //              custom WebRequest system without retries (requestAttemps = 1) and let the current code to apply the retries.
                WebRequestAsyncOperation requestOp = WebRequestController.i.Get(url: url, requestAttemps: 1, disposeOnCompleted: false);
                yield return requestOp;

                AssetsResponse response = null;
                if (requestOp.isSucceded)
                {
                    response = Utils.FromJsonWithNulls<AssetsResponse>(requestOp.webRequest.downloadHandler.text);
                }

                if (OpenSeaRequestController.VERBOSE)
                    Debug.Log($"RequestGroup: Request resolving {response != null} {requestOp.webRequest.error} {url}");

                if (response != null)
                {
                    shouldRetry = false;
                    OnSuccess?.Invoke(response);
                    requestOp.Dispose();
                }
                else
                {
                    shouldRetry = retryCount < REQUEST_RETRIES;
                    if (!shouldRetry)
                    {
                        OnError?.Invoke("Error " + requestOp.webRequest.downloadHandler.text);
                        requestOp.Dispose();
                    }
                    else
                    {
                        retryCount++;

                        if (OpenSeaRequestController.VERBOSE)
                            Debug.Log($"RequestGroup: Request retrying {url}");

                        yield return new WaitForSeconds(GetRetryDelay());
                        requestOp.Dispose();
                    }
                }
            } while (shouldRetry);
        }

        float GetRetryDelay() { return OpenSeaRequestController.MIN_REQUEST_DELAY; }

        public override string ToString() { return $"asset_contract_addresses={assetContractAddress}"; }
    }

    internal class OpenSeaRequest
    {
        string assetContractAddress;
        string tokenId;
        bool resolved = false;

        AssetResponse assetResponse = null;
        string error = null;

        public OpenSeaRequest(string assetContractAddress, string tokenId)
        {
            this.assetContractAddress = assetContractAddress;
            this.tokenId = tokenId;
            if (OpenSeaRequestController.VERBOSE)
                Debug.Log($"Request: created {this.ToString()}");
        }

        public void Resolve(AssetsResponse response)
        {
            AssetResponse asset = null;
            for (int i = 0; i < response.assets.Length; i++)
            {
                asset = response.assets[i];
                if (asset.token_id == tokenId && String.Equals(asset.asset_contract.address, assetContractAddress, StringComparison.OrdinalIgnoreCase))
                {
                    if (OpenSeaRequestController.VERBOSE)
                        Debug.Log($"Request: resolved {this.ToString()}");
                    assetResponse = asset;
                    break;
                }
            }
            if (assetResponse == null)
            {
                error = $"asset {assetContractAddress}/{tokenId} not found in api response";
                if (OpenSeaRequestController.VERBOSE)
                    Debug.Log($"Request: for {assetContractAddress}/{tokenId} not found {JsonUtility.ToJson(response)}");
            }
            resolved = true;
        }

        public void Resolve(string error)
        {
            this.error = error;
            resolved = true;
        }

        public IEnumerator OnResolved(Action<AssetResponse> onSuccess, Action<string> onError)
        {
            yield return new WaitUntil(() => resolved);

            if (assetResponse != null)
            {
                onSuccess?.Invoke(assetResponse);
            }
            else
            {
                onError?.Invoke(error);
            }
        }

        public override string ToString() { return $"asset_contract_addresses={assetContractAddress}&token_ids={tokenId}"; }
    }

}