using System;
using UnityEngine;

namespace NFTShape_Internal
{
    public interface INFTAsset : IDisposable
    {
        bool isHQ { get; }
        int hqResolution { get; }
        DCL.ITexture previewAsset { get; }
        DCL.ITexture hqAsset { get; }
        void FetchAndSetHQAsset(string url, Action onSuccess, Action onFail);
        void RestorePreviewAsset();
    }
}