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
        private string[] items;
        private string[] collections;

        public static WebInterfaceCustomNftCatalogBridge GetOrCreate()
        {
            var bridgeObj = SceneReferences.i?.bridgeGameObject;

            return SceneReferences.i?.bridgeGameObject == null
                ? new GameObject("Bridge").AddComponent<WebInterfaceCustomNftCatalogBridge>()
                : bridgeObj.GetOrCreateComponent<WebInterfaceCustomNftCatalogBridge>();
        }

        public void Dispose() { }

        public void Initialize() { }

        public async UniTask<IReadOnlyList<string>> GetConfiguredCustomNftCollectionAsync(CancellationToken cancellationToken)
        {
            if (collections != null)
                return collections;

            WebInterface.GetWithCollectionsUrlParam();

            // Reworked into this approach. Using a UniTaskCompletionSource<IReadOnlyList<string>> emits an invalid string array
            // on WebGL. Seems like a IL2CPP issue, not sure. This workaround is the only one i found to make it work on all platforms
            await UniTask.WaitUntil(() => collections != null, cancellationToken: cancellationToken)
                         .Timeout(TimeSpan.FromSeconds(30));

            return collections;
        }

        public async UniTask<IReadOnlyList<string>> GetConfiguredCustomNftItemsAsync(CancellationToken cancellationToken)
        {
            if (items != null)
                return items;

            WebInterface.GetWithItemsUrlParam();

            // Reworked into this approach. Using a UniTaskCompletionSource<IReadOnlyList<string>> emits an invalid string array
            // on WebGL. Seems like a IL2CPP issue, not sure. This workaround is the only one i found to make it work on all platforms
            await UniTask.WaitUntil(() => items != null, cancellationToken: cancellationToken)
                         .Timeout(TimeSpan.FromSeconds(30));

            return items;
        }

        [PublicAPI("Kernel response for GetParametrizedCustomNftCollectionAsync")]
        public void SetWithCollectionsParam(string json)
        {
            CollectionIdsPayload payload = JsonConvert.DeserializeObject<CollectionIdsPayload>(json);
            string[] collectionIds = payload.collectionIds.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            collections = collectionIds;
        }

        [PublicAPI("Kernel response for GetConfiguredCustomNftItemsAsync")]
        public void SetWithItemsParam(string json)
        {
            ItemIdsPayload payload = JsonConvert.DeserializeObject<ItemIdsPayload>(json);

            string[] itemIds = payload.itemIds.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            items = itemIds;
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
