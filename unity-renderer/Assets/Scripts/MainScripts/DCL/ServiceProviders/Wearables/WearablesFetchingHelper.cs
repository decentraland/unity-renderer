using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;
using Collection = WearableCollectionsAPIData.Collection;

namespace DCL.Helpers
{
    public static class WearablesFetchingHelper
    {
        public const string WEARABLES_FETCH_URL = "https://peer.decentraland.org/lambdas/collections/wearables?";
        private const string COLLECTIONS_FETCH_URL = "https://peer.decentraland.org/lambdas/collections";
        private const string BASE_WEARABLES_COLLECTION_ID = "urn:decentraland:off-chain:base-avatars";

        private static Collection[] collections;
        private static IEnumerator EnsureCollectionsData()
        {
            if (collections?.Length > 0)
                yield break;

            yield return Environment.i.platform.webRequest.Get(
                url: COLLECTIONS_FETCH_URL,
                downloadHandler: new DownloadHandlerBuffer(),
                timeout: 5000,
                disposeOnCompleted: false,
                OnFail: (webRequest) =>
                {
                    Debug.LogWarning($"Request error! collections couldn't be fetched! -- {webRequest.error}");
                },
                OnSuccess: (webRequest) =>
                {
                    var collectionsApiData = JsonUtility.FromJson<WearableCollectionsAPIData>(webRequest.downloadHandler.text);
                    collections = collectionsApiData.collections;
                });
        }

        /// <summary>
        /// Fetches all the existent collection ids and triggers a randomized selection to be loaded
        /// </summary>
        /// <param name="finalCollectionIdsList">A strings list that will be filled with the randomized collection ids</param>
        public static IEnumerator GetRandomCollections(int amount, bool ensureBaseWearables, List<string> finalCollectionIdsList)
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

                if (ensureBaseWearables && collections[randomIndex].id == BASE_WEARABLES_COLLECTION_ID)
                    addedBaseWearablesCollection = true;

                finalCollectionIdsList.Add(collections[randomIndex].id);
                randomizedIndices.Add(randomIndex);
            }

            // We add the base wearables collection to make sure we have at least 1 of each avatar body-part
            if (!addedBaseWearablesCollection)
                finalCollectionIdsList[0] = BASE_WEARABLES_COLLECTION_ID;
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
                timeout: 5000,
                disposeOnCompleted: false,
                OnFail: (webRequest) =>
                {
                    Debug.LogWarning($"Request error! wearables couldn't be fetched! -- {webRequest.error}");
                },
                OnSuccess: (webRequest) =>
                {
                    var wearablesApiData = JsonUtility.FromJson<WearablesAPIData>(webRequest.downloadHandler.text);
                    var wearableItemsList = wearablesApiData.GetWearableItems();
                    finalWearableItemsList.AddRange(wearableItemsList);

                    nextPageParams = wearablesApiData.pagination.next;
                });

            if (!string.IsNullOrEmpty(nextPageParams))
            {
                // Since the wearables deployments response returns only a batch of elements, we need to fetch all the
                // batches sequentially
                yield return GetWearableItems(WEARABLES_FETCH_URL + nextPageParams, finalWearableItemsList);
            }
        }
    }
}