using Cysharp.Threading.Tasks;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DCL.Providers
{
    public class AddressableFontProvider : IFontAssetProvider
    {
        private readonly IAddressableResourceProvider addressableProvider;

        public AddressableFontProvider(IAddressableResourceProvider provider)
        {
            addressableProvider = provider;
        }

        public UniTask<TMP_FontAsset> GetFontAsync(string url, CancellationToken cancellationToken = default) =>
            addressableProvider.GetAddressable<TMP_FontAsset>(url, cancellationToken);
    }
}
