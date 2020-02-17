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

        public static Dictionary<string, Shader> nameToShader = new Dictionary<string, Shader>();

        public static string ComputeCRC(Material mat)
        {
            return mat.ComputeCRC().ToString();
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
                    string shaderName = mat.shader.name;

                    if (nameToShader.ContainsKey(shaderName))
                    {
                        mat.shader = nameToShader[shaderName];
                    }
                    else
                    {
                        nameToShader.Add(shaderName, Shader.Find(shaderName));
                    }

                    string crc = ComputeCRC(mat);

                    RefCountedMaterialData refCountedMat;

                    if (!PersistentAssetCache.MaterialCacheByCRC.ContainsKey(crc))
                    {
                        //NOTE(Brian): Have to copy material because will be unloaded later.
                        var materialCopy = new Material(mat);
                        PersistentAssetCache.MaterialCacheByCRC.Add(crc, new RefCountedMaterialData(crc, materialCopy));
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
