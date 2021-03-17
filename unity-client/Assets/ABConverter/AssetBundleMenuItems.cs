using System;
using DCL.Helpers;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using DCL.ABConverter;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityGLTF;
using static DCL.ContentServerUtils;
using Utils = DCL.Helpers.Utils;

namespace DCL.ABConverter
{
}

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

        [MenuItem("Decentraland/Asset Bundle Builder/Dump Default Empty Parcels")]
        public static void DumpEmptyParcels_Default()
        {
            DumpEmptyParcels();
        }

        [MenuItem("Decentraland/Asset Bundle Builder/Dump Halloween Empty Parcels")]
        public static void DumpEmptyParcels_Halloween()
        {
            DumpEmptyParcels("empty-scenes-halloween");
        }

        public static void DumpEmptyParcels(string folderName = "empty-scenes")
        {
            string indexJsonPath = Application.dataPath;

            indexJsonPath += $"/../../kernel/static/loader/{folderName}/index.json";

            if (!File.Exists(indexJsonPath))
            {
                Debug.LogError("Index.json path doesn't exists! Make sure to 'make watch' first so it gets generated.");
                return;
            }

            string emptyScenes = File.ReadAllText(indexJsonPath);
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
            emptyScenesResourcesPath += $"/../../kernel/static/loader/{folderName}";

            string customBaseUrl = "file://" + emptyScenesResourcesPath;

            var settings = new ABConverter.Client.Settings(ApiTLD.NONE);
            settings.skipAlreadyBuiltBundles = true;
            settings.deleteDownloadPathAfterFinished = false;
            settings.baseUrl = customBaseUrl + "/contents/";

            var core = new ABConverter.Core(ABConverter.Environment.CreateWithDefaultImplementations(), settings);
            core.Convert(mappings.ToArray());
        }

        [MenuItem("Decentraland/Asset Bundle Builder/Dump All Wearables")]
        public static void DumpBaseAvatars()
        {
            var avatarItemList = GetAvatarMappingList("https://dcl-wearables.now.sh/index.json");
            var builder = new ABConverter.Core(ABConverter.Environment.CreateWithDefaultImplementations());
            builder.Convert(avatarItemList);
        }

        [MenuItem("Decentraland/Start Visual Tests")]
        public static void StartVisualTests()
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(VisualTests.TestConvertedAssets());

        }

        [MenuItem("Decentraland/Asset Bundle Builder/Dump Org -110,-110")]
        public static void DumpZoneArea()
        {
            ABConverter.Client.DumpArea(new Vector2Int(-110, -110), new Vector2Int(1, 1));
        }

         [MenuItem("Decentraland/Asset Bundle Builder/Dump Single Asset")]
         public static void DumpSingleAsset()
         {
             // TODO: Make an editor window to setup these values from editor (for other dump-modes as well)
             ABConverter.Client.DumpAsset("QmS9eDwvcEpyYXChz6pFpyWyfyajiXbt6KA4CxQa3JKPGC",
                                            "models/FloorBaseGrass_01/FloorBaseGrass_01.glb",
                                            "QmXMzPLZNx5EHiYi3tK9MT5g9HqjAqgyAoZUu2LfAXJcSM");
         }

        [MenuItem("Decentraland/Asset Bundle Builder/Dump Org 0,0")]
        public static void DumpCenterPlaza()
        {
            var zoneArray = Utils.GetCenteredZoneArray(new Vector2Int(0, 0), new Vector2Int(1, 1));
            ABConverter.Client.DumpArea(zoneArray);
        }

        [MenuItem("Decentraland/Asset Bundle Builder/Only Build Bundles")]
        public static void OnlyBuildBundles()
        {
            BuildPipeline.BuildAssetBundles(ABConverter.Config.ASSET_BUNDLES_PATH_ROOT, BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.ForceRebuildAssetBundle, BuildTarget.WebGL);
        }

        public class WearableItemArray
        {
            public List<WearableItem> data;
        }

        public static MappingPair[] GetAvatarMappingList(string url)
        {
            List<MappingPair> mappingPairs = new List<MappingPair>();

            UnityWebRequest w = UnityWebRequest.Get(url);
            w.SendWebRequest();

            while (!w.isDone)
            {
            }

            if (!w.WebRequestSucceded())
            {
                Debug.LogWarning($"Request error! Parcels couldn't be fetched! -- {w.error}");
                return null;
            }

            var avatarApiData = JsonUtility.FromJson<WearableItemArray>("{\"data\":" + w.downloadHandler.text + "}");

            foreach (var avatar in avatarApiData.data)
            {
                foreach (var representation in avatar.representations)
                {
                    foreach (var datum in representation.contents)
                    {
                        mappingPairs.Add(datum);
                    }
                }
            }

            return mappingPairs.ToArray();
        }
    }
}