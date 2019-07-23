using System;
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
}
