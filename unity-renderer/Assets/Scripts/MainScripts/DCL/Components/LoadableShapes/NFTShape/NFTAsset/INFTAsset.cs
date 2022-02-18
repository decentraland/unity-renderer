using System;
using UnityEngine;

namespace NFTShape_Internal
{
    public interface INFTAsset : IDisposable
    {
        bool isHQ { get; }
        DCL.ITexture previewAsset { get; }
        DCL.ITexture hqAsset { get; }
        void FetchAndSetHQAsset(string url, Action onSuccess, Action<Exception> onFail);
        void RestorePreviewAsset();
        event Action<Texture2D> OnTextureUpdate;
    }
}