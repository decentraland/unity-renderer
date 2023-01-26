using Cysharp.Threading.Tasks;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DCL.Providers
{
    public class AddressableFontProvider : IFontAssetProvider
    {
        private readonly AddressableResourceProvider addressableProvider;

        public AddressableFontProvider(AddressableResourceProvider provider)
        {
            addressableProvider = provider;
            Debug.Log("AAAA " + Addressables.RuntimePath);
        }

        public UniTask<TMP_FontAsset> GetFontAsync(string url, CancellationToken cancellationToken = default) =>
            addressableProvider.GetAddressable<TMP_FontAsset>(url, cancellationToken);
    }
}
