using System.IO;
using UnityEngine;

namespace UnityGLTF.Cache
{
    public class RefCountedTextureData : RefCountedBase
    {
        public Texture2D Texture;
        private string id;

        public RefCountedTextureData(string id, Texture2D texture)
        {
            this.id = id;
            this.Texture = texture;
        }

        protected override void OnDestroyCachedData()
        {
            PersistentAssetCache.RemoveImage(id);
            Object.Destroy(Texture);
        }
    }


    public class RefCountedStreamData : RefCountedBase
    {
        public Stream stream;
        private string id;

        public RefCountedStreamData(string id, Stream stream)
        {
            this.id = id;
            this.stream = stream;
        }

        protected override void OnDestroyCachedData()
        {
            PersistentAssetCache.RemoveBuffer(id);
            stream.Dispose();
        }
    }
}