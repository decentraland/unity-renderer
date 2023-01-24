using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace DCL
{
    public interface ITextureAssetProvider
    {
        UniTask<Texture2D> GetTextureAsync(string url, CancellationToken cancellationToken = default);
    }
}
