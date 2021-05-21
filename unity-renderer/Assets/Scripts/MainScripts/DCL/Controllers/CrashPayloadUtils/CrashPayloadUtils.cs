using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Controllers;
using DCL.Interface;
using DCL.Models;
using DCL.SettingsData;
using UnityGLTF.Cache;

namespace DCL.Helpers
{
    public static class CrashPayloadUtils
    {
        public static CrashPayload ComputePayload(Dictionary<string, IParcelScene> allScenes)
        {
            CrashPayload result = new CrashPayload();

            ScenesDumper.Dump(allScenes, result );
            PoolManagerDumper.Dump(PoolManager.i, result );
            QualitySettingsDumper.Dump(Settings.i.currentQualitySettings, result );
            GltfDumper.Dump( AssetPromiseKeeper_GLTF.i as AssetPromiseKeeper_GLTF, result );
            AssetBundleDumper.Dump( AssetPromiseKeeper_AB.i as AssetPromiseKeeper_AB, result );
            TextureDumper.Dump( AssetPromiseKeeper_Texture.i as AssetPromiseKeeper_Texture, PersistentAssetCache.ImageCacheByUri, result );

            return result;
        }
    }

    static class GltfDumper
    {
        [System.Serializable]
        public struct AssetInfo
        {
            public string id;
        }

        public static void Dump(AssetPromiseKeeper_GLTF apk, CrashPayload payload)
        {
            var assets = new AssetInfo[ apk.library.masterAssets.Count ];

            var ids = apk.library.masterAssets
                .Select( x =>
                    new AssetInfo()
                    {
                        id = x.Key as string
                    } )
                .ToArray();

            payload.fields.Add( CrashPayload.DumpLiterals.GLTFS, ids );
        }
    }

    static class AssetBundleDumper
    {
        [System.Serializable]
        public struct AssetInfo
        {
            public string id;
        }

        public static void Dump(AssetPromiseKeeper_AB apk, CrashPayload payload)
        {
            var assets = new AssetInfo[ apk.library.masterAssets.Count ];

            var ids = apk.library.masterAssets
                .Select( x =>
                    new AssetInfo()
                    {
                        id = x.Key as string
                    } )
                .ToArray();

            payload.fields.Add( CrashPayload.DumpLiterals.ASSET_BUNDLES, ids );
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

        public static void Dump(AssetPromiseKeeper_Texture apk, Dictionary<string, RefCountedTextureData> textureData, CrashPayload payload)
        {
            var apkData = apk.library.masterAssets
                .Select( x =>
                    new TextureInfo()
                    {
                        id = x.Key as string,
                        width = x.Value.asset.texture.width,
                        height = x.Value.asset.texture.height,
                        mipmaps = x.Value.asset.texture.mipmapCount,
                        format = x.Value.asset.texture.graphicsFormat.ToString(),
                        refCount = x.Value.referenceCount
                    } );

            var persistentCacheData = textureData
                .Select( x =>
                    new TextureInfo()
                    {
                        id = x.Key as string,
                        height = x.Value.Texture.width,
                        mipmaps = x.Value.Texture.mipmapCount,
                        format = x.Value.Texture.graphicsFormat.ToString(),
                        refCount = x.Value.RefCount
                    } );

            TextureInfo[] finalData = apkData.Union( persistentCacheData ).ToArray();

            payload.fields.Add( CrashPayload.DumpLiterals.TEXTURES, finalData );
        }
    }

    static class QualitySettingsDumper
    {
        public static void Dump(DCL.SettingsData.QualitySettings settings, CrashPayload payload) { payload.fields.Add( CrashPayload.DumpLiterals.QUALITY_SETTINGS, settings ); }
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
            PoolInfo[] pools = new PoolInfo[ poolManager.pools.Count ];

            int index = 0;

            foreach ( KeyValuePair<object, Pool> pool in poolManager.pools )
            {
                pools[index] = new PoolInfo
                {
                    id = (string)pool.Value.id,
                    used = pool.Value.usedObjectsCount,
                    unused = pool.Value.unusedObjectsCount
                };

                index++;
            }

            payload.fields.Add( CrashPayload.DumpLiterals.POOL_MANAGER, pools );
        }
    }

    static class ScenesDumper
    {
        [System.Serializable]
        public struct LoadedScenesDump
        {
            public string id;
        }

        [System.Serializable]
        public struct ComponentsDump
        {
            public string type;
            public int quantity;
        }

        public static void Dump(Dictionary<string, IParcelScene> allScenes, CrashPayload payload)
        {
            var loadedScenes = new List<LoadedScenesDump>();
            var componentsDump = new List<ComponentsDump>();
            var totalSceneLimits = new WebInterface.MetricsModel();

            loadedScenes = allScenes
                .Select( x =>
                    new LoadedScenesDump
                    {
                        id = x.Key
                    }
                )
                .ToList();

            // <class, count>
            Dictionary<int, int> sharedComponentsCount = new Dictionary<int, int>();
            Dictionary<int, int> entityComponentsCount = new Dictionary<int, int>();

            foreach ( var kvp in allScenes )
            {
                IParcelScene scene = kvp.Value;

                // Sum operator is overloaded
                totalSceneLimits += scene.metricsController.GetModel().ToMetricsModel();

                loadedScenes.Add( new LoadedScenesDump
                    {
                        id = kvp.Key
                    }
                );

                foreach ( var kvpComponents in scene.disposableComponents )
                {
                    sharedComponentsCount[ kvpComponents.Value.GetClassId() ]++;
                }

                foreach ( var kvpEntities in kvp.Value.entities )
                {
                    foreach ( var kvpComponents in kvpEntities.Value.components )
                    {
                        entityComponentsCount[ kvpComponents.Value.GetClassId() ]++;
                    }
                }
            }

            foreach ( var kvp in sharedComponentsCount )
            {
                componentsDump.Add( new ComponentsDump
                    {
                        type = ((CLASS_ID)kvp.Key).ToString(),
                        quantity = kvp.Value
                    }
                );
            }

            foreach ( var kvp in entityComponentsCount )
            {
                componentsDump.Add( new ComponentsDump
                    {
                        type = ((CLASS_ID_COMPONENT)kvp.Key).ToString(),
                        quantity = kvp.Value
                    }
                );
            }

            payload.fields.Add(CrashPayload.DumpLiterals.COMPONENTS, componentsDump.ToArray());
            payload.fields.Add(CrashPayload.DumpLiterals.LOADED_SCENES, loadedScenes.ToArray());
            payload.fields.Add(CrashPayload.DumpLiterals.TOTAL_SCENE_LIMITS, totalSceneLimits);
        }
    }
}