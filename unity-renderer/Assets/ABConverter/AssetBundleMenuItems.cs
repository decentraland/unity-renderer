using System.Collections.Generic;
using System.IO;
using DCL.ABConverter;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using static DCL.ContentServerUtils;
using Utils = DCL.Helpers.Utils;

namespace DCL
{
    public static class AssetBundleMenuItems
    {
        [System.Serializable]
        public class EmptyParcels
        {
            public MappingPair[] EP_01;
            public MappingPair[] EP_02;
            public MappingPair[] EP_03;
            public MappingPair[] EP_04;
            public MappingPair[] EP_05;
            public MappingPair[] EP_06;
            public MappingPair[] EP_07;
            public MappingPair[] EP_08;
            public MappingPair[] EP_09;
            public MappingPair[] EP_10;
            public MappingPair[] EP_11;
            public MappingPair[] EP_12;
        }

        private const string KERNEL_RELATIVE_PATH = "/../../../kernel"; // This has to be set manually according to the local paths 

        [MenuItem("Decentraland/Asset Bundle Builder/Dump Default Empty Parcels")]
        public static void DumpEmptyParcels_Default() { DumpEmptyParcels(); }

        [MenuItem("Decentraland/Asset Bundle Builder/Dump Halloween Empty Parcels")]
        public static void DumpEmptyParcels_Halloween() { DumpEmptyParcels("halloween"); }
        
        [MenuItem("Decentraland/Asset Bundle Builder/Dump XMas Empty Parcels")]
        public static void DumpEmptyParcels_XMas() { DumpEmptyParcels("xmas"); }

        public static void DumpEmptyParcels(string folderName = "common")
        {
            string mappingsJsonPath = Application.dataPath;

            mappingsJsonPath += KERNEL_RELATIVE_PATH + $"/public/empty-scenes/{folderName}/mappings.json";

            if (!File.Exists(mappingsJsonPath))
            {
                Debug.LogError($"mappings.json not found at '{mappingsJsonPath}'! Make sure to run 'make empty-parcels' and then 'make watch'");
                return;
            }

            string emptyScenes = File.ReadAllText(mappingsJsonPath);
            var es = JsonUtility.FromJson<EmptyParcels>(emptyScenes);

            List<MappingPair> mappings = new List<MappingPair>();

            mappings.AddRange(es.EP_01);
            mappings.AddRange(es.EP_02);
            mappings.AddRange(es.EP_03);
            mappings.AddRange(es.EP_04);
            mappings.AddRange(es.EP_05);
            mappings.AddRange(es.EP_06);
            mappings.AddRange(es.EP_07);
            mappings.AddRange(es.EP_08);
            mappings.AddRange(es.EP_09);
            mappings.AddRange(es.EP_10);
            mappings.AddRange(es.EP_11);
            mappings.AddRange(es.EP_12);

            string emptyScenesResourcesPath = Application.dataPath;
            emptyScenesResourcesPath += KERNEL_RELATIVE_PATH + $"/public/empty-scenes/{folderName}";

            string customBaseUrl = "file://" + emptyScenesResourcesPath;

            var settings = new ABConverter.ClientSettings(ApiTLD.NONE);
            settings.skipAlreadyBuiltBundles = true;
            settings.deleteDownloadPathAfterFinished = false;
            settings.baseUrl = customBaseUrl + "/contents/";

            var core = new ABConverter.Core(ABConverter.Environment.CreateWithDefaultImplementations(), settings);
            core.Convert(mappings.ToArray());
        }

        [MenuItem("Decentraland/Asset Bundle Builder/Dump All Body-Wearables")]
        public static void DumpAllBodiesWearables() { ABConverter.WearablesCollectionClient.DumpAllBodyshapeWearables(); }

        [MenuItem("Decentraland/Asset Bundle Builder/Dump All Non-Body-Wearables (Optimized)")]
        public static void DumpAllNonBodiesWearables() { ABConverter.WearablesCollectionClient.DumpAllNonBodyshapeWearables(); }
        
        [MenuItem("Decentraland/Asset Bundle Builder/Dump Single Wearables Collection")]
        public static void DumpSingleWearablesCollection() { ABConverter.WearablesCollectionClient.DumpSingleWearablesCollection("urn:decentraland:ethereum:collections-v1:atari_launch"); }

        [MenuItem("Decentraland/Start Visual Tests")]
        public static void StartVisualTests() { EditorCoroutineUtility.StartCoroutineOwnerless(VisualTests.TestConvertedAssets()); }

        [MenuItem("Decentraland/Asset Bundle Builder/Dump Org -110,-110")]
        public static void DumpArea() { ABConverter.SceneClient.DumpArea(new Vector2Int(-110, -110), new Vector2Int(1, 1)); }

        [MenuItem("Decentraland/Asset Bundle Builder/Dump scene QmXMzPLZNx5EHiYi3tK9MT5g9HqjAqgyAoZUu2LfAXJcSM")]
        public static void DumpSceneId() { ABConverter.SceneClient.DumpScene("QmXMzPLZNx5EHiYi3tK9MT5g9HqjAqgyAoZUu2LfAXJcSM"); }

        [MenuItem("Decentraland/Asset Bundle Builder/Dump Single Asset")]
        public static void DumpSingleAsset()
        {
            // TODO: Make an editor window to setup these values from editor (for other dump-modes as well)
            ABConverter.SceneClient.DumpAsset("QmS9eDwvcEpyYXChz6pFpyWyfyajiXbt6KA4CxQa3JKPGC",
                "models/FloorBaseGrass_01/FloorBaseGrass_01.glb",
                "QmXMzPLZNx5EHiYi3tK9MT5g9HqjAqgyAoZUu2LfAXJcSM");
        }

        [MenuItem("Decentraland/Asset Bundle Builder/Dump Org -6,30")]
        public static void DumpOrg() { ABConverter.SceneClient.DumpArea(new Vector2Int(-6, 30), new Vector2Int(15, 15)); }

        [MenuItem("Decentraland/Asset Bundle Builder/Dump Org 0,0")]
        public static void DumpCenterPlaza()
        {
            var zoneArray = Utils.GetCenteredZoneArray(new Vector2Int(0, 0), new Vector2Int(1, 1));
            ABConverter.SceneClient.DumpArea(zoneArray);
        }

        [MenuItem("Decentraland/Asset Bundle Builder/Only Build Bundles")]
        public static void OnlyBuildBundles() { BuildPipeline.BuildAssetBundles(ABConverter.Config.ASSET_BUNDLES_PATH_ROOT, BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.ForceRebuildAssetBundle, BuildTarget.WebGL); }
    }
}