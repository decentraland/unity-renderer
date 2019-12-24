using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGLTF.Cache;

namespace DCL
{
    public static class MaterialCachingHelper
    {
        public static float timeBudgetMax = 0.003f;
        public static float timeBudget = 0;

        public static string ComputeCRC(Material mat)
        {
            //NOTE(Brian): Workaround fix until we solve the CRC issue with our materials.
            if (mat.name.Contains("Mini Town_MAT"))
            {
                return mat.name;
            }

            return mat.ComputeCRC() + mat.name;
        }

        public static IEnumerator UseCachedMaterials(GameObject obj)
        {
            if (obj == null)
                yield break;

            var matList = new List<Material>(1);

            foreach (var rend in obj.GetComponentsInChildren<Renderer>(true))
            {
                matList.Clear();

                foreach (var mat in rend.sharedMaterials)
                {
                    float elapsedTime = Time.realtimeSinceStartup;

                    string crc = ComputeCRC(mat);

                    RefCountedMaterialData refCountedMat;

                    if (!PersistentAssetCache.MaterialCacheByCRC.ContainsKey(crc))
                    {
                        PersistentAssetCache.MaterialCacheByCRC.Add(crc, new RefCountedMaterialData(crc, mat));
                    }

                    refCountedMat = PersistentAssetCache.MaterialCacheByCRC[crc];
                    refCountedMat.IncreaseRefCount();

                    matList.Add(refCountedMat.material);

                    elapsedTime = Time.realtimeSinceStartup - elapsedTime;
                    timeBudget -= elapsedTime;

                    if (timeBudget < 0)
                    {
                        yield return null;
                        timeBudget += timeBudgetMax;
                    }
                }

                rend.sharedMaterials = matList.ToArray();
            }
        }
    }
}
