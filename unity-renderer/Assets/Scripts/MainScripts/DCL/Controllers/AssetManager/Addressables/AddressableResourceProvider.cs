using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace DCL.Providers
{
    public class AddressableResourceProvider<T> : IAddressableResourceProvider<T>
    {
        private bool areAddressablesInitialized;

        public AddressableResourceProvider()
        {
            Addressables.InitializeAsync().Completed += AddressablesInitiated;
        }

        public async UniTask<IList<T>> GetAddressablesList(string key, CancellationToken cancellationToken = default)
        {
            await UniTask.WaitUntil(() => areAddressablesInitialized, cancellationToken: cancellationToken);

            AsyncOperationHandle<IList<T>> handler = Addressables.LoadAssetsAsync<T>(key, null);
            await handler.Task;

            if (handler.Status.Equals(AsyncOperationStatus.Succeeded))
                return handler.Result;

            return null;
        }

        public async UniTask<T> GetAddressable(string key, CancellationToken cancellationToken = default)
        {
            await UniTask.WaitUntil(() => areAddressablesInitialized, cancellationToken: cancellationToken);

            try
            {
                AsyncOperationHandle<T> handler = Addressables.LoadAssetAsync<T>(key);
                await handler.WithCancellation(cancellationToken);
                return handler.Result;
            }
            catch (Exception e) { throw new Exception($"Addressable with {key} failed with message {e.Message}"); }
        }

        private void AddressablesInitiated(AsyncOperationHandle<IResourceLocator> obj)
        {
            Addressables.InitializeAsync().Completed -= AddressablesInitiated;
            areAddressablesInitialized = true;
        }
    }
}
