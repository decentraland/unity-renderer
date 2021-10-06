using GLTF.Schema;
using UnityEngine;

namespace UnityGLTF.Cache
{
    public class MaterialCacheData
    {
        public GLTFMaterial GLTFMaterial { get; set; }

        public RefCountedMaterialData CachedMaterial;

        public Material GetContents()
        {
            return CachedMaterial.material;
        }

        /// <summary>
        /// Unloads the materials in this cache.
        /// </summary>
        public void Unload()
        {
            CachedMaterial?.DecreaseRefCount();
        }
    }
}