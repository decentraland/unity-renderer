using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using DCL.Interface;
using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCLServices.CustomNftCollection
{
    public class WebInterfaceCustomNftCatalogBridge : MonoBehaviour, ICustomNftCollectionService
    {
        private UniTaskCompletionSource<IReadOnlyList<string>> getCollectionsTask;

        public static WebInterfaceCustomNftCatalogBridge GetOrCreate()
        {
            var bridgeObj = SceneReferences.i?.bridgeGameObject;

            return SceneReferences.i?.bridgeGameObject == null
                ? new GameObject("Bridge").AddComponent<WebInterfaceCustomNftCatalogBridge>()
                : bridgeObj.GetOrCreateComponent<WebInterfaceCustomNftCatalogBridge>();
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

                getCollectionsTask ??= new UniTaskCompletionSource<IReadOnlyList<string>>();

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

        [PublicAPI("Kernel response for GetParametrizedCustomNftCollectionAsync")]
        public void SetWithCollectionsParam(string json)
        {
            string[] collectionIds = JsonConvert.DeserializeObject<string[]>(json);
            getCollectionsTask.TrySetResult(collectionIds);
            getCollectionsTask = null;
        }
    }
}
