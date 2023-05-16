using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;
using Collection = WearableCollectionsAPIData.Collection;

namespace DCL.Helpers
{
    public static class WearablesFetchingHelper
    {
        // TODO: change fetching logic to allow for auto-pagination
        // The https://nft-api.decentraland.org/v1/ endpoint doesn't fetch L1 wearables right now, if those need to be re-converted we should use that old endpoint again and change the WearablesAPIData structure again for that response.
        public const string BASE_FETCH_URL = "https://peer.decentraland.org/lambdas/collections";
        public const string COLLECTIONS_FETCH_PARAMS = "?sortBy=newest&first=1000";
        public const string WEARABLES_FETCH_PARAMS = "/wearables?";
        public const string BASE_WEARABLES_COLLECTION_ID = "urn:decentraland:off-chain:base-avatars";
        public const string THIRD_PARTY_COLLECTIONS_FETCH_URL = "third-party-integrations";
        private static Collection[] collections;

        private static IEnumerator EnsureCollectionsData()
        {
            if (collections?.Length > 0)
                yield break;

            yield return Environment.i.platform.webRequest.Get(
                url: GetCollectionsFetchURL(),
                downloadHandler: new DownloadHandlerBuffer(),
                timeout: 60,
                disposeOnCompleted: false,
                OnFail: (webRequest) =>
                {
                    Debug.LogWarning($"Request error! collections couldn't be fetched! -- {webRequest.webRequest.error}");
                },
                OnSuccess: (webRequest) =>
                {
                    var collectionsApiData = JsonUtility.FromJson<WearableCollectionsAPIData>(webRequest.webRequest.downloadHandler.text);
                    collections = collectionsApiData.data;
                });
        }

        public static string GetCollectionsFetchURL()
        {
            return $"{BASE_FETCH_URL}{COLLECTIONS_FETCH_PARAMS}";
        }

        public static string GetWearablesFetchURL()
        {
            return $"{BASE_FETCH_URL}{WEARABLES_FETCH_PARAMS}";
        }

        /// <summary>
        /// Fetches base collection ids and adds them to the provided ids list
        /// </summary>
        /// <param name="finalCollectionIdsList">A strings list that will be filled with the base collection ids</param>
        public static IEnumerator GetBaseCollections(List<string> finalCollectionIdsList)
        {
            yield return EnsureCollectionsData();

            finalCollectionIdsList.Add( BASE_WEARABLES_COLLECTION_ID );
        }

        public static IEnumerator GetRandomCollections(int amount, List<string> finalCollectionIdsList)
        {
            yield return EnsureCollectionsData();

            List<int> randomizedIndices = new List<int>();
            int randomIndex;
            bool addedBaseWearablesCollection = false;

            for (int i = 0; i < amount; i++)
            {
                randomIndex = Random.Range(0, collections.Length);

                while (randomizedIndices.Contains(randomIndex))
                {
                    randomIndex = Random.Range(0, collections.Length);
                }

                if (collections[randomIndex].urn == BASE_WEARABLES_COLLECTION_ID)
                    addedBaseWearablesCollection = true;

                finalCollectionIdsList.Add(collections[randomIndex].urn);
                randomizedIndices.Add(randomIndex);
            }
        }

        /// <summary>
        /// Given a base url for fetching wearables, this method recursively downloads all the 'pages' responded by the server
        /// and populates the global Catalogue with those wearables.
        /// </summary>
        /// <param name="url">The API url to fetch the list of wearables</param>
        /// <param name="finalWearableItemsList">A WearableItems list that will be filled with the fetched wearables</param>
        public static IEnumerator GetWearableItems(string url, List<WearableItem> finalWearableItemsList)
        {
            string nextPageParams = null;

            yield return Environment.i.platform.webRequest.Get(
                url: url,
                downloadHandler: new DownloadHandlerBuffer(),
                timeout: 60,
                disposeOnCompleted: false,
                OnFail: (webRequest) =>
                {
                    Debug.LogWarning($"Request error! wearables couldn't be fetched! -- {webRequest.webRequest.error}");
                },
                OnSuccess: (webRequest) =>
                {
                    var wearablesApiData = JsonConvert.DeserializeObject<WearablesAPIData>(webRequest.webRequest.downloadHandler.text);
                    var wearableItemsList = wearablesApiData.GetWearableItems();
                    finalWearableItemsList.AddRange(wearableItemsList);

                    nextPageParams = wearablesApiData.pagination.next;
                });

            if (!string.IsNullOrEmpty(nextPageParams))
            {
                // Since the wearables deployments response returns only a batch of elements, we need to fetch all the
                // batches sequentially
                yield return GetWearableItems(
                    GetWearablesFetchURL() + $"{nextPageParams}",
                    finalWearableItemsList);
            }
        }

        [Obsolete("Deprecated. Use IWearablesCatalogService.GetAllThirdPartyCollectionsAsync instead")]
        public static Promise<Collection[]> GetThirdPartyCollections()
        {
            Promise<Collection[]> promiseResult = new Promise<Collection[]>();

            Environment.i.platform.webRequest.Get(
                url: $"{Environment.i.platform.serviceProviders.catalyst.lambdasUrl}/{THIRD_PARTY_COLLECTIONS_FETCH_URL}",
                downloadHandler: new DownloadHandlerBuffer(),
                timeout: 60,
                OnFail: (webRequest) =>
                {
                    promiseResult.Reject($"Request error! third party collections couldn't be fetched! -- {webRequest.webRequest.error}");
                },
                OnSuccess: (webRequest) =>
                {
                    var collectionsApiData = JsonUtility.FromJson<WearableCollectionsAPIData>(webRequest.webRequest.downloadHandler.text);
                    promiseResult.Resolve(collectionsApiData.data);
                });

            return promiseResult;
        }
    }
}
