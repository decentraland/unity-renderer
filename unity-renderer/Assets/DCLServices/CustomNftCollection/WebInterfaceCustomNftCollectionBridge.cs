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
        private UniTaskCompletionSource<IReadOnlyList<string>> getCollectionsTask;
        private UniTaskCompletionSource<IReadOnlyList<string>> getItemsTask;
        private string[] items;
        private string[] collections;

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

        public async UniTask<IReadOnlyList<string>> GetConfiguredCustomNftCollectionAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (collections != null)
                    return collections;

                if (getCollectionsTask != null)
                    return await getCollectionsTask.Task
                                                   .Timeout(TimeSpan.FromSeconds(30))
                                                   .AttachExternalCancellation(cancellationToken);

                getCollectionsTask = new UniTaskCompletionSource<IReadOnlyList<string>>();

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

        public async UniTask<IReadOnlyList<string>> GetConfiguredCustomNftItemsAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (items != null)
                    return items;

                if (getItemsTask != null)
                    return await getItemsTask.Task
                                             .Timeout(TimeSpan.FromSeconds(30))
                                             .AttachExternalCancellation(cancellationToken);

                getItemsTask = new UniTaskCompletionSource<IReadOnlyList<string>>();

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
            CollectionIdsPayload payload = JsonConvert.DeserializeObject<CollectionIdsPayload>(json);
            string[] collectionIds = payload.collectionIds.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            collections = collectionIds;
            getCollectionsTask.TrySetResult(collectionIds);
            getCollectionsTask = null;
        }

        [PublicAPI("Kernel response for GetConfiguredCustomNftItemsAsync")]
        public void SetWithItemsParam(string json)
        {
            ItemIdsPayload payload = JsonConvert.DeserializeObject<ItemIdsPayload>(json);

            string[] itemIds = payload.itemIds.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            items = itemIds;
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
