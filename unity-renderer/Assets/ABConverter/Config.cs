using System.IO;
using UnityEngine;

namespace DCL.ABConverter
{
    public static class Config
    {
        internal const string CLI_VERBOSE = "verbose";
        internal const string CLI_ALWAYS_BUILD_SYNTAX = "alwaysBuild";
        internal const string CLI_KEEP_BUNDLES_SYNTAX = "keepBundles";
        internal const string CLI_BUILD_SCENE_SYNTAX = "sceneCid";
        internal const string CLI_BUILD_WEARABLES_COLLECTION_SYNTAX = "wearablesCollectionUrnId";
        internal const string CLI_BUILD_WEARABLES_COLLECTION_RANGE_START_SYNTAX = "wearablesFirstCollectionIndex";
        internal const string CLI_BUILD_WEARABLES_COLLECTION_RANGE_END_SYNTAX = "wearablesLastCollectionIndex";
        internal const string CLI_BUILD_PARCELS_RANGE_SYNTAX = "parcelsXYWH";
        internal const string CLI_SET_CUSTOM_BASE_URL = "baseUrl";

        internal const string CLI_SET_CUSTOM_OUTPUT_ROOT_PATH = "output";

        internal static string ASSET_BUNDLE_FOLDER_NAME = "AssetBundles";
        internal static string DOWNLOADED_FOLDER_NAME = "_Downloaded";

        internal static char DASH = Path.DirectorySeparatorChar;

        internal static string DOWNLOADED_PATH_ROOT = $"{PathUtils.FixDirectorySeparator(Application.dataPath)}{DASH}{DOWNLOADED_FOLDER_NAME}";
        internal static string ASSET_BUNDLES_PATH_ROOT = $"{PathUtils.FixDirectorySeparator(Application.dataPath)}{DASH}..{DASH}{ASSET_BUNDLE_FOLDER_NAME}";

        internal static string[] bufferExtensions = { ".bin" };
        internal static string[] gltfExtensions = { ".glb", ".gltf" };
        internal static string[] textureExtensions = { ".jpg", ".png", ".jpeg", ".tga", ".gif", ".bmp", ".psd", ".tiff", ".iff" };
    }
}