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
        private UniTaskCompletionSource<IReadOnlyList<string>> getParametrizedTask;

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

        public async UniTask<IReadOnlyList<string>> GetParametrizedCustomNftCollectionAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (getParametrizedTask != null)
                    return await getParametrizedTask.Task.AttachExternalCancellation(cancellationToken);

                getParametrizedTask ??= new UniTaskCompletionSource<IReadOnlyList<string>>();

                WebInterface.GetWithCollectionsUrlParam();

                return await getParametrizedTask.Task
                                                .Timeout(TimeSpan.FromSeconds(30))
                                                .AttachExternalCancellation(cancellationToken);
            }
            catch (Exception)
            {
                getParametrizedTask = null;
                throw;
            }
        }

        [PublicAPI("Kernel response for GetParametrizedCustomNftCollectionAsync")]
        public void SetWithCollectionsParam(string json)
        {
            string[] collectionIds = JsonConvert.DeserializeObject<string[]>(json);
            getParametrizedTask.TrySetResult(collectionIds);
            getParametrizedTask = null;
        }
    }
}
