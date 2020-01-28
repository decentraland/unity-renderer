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

        public static IEnumerator UseCachedMaterials(List<Renderer> renderers, bool enableRenderers = true)
        {
            if (renderers == null)
                yield break;

            int renderersCount = renderers.Count;

            if (renderersCount == 0)
                yield break;

            var matList = new List<Material>(1);

            for (int i = 0; i < renderersCount; i++)
            {
                Renderer r = renderers[i];

                if (!enableRenderers)
                    r.enabled = false;

                matList.Clear();

                foreach (var mat in r.sharedMaterials)
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

                r.sharedMaterials = matList.ToArray();
            }
        }
    }
}
