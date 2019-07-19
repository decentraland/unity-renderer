using GLTF.Schema;
using UnityEngine;

namespace UnityGLTF.Cache
{
    public class MaterialCacheData
    {
        public GLTFMaterial GLTFMaterial { get; set; }

        public RefCountedMaterialData CachedMaterial;
        public RefCountedMaterialData CachedMaterialWithVertexColor;
        public Material GetContents(bool useVertexColors)
        {
            if (useVertexColors)
            {
                CachedMaterialWithVertexColor.IncreaseRefCount();
                return CachedMaterialWithVertexColor.material;
            }
            else
            {
                CachedMaterial.IncreaseRefCount();
                return CachedMaterial.material;
            }
        }

        /// <summary>
        /// Unloads the materials in this cache.
        /// </summary>
        public void Unload()
        {
            CachedMaterial?.DecreaseRefCount();
            CachedMaterialWithVertexColor?.DecreaseRefCount();
        }
    }
}
