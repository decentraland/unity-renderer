using Cysharp.Threading.Tasks;
using DCL.Providers;
using System;
using System.Threading;
using TMPro;

public class AddressableFontProvider : AddressableResourceProvider<TMP_FontAsset>, IFontAssetProvider
{
    public UniTask<TMP_FontAsset> GetFontAsync(string url, CancellationToken cancellationToken = default)  =>
        GetAddressable(url, cancellationToken);
}
