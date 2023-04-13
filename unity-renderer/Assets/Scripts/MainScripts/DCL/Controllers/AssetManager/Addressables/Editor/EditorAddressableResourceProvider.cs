using Cysharp.Threading.Tasks;
using DCL.Providers;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MainScripts.DCL.Controllers.AssetManager.Addressables.Editor
{
    /// <summary>
    /// <see cref="AddressableResourceProvider"/> does not advance the UniTask if not in the play mode.
    /// it is a synchronous replacements to operates from Unit tests fully synchronously
    /// </summary>
    public class EditorAddressableResourceProvider : IAddressableResourceProvider
    {
        public UniTask<IList<T>> GetAddressablesList<T>(string key, CancellationToken cancellationToken = default)
        {
            EnsureInitializedSync();
            var r = UnityEngine.AddressableAssets.Addressables.LoadAssetsAsync<T>(key, null).WaitForCompletion();
            return UniTask.FromResult(r);
        }

        public UniTask<T> GetAddressable<T>(string key, CancellationToken cancellationToken = default)
        {
            EnsureInitializedSync();

            if (typeof(Component).IsAssignableFrom(typeof(T)))
            {
                // load as GameObject instead as it is the root type
                var goRequest = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(key);
                goRequest.WaitForCompletion();

                var component = goRequest.Result.GetComponent<T>();

                if (component == null)
                    throw new InvalidKeyException($"GameObject {key} does not contain the component of the requested type {typeof(T)}");

                return UniTask.FromResult(component);
            }

            AsyncOperationHandle<T> request = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<T>(key);
            request.WaitForCompletion();
            return UniTask.FromResult(request.Result);
        }

        public UniTask<T> Instantiate<T>(string address, string name = "", CancellationToken cancellationToken = default)
        {
            EnsureInitializedSync();

            var request = UnityEngine.AddressableAssets.Addressables.InstantiateAsync(address);
            request.WaitForCompletion();

            if (Application.isEditor && name != default(string))
                request.Result.name = name;

            return UniTask.FromResult(request.Result.GetComponent<T>());
        }

        private void EnsureInitializedSync()
        {
            var init = UnityEngine.AddressableAssets.Addressables.InitializeAsync();
            init.WaitForCompletion();
        }

        public void Dispose() { }

        public void Initialize() { }
    }
}
