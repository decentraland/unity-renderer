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

        public Texture2D GetTexture(bool linear)
        {
            Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, 0, linear);
            Debug.Log($"DEBUG NULL 1: {asyncOp == null}");
            Debug.Log($"DEBUG NULL 2: {asyncOp.webRequest == null}");
            Debug.Log($"DEBUG NULL 3: {asyncOp.webRequest.downloadHandler == null}");

            if (asyncOp.webRequest.downloadHandler.data != null)
            {
                try { texture2D.LoadImage(asyncOp.webRequest.downloadHandler.data); }
                catch (Exception e)
                {
                    Debug.LogError($"Texture promise failed: {e}");
                    return null;
                }
            }
            else { return null; }
#if UNITY_WEBGL
                texture2D.Compress(false);
#endif
            texture2D = TextureHelpers.ClampSize(texture2D, DataStore.i.textureConfig.gltfMaxSize.Get(), linear);

            return texture2D;
        }

        public void Dispose()
        {
            asyncOp.Dispose();
        }
    }
}
