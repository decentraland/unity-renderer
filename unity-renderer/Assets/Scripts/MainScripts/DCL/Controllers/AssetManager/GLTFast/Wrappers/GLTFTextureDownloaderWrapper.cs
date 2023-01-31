using GLTFast.Loading;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL.GLTFast.Wrappers
{
    internal class GltfTextureDownloaderWrapper : ITextureDownload
    {
        private readonly UnityWebRequest uwr;

        public GltfTextureDownloaderWrapper(UnityWebRequest uwr)
        {
            this.uwr = uwr;
        }

        public bool Success => uwr.result == UnityWebRequest.Result.Success;
        public string Error => uwr.error;
        public byte[] Data => uwr.downloadHandler.data;
        public string Text => uwr.downloadHandler.text;
        public bool? IsBinary => true;

        
        public Texture2D Texture
        {
            get
            {
                Texture2D texture2D;

                if (uwr.downloadHandler is DownloadHandlerTexture downloadHandlerTexture)
                    texture2D = downloadHandlerTexture.texture;
                else
                    return null;

#if UNITY_WEBGL
                texture2D.Compress(false);
#endif
                texture2D = TextureHelpers.ClampSize(texture2D, DataStore.i.textureConfig.gltfMaxSize.Get(), true);

                return texture2D;
            }
        }

        public void Dispose()
        {
            uwr.Dispose();
        }
    }
}
