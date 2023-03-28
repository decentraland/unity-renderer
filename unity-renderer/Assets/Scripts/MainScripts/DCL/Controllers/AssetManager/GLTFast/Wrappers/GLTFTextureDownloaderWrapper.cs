using System;
using GLTFast.Loading;
using UnityEngine;
using Object = UnityEngine.Object;

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
        public string Error => asyncOp.webRequest?.error;
        public byte[] Data => asyncOp.webRequest?.downloadHandler?.data;
        public string Text => asyncOp.webRequest?.downloadHandler?.text;
        public bool? IsBinary => true;

        public bool MoveNext() =>
            asyncOp.MoveNext();

        public Texture2D GetTexture(bool forceSampleLinear)
        {
            Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, 0, forceSampleLinear);

            if (LoadTexture(texture2D))
            {
                texture2D = OptimizeTexture(forceSampleLinear, texture2D);
                return texture2D;
            }
            else
            {
                Debug.Log("Failed to load texture with downloaded data");
                DisposeTexture(texture2D);
                return null;
            }
        }

        private static void DisposeTexture(Texture2D texture2D)
        {
            if (Application.isPlaying)
                Object.Destroy(texture2D);
            else
                Object.DestroyImmediate(texture2D);
        }

        private static Texture2D OptimizeTexture(bool linear, Texture2D texture2D)
        {
#if UNITY_WEBGL
            texture2D.Compress(false);
#endif
            texture2D = TextureHelpers.ClampSize(texture2D, DataStore.i.textureConfig.gltfMaxSize.Get(), linear);
            return texture2D;
        }

        private bool LoadTexture(Texture2D texture2D)
        {
            if (asyncOp?.webRequest?.downloadHandler?.data != null)
            {
                try { texture2D.LoadImage(asyncOp.webRequest.downloadHandler.data); }
                catch (Exception e)
                {
                    Debug.LogError($"Texture promise failed: {e}");
                    return false;
                }
            }
            else { return false; }

            return true;
        }

        public void Dispose()
        {
            asyncOp.Dispose();
        }
    }
}
