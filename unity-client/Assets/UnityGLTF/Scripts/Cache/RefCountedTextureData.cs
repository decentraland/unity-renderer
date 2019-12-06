using System.IO;
using UnityEngine;

namespace UnityGLTF.Cache
{
    public class RefCountedTextureData : RefCountedBase
    {
        public Texture2D Texture;
        private string uri;

        public RefCountedTextureData(string uri, Texture2D texture)
        {
            this.uri = uri;
            this.Texture = texture;
        }

        protected override void OnDestroyCachedData()
        {
            if (!string.IsNullOrEmpty(uri) && PersistentAssetCache.ImageCacheByUri.ContainsKey(uri))
                PersistentAssetCache.ImageCacheByUri.Remove(uri);

            UnityEngine.Object.Destroy(Texture);
        }
    }


    public class RefCountedStreamData : RefCountedBase
    {
        public Stream stream;
        private string uri;

        public RefCountedStreamData(string uri, Stream stream)
        {
            this.uri = uri;
            this.stream = stream;
        }

        protected override void OnDestroyCachedData()
        {
            if (!string.IsNullOrEmpty(uri) && PersistentAssetCache.ImageCacheByUri.ContainsKey(uri))
                PersistentAssetCache.StreamCacheByUri.Remove(uri);

            stream.Dispose();
        }
    }

}
