using GLTF.Schema;
using UnityEngine;

namespace UnityGLTF.Cache
{
    public class TextureCacheData
    {
        public GLTFTexture TextureDefinition;
        public RefCountedTextureData CachedTexture;

        /// <summary>
        /// Unloads the textures in this cache.
        /// </summary>
        public void Unload()
        {
            if (CachedTexture != null)
                CachedTexture.DecreaseRefCount();
        }
    }
}
