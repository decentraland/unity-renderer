using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityGLTF.Cache;

namespace DCL.Helpers
{
    public static class MaterialCachingHelper
    {
        private static readonly int _ParcelAmount = Shader.PropertyToID("_ParcelAmount");
        private static readonly int _ParcelCenters = Shader.PropertyToID("_ParcelCenters");

        [System.Flags]
        public enum Mode
        {
            NONE = 0,
            CACHE_MATERIALS = 1,
            CACHE_SHADERS = 2,
            CACHE_EVERYTHING = CACHE_MATERIALS | CACHE_SHADERS,
        }

        public static bool timeBudgetEnabled => CommonScriptableObjects.rendererState.Get();
        public static float timeBudgetMax = 0.003f;
        public static float timeBudget = 0;

        public static Dictionary<string, Shader> shaderByHash = new Dictionary<string, Shader>();

        private static Shader mainShader;

        public static string ComputeHash(Material mat) { return mat.ComputeCRC().ToString(); }

        static Shader EnsureMainShader()
        {
            if (mainShader == null)
            {
                mainShader = Shader.Find("DCL/Universal Render Pipeline/Lit");
            }

            return mainShader;
        }

        public static Material ProcessSingleMaterial(Material mat, Mode cachingFlags = Mode.CACHE_EVERYTHING)
        {
            if ((cachingFlags & Mode.CACHE_SHADERS) != 0)
            {
                string shaderHash = mat.shader.name;

                if (!shaderByHash.ContainsKey(shaderHash))
                {
                    if (!mat.shader.name.Contains("Error"))
                    {
                        shaderByHash.Add(shaderHash, Shader.Find(mat.shader.name));
                    }
                    else
                    {
                        shaderByHash.Add(shaderHash, EnsureMainShader());
                    }
                }

                mat.shader = shaderByHash[shaderHash];
            }

            if ((cachingFlags & Mode.CACHE_MATERIALS) != 0) // Cache Materials
            {
                int crc = mat.ComputeCRC();
                string hash = crc.ToString();

                RefCountedMaterialData refCountedMat;

                if (!PersistentAssetCache.MaterialCacheByCRC.ContainsKey(hash))
                {
                    var materialCopy = new Material(mat);
                    SRPBatchingHelper.OptimizeMaterial(materialCopy);
#if UNITY_EDITOR
                    materialCopy.name += $" (crc: {materialCopy.ComputeCRC()})";
#endif
                    PersistentAssetCache.MaterialCacheByCRC.Add(hash, new RefCountedMaterialData(hash, materialCopy));
                }

                refCountedMat = PersistentAssetCache.MaterialCacheByCRC[hash];
                refCountedMat.IncreaseRefCount();
                return refCountedMat.material;
            }
            else
            {
                //NOTE(Brian): Have to copy material because will be unloaded later.
                var materialCopy = new Material(mat);
                SRPBatchingHelper.OptimizeMaterial(materialCopy);
                return materialCopy;
            }
        }

        public static IEnumerator Process(List<Renderer> renderers, bool enableRenderers = true, Mode cachingFlags = Mode.CACHE_EVERYTHING)
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

                var sharedMats = r.sharedMaterials;

                for (int i1 = 0; i1 < sharedMats.Length; i1++)
                {
                    Material mat = sharedMats[i1];

                    float elapsedTime = Time.realtimeSinceStartup;

                    matList.Add( ProcessSingleMaterial(mat, cachingFlags) );

                    if (timeBudgetEnabled)
                    {
                        elapsedTime = Time.realtimeSinceStartup - elapsedTime;
                        timeBudget -= elapsedTime;

                        if (timeBudget < 0)
                        {
                            yield return null;
                            timeBudget += timeBudgetMax;
                        }
                    }
                }

                if (r != null)
                    r.sharedMaterials = matList.ToArray();
            }
        }

        public static Material ProcessSingleMaterialForScene(Material mat, string sceneId, IEnumerable<Vector2Int> parcels, Mode cachingFlags = Mode.CACHE_EVERYTHING)
        {

            if ((cachingFlags & Mode.CACHE_SHADERS) != 0)
            {
                string shaderHash = mat.shader.name;

                if (!shaderByHash.ContainsKey(shaderHash))
                {
                    if (!mat.shader.name.Contains("Error"))
                    {
                        shaderByHash.Add(shaderHash, Shader.Find(mat.shader.name));
                    }
                    else
                    {
                        shaderByHash.Add(shaderHash, EnsureMainShader());
                    }
                }

                mat.shader = shaderByHash[shaderHash];
            }

            if ((cachingFlags & Mode.CACHE_MATERIALS) != 0) // Cache Materials
            {
                int crc = mat.ComputeCRC();
                string hash = crc.ToString();
                //hash += sceneId;

                RefCountedMaterialData refCountedMat;

                if (!PersistentAssetCache.MaterialCacheByCRC.ContainsKey(hash))
                {
                    var materialCopy = new Material(mat);
                    if (!DataStore.i.featureFlags.flags.Get().IsFeatureEnabled("sbc_debug"))
                        materialCopy.EnableKeyword("_ENABLE_SB");
                    else
                        materialCopy.EnableKeyword("_ENABLE_SB_DEBUG");
                    materialCopy.SetInt(_ParcelAmount, parcels.Count());
                    materialCopy.SetVectorArray(_ParcelCenters, parcels.Select(x => new Vector4(x.x, 0, x.y, 16)).ToArray());

                    SRPBatchingHelper.OptimizeMaterial(materialCopy);
#if UNITY_EDITOR
                    materialCopy.name += $" (crc: {materialCopy.ComputeCRC()})";
#endif
                    PersistentAssetCache.MaterialCacheByCRC.Add(hash, new RefCountedMaterialData(hash, materialCopy));
                }

                refCountedMat = PersistentAssetCache.MaterialCacheByCRC[hash];
                refCountedMat.IncreaseRefCount();
                return refCountedMat.material;
            }
            else
            {
                //NOTE(Brian): Have to copy material because will be unloaded later.
                var materialCopy = new Material(mat);
                if (!DataStore.i.featureFlags.flags.Get().IsFeatureEnabled("sbc_debug"))
                    materialCopy.EnableKeyword("_ENABLE_SB");
                else
                    materialCopy.EnableKeyword("_ENABLE_SB_DEBUG");
                materialCopy.SetInt(_ParcelAmount, parcels.Count());
                materialCopy.SetVectorArray(_ParcelCenters, parcels.Select(x => new Vector4(x.x, 0, x.y, 16)).ToArray());
                SRPBatchingHelper.OptimizeMaterial(materialCopy);
                return materialCopy;
            }
        }

        public static void ProcessForScene(List<Renderer> renderers, string sceneId, IEnumerable<Vector2Int> parcels, bool enableRenderers = true, Mode cachingFlags = Mode.CACHE_EVERYTHING)
        {
            if (renderers == null)
                return;

            int renderersCount = renderers.Count;

            if (renderersCount == 0)
                return;

            var matList = new List<Material>(1);
            for (int i = 0; i < renderersCount; i++)
            {
                Renderer r = renderers[i];
                if (!enableRenderers)
                    r.enabled = false;

                matList.Clear();
                var sharedMats = r.sharedMaterials;

                for (int i1 = 0; i1 < sharedMats.Length; i1++)
                {
                    Material mat = sharedMats[i1];

                    matList.Add( ProcessSingleMaterialForScene(mat, sceneId, parcels, cachingFlags) );

                }

                if (r != null)
                    r.sharedMaterials = matList.ToArray();
            }
        }
    }
}