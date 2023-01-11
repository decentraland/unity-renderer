using System;
using GLTFast.Loading;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL.GLTFast.Wrappers
{
    internal class GltfTextureDownloaderWrapper : ITextureDownload
    {
        private readonly IWebRequestAsyncOperation asyncOp;

        public GltfTextureDownloaderWrapper(IWebRequestAsyncOperation asyncOp)
        {
            this.asyncOp = asyncOp;
        }

        public bool Success => asyncOp.isSucceeded;
        public string Error => asyncOp.webRequest.error;
        public byte[] Data => asyncOp.webRequest.downloadHandler.data;
        public string Text => asyncOp.webRequest.downloadHandler.text;
        public bool? IsBinary => true;

        public bool MoveNext() =>
            asyncOp.MoveNext();

        public Texture2D Texture
        {
            get
            {
                Texture2D texture2D;

                if (asyncOp.webRequest.downloadHandler is DownloadHandlerTexture downloadHandlerTexture) { texture2D = downloadHandlerTexture.texture; }
                else { return null; }

#if UNITY_WEBGL
                texture2D.Compress(false);
#endif
                texture2D = TextureHelpers.ClampSize(texture2D, DataStore.i.textureConfig.gltfMaxSize.Get(), true);

                return texture2D;
            }
        }

        public void Dispose()
        {
            asyncOp.Dispose();
        }
    }
}
