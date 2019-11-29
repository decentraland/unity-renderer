using System.Collections.Generic;
using UnityEngine;
using UnityGLTF.Cache;

namespace DCL
{
    public static class MaterialCachingHelper
    {
        public static void UseCachedMaterials(GameObject obj)
        {
            if (obj == null)
                return;

            var matList = new List<Material>(1);

            foreach (var rend in obj.GetComponentsInChildren<Renderer>(true))
            {
                matList.Clear();

                foreach (var mat in rend.sharedMaterials)
                {
                    string crc = mat.ComputeCRC() + mat.name;

                    RefCountedMaterialData refCountedMat;

                    if (!PersistentAssetCache.MaterialCacheByCRC.ContainsKey(crc))
                    {
                        mat.enableInstancing = true;
                        PersistentAssetCache.MaterialCacheByCRC.Add(crc, new RefCountedMaterialData(crc, mat));
                    }

                    refCountedMat = PersistentAssetCache.MaterialCacheByCRC[crc];
                    refCountedMat.IncreaseRefCount();

                    matList.Add(refCountedMat.material);
                }

                rend.sharedMaterials = matList.ToArray();
            }
        }
    }
}
