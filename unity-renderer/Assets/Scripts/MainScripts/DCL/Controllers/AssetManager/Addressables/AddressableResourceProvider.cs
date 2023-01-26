using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace DCL.Providers
{
    public class AddressableResourceProvider : IAddressableResourceProvider
    {
        public async UniTask<IList<T>> GetAddressablesList<T>(string key, CancellationToken cancellationToken = default)
        {
            //This function does nothing if initialization has already occurred
            await Addressables.InitializeAsync().WithCancellation(cancellationToken);

            AsyncOperationHandle<IList<T>> request = Addressables.LoadAssetsAsync<T>(key, null);
            await request.WithCancellation(cancellationToken);
            return request.Result;
        }

        public async UniTask<T> GetAddressable<T>(string key, CancellationToken cancellationToken = default)
        {
            //This function does nothing if initialization has already occurred
            await Addressables.InitializeAsync().WithCancellation(cancellationToken);

            AsyncOperationHandle<T> request = Addressables.LoadAssetAsync<T>(key);
            await request.WithCancellation(cancellationToken);
            return request.Result;
        }

        public void Dispose() { }

        public void Initialize() { }
    }
}
