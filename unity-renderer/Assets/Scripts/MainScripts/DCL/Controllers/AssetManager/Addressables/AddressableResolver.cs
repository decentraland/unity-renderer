using Cysharp.Threading.Tasks;
using MainScripts.DCL.Controllers.AssetManager;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace DCL.Providers
{
    public class AddressableResolver : AssetResolverBase, IAddressableResolver
    {
        private bool areAddressablesInitialized;

        public AddressableResolver(DataStore_FeatureFlag featureFlags) : base(featureFlags)
        {
            Addressables.InitializeAsync().Completed += AddressablesInitiated;
        }

        public async UniTask<IList<T>> GetAddressablesListByLabel<T>(string label, CancellationToken cancellationToken = default)
        {
            await UniTask.WaitUntil(() => areAddressablesInitialized, cancellationToken: cancellationToken);

            AsyncOperationHandle<IList<T>> handler = Addressables.LoadAssetsAsync<T>("fonts", null);
            await handler.Task;

            if (handler.Status.Equals(AsyncOperationStatus.Succeeded))
                return handler.Result;

            return null;
        }

        private void AddressablesInitiated(AsyncOperationHandle<IResourceLocator> obj)
        {
            Addressables.InitializeAsync().Completed -= AddressablesInitiated;
            areAddressablesInitialized = true;
        }
    }
}
