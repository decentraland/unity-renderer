using System.Collections.Generic;
using System.Linq;
using DCL.Controllers;
using DCL.Interface;
using DCL.Models;
using DCL.SettingsCommon;
using UnityEngine;
using QualitySettings = DCL.SettingsCommon.QualitySettings;

namespace DCL.Helpers
{
    public static class CrashPayloadUtils
    {
        public static CrashPayload ComputePayload(IEnumerable<KeyValuePair<int, IParcelScene>> allScenes,
            List<Vector3> trackedMovements,
            List<Vector3> trackedTeleports)
        {
            CrashPayload result = new CrashPayload();

            ScenesDumper.Dump(allScenes, AssetPromiseKeeper_Texture.i.library, DataStore.i.sceneWorldObjects, result);
            PoolManagerDumper.Dump(PoolManager.i, result);
            QualitySettingsDumper.Dump(Settings.i.qualitySettings.Data, result);
            GltfDumper.Dump(AssetPromiseKeeper_GLTFast.i.library, result);
            AssetBundleDumper.Dump(AssetPromiseKeeper_AB.i.library, result);
            TextureDumper.Dump(AssetPromiseKeeper_Texture.i.library, result);
            PositionDumper.Dump(trackedMovements, trackedTeleports, result);

            return result;
        }
    }

    static class PositionDumper
    {
        public static void Dump(List<Vector3> movePositions, List<Vector3> teleportPositions, CrashPayload payload)
        {
            payload.fields.Add(CrashPayload.DumpLiterals.TRAIL, movePositions.ToArray());
            payload.fields.Add(CrashPayload.DumpLiterals.TELEPORTS, teleportPositions.ToArray());
        }
    }

    static class GltfDumper
    {
        [System.Serializable]
        public struct AssetInfo
        {
            public string id;
        }

        public static void Dump(AssetLibrary_GLTFast library, CrashPayload payload)
        {
            var ids = library.masterAssets
                .Select(x =>
                    new AssetInfo()
                    {
                        id = x.Key.ToString()
                    })
                .ToArray();

            payload.fields.Add(CrashPayload.DumpLiterals.GLTFS, ids);
        }
    }

    static class AssetBundleDumper
    {
        [System.Serializable]
        public struct AssetInfo
        {
            public string id;
        }

        public static void Dump(AssetLibrary_AB library, CrashPayload payload)
        {
            var assets = new AssetInfo[library.masterAssets.Count];

            var ids = library.masterAssets
                .Select(x =>
                    new AssetInfo()
                    {
                        id = x.Key.ToString()
                    })
                .ToArray();

            payload.fields.Add(CrashPayload.DumpLiterals.ASSET_BUNDLES, ids);
        }
    }

    static class TextureDumper
    {
        public class TextureInfo
        {
            public string id;
            public float width;
            public float height;
            public int mipmaps;
            public string format;
            public int refCount;
        }

        public static void Dump(AssetLibrary_Texture library,
            CrashPayload payload)
        {
            var apkData = library.masterAssets
                .Select(x =>
                    new TextureInfo()
                    {
                        id = x.Key.ToString(),
                        width = x.Value.asset.texture.width,
                        height = x.Value.asset.texture.height,
                        mipmaps = x.Value.asset.texture.mipmapCount,
                        format = x.Value.asset.texture.graphicsFormat.ToString(),
                        refCount = x.Value.referenceCount
                    });

            payload.fields.Add(CrashPayload.DumpLiterals.TEXTURES, apkData);
        }
    }

    static class QualitySettingsDumper
    {
        public static void Dump(QualitySettings settings, CrashPayload payload)
        {
            payload.fields.Add(CrashPayload.DumpLiterals.QUALITY_SETTINGS, settings);
        }
    }

    static class PoolManagerDumper
    {
        [System.Serializable]
        public struct PoolInfo
        {
            public string id;
            public int used;
            public int unused;
        }

