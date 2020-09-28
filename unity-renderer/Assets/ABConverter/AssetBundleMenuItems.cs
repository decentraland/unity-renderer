using DCL.Helpers;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using static DCL.ContentServerUtils;

namespace DCL
{
    public static class AssetBundleMenuItems
    {
        [MenuItem("Decentraland/Asset Bundle Builder/Dump All Wearables")]
        public static void DumpBaseAvatars()
        {
            var avatarItemList = GetAvatarMappingList("https://dcl-wearables.now.sh/index.json");
            var builder = new ABConverter.Core(ABConverter.Environment.CreateWithDefaultImplementations());
            builder.Convert(avatarItemList);
        }

        [MenuItem("Decentraland/Asset Bundle Builder/Dump Zone -110,-110")]
        public static void DumpZoneArea()
        {
            ABConverter.Client.DumpArea(new Vector2Int(-110, -110), new Vector2Int(1, 1));
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