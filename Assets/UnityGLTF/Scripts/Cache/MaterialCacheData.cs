using GLTF.Schema;
using UnityEngine;

namespace UnityGLTF.Cache
{
    public class MaterialCacheData
    {
        public GLTFMaterial GLTFMaterial { get; set; }

        public RefCountedMaterialData CachedMaterial;
        public RefCountedMaterialData CachedMaterialWithVertexColor;

        private bool refVertexColorMat = false;
        private bool refMat = false;

        public Material GetContents(bool useVertexColors)
        {
            if (useVertexColors)
            {
                refVertexColorMat = true;
                CachedMaterialWithVertexColor.IncreaseRefCount();
                return CachedMaterialWithVertexColor.material;
            }
            else
            {
                refMat = true;
                CachedMaterial.IncreaseRefCount();
                return CachedMaterial.material;
            }
        }

        /// <summary>
        /// Unloads the materials in this cache.
        /// </summary>
        public void Unload()
        {
            if (refVertexColorMat)
                CachedMaterial?.DecreaseRefCount();

            if (refMat)
                CachedMaterialWithVertexColor?.DecreaseRefCount();
        }
    }
}
