using System;
using System.Collections;
using System.Collections.Generic;
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
            if (VERBOSE) Debug.Log($"RequestController: RequestGroup created to request at {lastApiRequestTime}");
            return group;
        }

        void OnGroupClosed(OpenSeaRequestGroup group)
        {
            if (VERBOSE) Debug.Log("RequestController: RequestGroup closed");
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
                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {
                    if (OpenSeaRequestController.VERBOSE) Debug.Log($"RequestGroup: Request to OpenSea {url}");
                    yield return request.SendWebRequest();

                    AssetsResponse response = null;

                    if (!request.isNetworkError && !request.isHttpError)
                    {
                        response = Utils.FromJsonWithNulls<AssetsResponse>(request.downloadHandler.text);
                    }

                    if (OpenSeaRequestController.VERBOSE) Debug.Log($"RequestGroup: Request resolving {response != null} {request.error} {url}");
                    if (response != null)
                    {
                        shouldRetry = false;
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
                        shouldRetry = retryCount < REQUEST_RETRIES;
                        if (!shouldRetry)
                        {
                            using (var iterator = requests.GetEnumerator())
                            {
                                while (iterator.MoveNext())
                                {
                                    iterator.Current.Value.Resolve(request.error);
                                }
                            }
                        }
                        else
                        {
                            retryCount++;
                            if (OpenSeaRequestController.VERBOSE) Debug.Log($"RequestGroup: Request retrying {url}");
                            yield return new WaitForSeconds(GetRetryDelay());
                        }
                    }
                }
            } while (shouldRetry);
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
            if (fetchRoutine != null) CoroutineStarter.Stop(fetchRoutine);
            fetchRoutine = null;
            CloseGroup();
        }
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
            if (OpenSeaRequestController.VERBOSE) Debug.Log($"Request: created {this.ToString()}");
        }

        public void Resolve(AssetsResponse response)
        {
            AssetResponse asset = null;
            for (int i = 0; i < response.assets.Length; i++)
            {
                asset = response.assets[i];
                if (asset.token_id == tokenId && String.Equals(asset.asset_contract.address, assetContractAddress, StringComparison.OrdinalIgnoreCase))
                {
                    if (OpenSeaRequestController.VERBOSE) Debug.Log($"Request: resolved {this.ToString()}");
                    assetResponse = asset;
                    break;
                }
            }
            if (assetResponse == null)
            {
                error = $"asset {assetContractAddress}/{tokenId} not found in api response";
                if (OpenSeaRequestController.VERBOSE) Debug.Log($"Request: for {assetContractAddress}/{tokenId} not found {JsonUtility.ToJson(response)}");
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

        public override string ToString()
        {
            return $"asset_contract_addresses={assetContractAddress}&token_ids={tokenId}";
        }
    }

}
