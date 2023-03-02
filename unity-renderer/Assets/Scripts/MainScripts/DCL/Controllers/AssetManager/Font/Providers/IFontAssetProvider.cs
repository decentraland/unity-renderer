using Cysharp.Threading.Tasks;
using System.Threading;
using TMPro;
using UnityEngine;

public interface IFontAssetProvider
{
    UniTask<TMP_FontAsset> GetFontAsync(string url, CancellationToken cancellationToken = default);

}
