using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace DCL.Providers
{
    /// <summary>
    /// Service provider for addressable assets
    /// </summary>
    public class AddressableResourceProvider : IAddressableResourceProvider
    {
        /// <summary>
        /// Provides a list of addressable assets
        /// </summary>
        /// <param name="key">The label to look for the addressable</param>
        /// <param name="cancellationToken">Flow control cancellation token</param>
        /// <typeparam name="T">Asset type</typeparam>
        /// <returns></returns>
        public async UniTask<IList<T>> GetAddressablesList<T>(string key, CancellationToken cancellationToken = default)
        {
            //This function does nothing if initialization has already occurred
            await Addressables.InitializeAsync().WithCancellation(cancellationToken);

            AsyncOperationHandle<IList<T>> request = Addressables.LoadAssetsAsync<T>(key, null);
            await request.WithCancellation(cancellationToken);
            return request.Result;
        }

        /// <summary>
        /// Provides a single addressable asset
        /// </summary>
        /// <param name="key">Key or address of the asset to be retrieved</param>
        /// <param name="cancellationToken">Flow control cancellation token</param>
        /// <typeparam name="T">Asset type</typeparam>
        /// <returns></returns>
        public async UniTask<T> GetAddressable<T>(string key, CancellationToken cancellationToken = default)
        {
            // This function does nothing if initialization has already occurred
            await Addressables.InitializeAsync().WithCancellation(cancellationToken);

            // T should be an asset type, component is not accepted
            if (typeof(Component).IsAssignableFrom(typeof(T)))
            {
                // load as GameObject instead as it is the root type
                var goRequest = Addressables.LoadAssetAsync<GameObject>(key);
                await goRequest.WithCancellation(cancellationToken);

                var component = goRequest.Result.GetComponent<T>();

                if (component == null)
                    throw new InvalidKeyException($"GameObject {key} does not contain the component of the requested type {typeof(T)}");

                return component;
            }

            AsyncOperationHandle<T> request = Addressables.LoadAssetAsync<T>(key);
            await request.WithCancellation(cancellationToken);
            return request.Result;
        }

        public async UniTask<T> Instantiate<T>(string address, string name = default, Transform parent = null, CancellationToken cancellationToken = default)
        {
            // This function does nothing if initialization has already occurred
            await Addressables.InitializeAsync().WithCancellation(cancellationToken);

            AsyncOperationHandle<GameObject> request = Addressables.InstantiateAsync(address, parent);
            await request.WithCancellation(cancellationToken);

            if(Application.isEditor && name != default(string))
                request.Result.name = name;

            return request.Result.GetComponent<T>();
        }

        public void Dispose() { }

        public void Initialize() { }
    }
}
