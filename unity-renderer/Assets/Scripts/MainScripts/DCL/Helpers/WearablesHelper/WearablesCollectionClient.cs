using System;
using System.Collections.Generic;
using System.Linq;
using DCL.Helpers;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL.ABConverter
{
    public static class WearablesCollectionClient
    {
        private static WearableCollectionsAPIData.Collection[] wearableCollections;
        private static double wearablesCollectionDumpStartTime;

        public static string BuildWearableCollectionFetchingURL(string targetCollectionId) =>
            WearablesFetchingHelper.GetWearablesFetchURL() + "collectionId=" + targetCollectionId;

        public static List<WearableItem> GetBaseWearableCollections() =>
            GetWearableItems(BuildWearableCollectionFetchingURL(WearablesFetchingHelper.BASE_WEARABLES_COLLECTION_ID));

        /// <summary>
        /// Given a base-url to fetch wearables collections, returns a list of all the WearableItems
        /// </summary>
        /// <param name="url">base-url to fetch the wearables collections</param>
        /// <returns>A list of all the WearableItems found</returns>
        private static List<WearableItem> GetWearableItems(string url)
        {
            UnityWebRequest w = UnityWebRequest.Get(url);
            w.SendWebRequest();

            while (!w.isDone) { }

            if (!w.WebRequestSucceded())
            {
                Debug.LogError($"Request error! Wearable at '{url}' couldn't be fetched! -- {w.error}");
                return null;
            }

            var wearablesApiData = JsonConvert.DeserializeObject<WearablesAPIData>(w.downloadHandler.text);
            var resultList = wearablesApiData.GetWearableItems();

            // Since the wearables deployments response returns only a batch of elements, we need to fetch all the
            // batches sequentially
            if (!string.IsNullOrEmpty(wearablesApiData.pagination.next))
            {
                var nextPageResults = GetWearableItems(WearablesFetchingHelper.GetWearablesFetchURL() + wearablesApiData.pagination.next);
                resultList.AddRange(nextPageResults);
            }

            return resultList;
        }
    }
}
