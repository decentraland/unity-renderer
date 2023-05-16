using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace DefaultNamespace
{
    public static class TestsUtils
    {
        public const string ASSETS_FOLDER_PATH = "Assets";

        public static IEnumerable<string> AllAssetsAtPaths(string assetTypes, params string[] assetPaths) =>
            AssetDatabase
               .FindAssets(assetTypes, assetPaths)
               .Select(AssetDatabase.GUIDToAssetPath);
    }
}
