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
                if (getCollectionsTask != null)
                    return await getCollectionsTask.Task.AttachExternalCancellation(cancellationToken);

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
                if (getItemsTask != null)
                    return await getItemsTask.Task.AttachExternalCancellation(cancellationToken);

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
            getCollectionsTask.TrySetResult(payload.collectionIds.Where(s => !string.IsNullOrEmpty(s)).ToArray());
            getCollectionsTask = null;
        }

        [PublicAPI("Kernel response for GetConfiguredCustomNftItemsAsync")]
        public void SetWithItemsParam(string json)
        {
            ItemIdsPayload payload = JsonConvert.DeserializeObject<ItemIdsPayload>(json);
            getItemsTask.TrySetResult(payload.itemIds.Where(s => !string.IsNullOrEmpty(s)).ToArray());
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
