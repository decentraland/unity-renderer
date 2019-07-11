using System;

namespace UnityGLTF.Cache
{
    /// <summary>
    /// A ref-counted cache data object containing lists of Unity objects that were created for the sake of a GLTF scene/node.
    /// This supports counting the amount of refcounts that will dispose of itself
    /// </summary>
    public class RefCountedCacheData : RefCountedBase
    {

        /// <summary>
        /// Meshes used by this GLTF node.
        /// </summary>
        public MeshCacheData[][] MeshCache { get; set; }

        /// <summary>
        /// Materials used by this GLTF node.
        /// </summary>
        public MaterialCacheData[] MaterialCache { get; set; }

        /// <summary>
        /// Textures used by this GLTF node.
        /// </summary>
        public TextureCacheData[] TextureCache { get; set; }

        protected override void OnDestroyCachedData()
        {
            // Destroy the cached meshes
            for (int i = 0; i < MeshCache.Length; i++)
            {
                for (int j = 0; j < MeshCache[i].Length; j++)
                {
                    if (MeshCache[i][j] != null)
                    {
                        MeshCache[i][j].Unload();
                    }
                }
            }

            // Decrease ref count of cached textures
            for (int i = 0; i < TextureCache.Length; i++)
            {
                if (TextureCache[i] != null)
                {
                    TextureCache[i].Unload();
                }
            }

            // Destroy the cached materials
            for (int i = 0; i < MaterialCache.Length; i++)
            {
                if (MaterialCache[i] != null)
                {
                    MaterialCache[i].Unload();
                }
            }
        }
    }
}
