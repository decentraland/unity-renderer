using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public interface IHintTextureRequestHandler
{
    UniTask<Texture2D> DownloadTexture(string url, CancellationToken ctx, int timeout = 2);
}
