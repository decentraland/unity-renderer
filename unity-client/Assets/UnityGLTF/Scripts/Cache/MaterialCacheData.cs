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

        public Material GetContents(bool useVertexColors, bool increaseRefCount = true)
        {
            if (useVertexColors)
            {
                if (increaseRefCount)
                {
                    refVertexColorMat = true;
                    CachedMaterialWithVertexColor.IncreaseRefCount();
                }

                return CachedMaterialWithVertexColor.material;
            }
            else
            {
                if (increaseRefCount)
                {
                    refMat = true;
                    CachedMaterial.IncreaseRefCount();
                }

                return CachedMaterial.material;
            }
        }

        /// <summary>
        /// Unloads the materials in this cache.
        /// </summary>
        public void Unload()
        {
            if (refMat)
                CachedMaterial?.DecreaseRefCount();

            if (refVertexColorMat)
                CachedMaterialWithVertexColor?.DecreaseRefCount();
        }
    }
}