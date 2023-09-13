using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using DCL.Interface;
using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace DCLServices.CustomNftCollection
{
    public class WebInterfaceCustomNftCatalogBridge : MonoBehaviour, ICustomNftCollectionService
    {
        private UniTaskCompletionSource<string[]> getCollectionsTask;
        private UniTaskCompletionSource<string[]> getItemsTask;

        public static WebInterfaceCustomNftCatalogBridge GetOrCreate()
        {
            var bridgeObj = SceneReferences.i.bridgeGameObject;
            return bridgeObj.GetOrCreateComponent<WebInterfaceCustomNftCatalogBridge>();
        }

        public void Dispose()
        {
        }

        public void Initialize()
        {
        }

        public async UniTask<string[]> GetConfiguredCustomNftCollectionAsync(CancellationToken cancellationToken)
        {
            Debug.Log("WebInterfaceCustomNftCollectionBridge.GetConfiguredCustomNftCollectionAsync");

            try
            {
                if (getCollectionsTask != null)
                    return await getCollectionsTask.Task
                                                   .Timeout(TimeSpan.FromSeconds(30))
                                                   .AttachExternalCancellation(cancellationToken);

                getCollectionsTask = new UniTaskCompletionSource<string[]>();

                WebInterface.GetWithCollectionsUrlParam();

                return await getCollectionsTask.Task
                                                .Timeout(TimeSpan.FromSeconds(30))
                                                .AttachExternalCancellation(cancellationToken);
            }
            catch (Exception)
            {
                getCollectionsTask = null;
                throw;
            }
        }

        public async UniTask<string[]> GetConfiguredCustomNftItemsAsync(CancellationToken cancellationToken)
        {
            Debug.Log("WebInterfaceCustomNftCollectionBridge.GetConfiguredCustomNftItemsAsync");

            try
            {
                if (getItemsTask != null)
                    return await getItemsTask.Task
                                             .Timeout(TimeSpan.FromSeconds(30))
                                             .AttachExternalCancellation(cancellationToken);

                getItemsTask = new UniTaskCompletionSource<string[]>();

                WebInterface.GetWithItemsUrlParam();

                return await getItemsTask.Task
                                         .Timeout(TimeSpan.FromSeconds(30))
                                         .AttachExternalCancellation(cancellationToken);
            }
            catch (Exception)
            {
                getItemsTask = null;
                throw;
            }
        }

        [PublicAPI("Kernel response for GetParametrizedCustomNftCollectionAsync")]
        public void SetWithCollectionsParam(string json)
        {
            Debug.Log($"WebInterfaceCustomNftCollectionBridge.SetWithCollectionsParam: {json}");
            CollectionIdsPayload payload = JsonConvert.DeserializeObject<CollectionIdsPayload>(json);
            string[] collectionIds = payload.collectionIds.Where(s => !string.IsNullOrEmpty(s)).ToArray();

            Debug.Log(collectionIds);

            foreach (string collectionId in collectionIds)
                Debug.Log(collectionId);

            getCollectionsTask.TrySetResult(collectionIds);
            getCollectionsTask = null;
        }

        [PublicAPI("Kernel response for GetConfiguredCustomNftItemsAsync")]
        public void SetWithItemsParam(string json)
        {
            Debug.Log($"WebInterfaceCustomNftCollectionBridge.SetWithItemsParam: {json}");
            ItemIdsPayload payload = JsonConvert.DeserializeObject<ItemIdsPayload>(json);

            string[] itemIds = payload.itemIds.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            Debug.Log(itemIds);

            foreach (string itemId in itemIds)
                Debug.Log(itemId);

            getItemsTask.TrySetResult(itemIds);
            getItemsTask = null;
        }

        [Serializable]
        private struct CollectionIdsPayload
        {
            public string[] collectionIds;
        }

        [Serializable]
        private struct ItemIdsPayload
        {
            public string[] itemIds;
        }
    }
}