        public static void Dump(PoolManager poolManager, CrashPayload payload)
        {
            PoolInfo[] pools = new PoolInfo[poolManager.pools.Count];

            int index = 0;

            foreach (KeyValuePair<object, Pool> pool in poolManager.pools)
            {
                pools[index] = new PoolInfo
                {
                    id = pool.Value.id.ToString(),
                    used = pool.Value.usedObjectsCount,
                    unused = pool.Value.unusedObjectsCount
                };

                index++;
            }

            payload.fields.Add(CrashPayload.DumpLiterals.POOL_MANAGER, pools);
        }
    }

    static class ScenesDumper
    {
        [System.Serializable]
        public struct LoadedScenesDump
        {
            public int sceneNumber;
        }

        [System.Serializable]
        public struct ComponentsDump
        {
            public string type;
            public int quantity;
        }

        public static void Dump(IEnumerable<KeyValuePair<int, IParcelScene>> allScenes, AssetLibrary_Texture library, DataStore_WorldObjects dataStoreWorldObjects, CrashPayload payload)
        {
            var componentsDump = new List<ComponentsDump>();
            var totalSceneLimits = new WebInterface.MetricsModel();

            var loadedScenes = allScenes
                .Select(x =>
                    new LoadedScenesDump
                    {
                        sceneNumber = x.Key
                    }
                )
                .ToList();

            // <class, count>
            Dictionary<int, int> sharedComponentsCount = new Dictionary<int, int>();
            Dictionary<int, int> entityComponentsCount = new Dictionary<int, int>();

            var totalMaterials = 0;
            foreach (var kvp in allScenes)
            {
                IParcelScene scene = kvp.Value;

                // Sum operator is overloaded
                totalSceneLimits += scene.metricsCounter?.currentCount.ToMetricsModel();

                int sceneNumber = kvp.Key;

                loadedScenes.Add(new LoadedScenesDump
                    {
                        sceneNumber = sceneNumber
                    }
                );

                foreach ( var kvpComponents in scene.componentsManagerLegacy.GetSceneSharedComponentsDictionary() )
                {
                    int classId = kvpComponents.Value.GetClassId();

                    if (!sharedComponentsCount.ContainsKey(classId))
                        sharedComponentsCount.Add(classId, 0);

                    sharedComponentsCount[classId]++;
                }

                foreach (var kvpEntities in kvp.Value.entities)
                {
                    using (var iterator = scene.componentsManagerLegacy.GetComponents(kvpEntities.Value))
                    {
                        while (iterator.MoveNext())
                        {
                            int classId = iterator.Current.GetClassId();

                            if ( !entityComponentsCount.ContainsKey(classId) )
                                entityComponentsCount.Add( classId, 0 );

                            entityComponentsCount[classId]++;
                        }
                    }
                }

                totalMaterials += dataStoreWorldObjects.sceneData[sceneNumber].materials.Count();
            }

            foreach (var kvp in sharedComponentsCount)
            {
                componentsDump.Add(new ComponentsDump
                    {
                        type = ((CLASS_ID) kvp.Key).ToString(),
                        quantity = kvp.Value
                    }
                );
            }

            foreach (var kvp in entityComponentsCount)
            {
                componentsDump.Add(new ComponentsDump
                    {
                        type = ((CLASS_ID_COMPONENT) kvp.Key).ToString(),
                        quantity = kvp.Value
                    }
                );
            }

            // Materials and textures can be shared between scenes. They can't be inferred by adding up the metrics.
            totalSceneLimits.materials = totalMaterials;
            totalSceneLimits.textures = library.masterAssets.Count;

            payload.fields.Add(CrashPayload.DumpLiterals.COMPONENTS, componentsDump.ToArray());
            payload.fields.Add(CrashPayload.DumpLiterals.LOADED_SCENES, loadedScenes.ToArray());
            payload.fields.Add(CrashPayload.DumpLiterals.TOTAL_SCENE_LIMITS, totalSceneLimits);
        }
    }
}
