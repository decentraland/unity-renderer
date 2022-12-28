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

        public bool success => asyncOp.isSucceeded;
        public string error => asyncOp.webRequest.error;
        public byte[] data => asyncOp.webRequest.downloadHandler.data;
        public string text => asyncOp.webRequest.downloadHandler.text;
        public bool? isBinary => true;

        public bool MoveNext() =>
            asyncOp.MoveNext();

        public Texture2D texture
        {
            get
            {
                Texture2D texture2D;

                if (asyncOp.webRequest.downloadHandler is DownloadHandlerTexture downloadHandlerTexture) { texture2D = downloadHandlerTexture.texture; }
                else { return null; }

#if UNITY_WEBGL
                texture2D.Compress(false);
#endif

                return texture2D;
            }
        }

        public void Dispose()
        {
            asyncOp.Dispose();
        }
    }
}
