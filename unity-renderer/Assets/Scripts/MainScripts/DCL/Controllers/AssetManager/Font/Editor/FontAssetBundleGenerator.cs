using DCL.ABConverter;
using System.IO;
using UnityEditor;
using UnityEngine;

public class FontAssetBundleGenerator : MonoBehaviour
{

    [MenuItem("Decentraland/Build Font AB from folder", priority = 2)]
    private static void BuildFontABForAllPlatforms()
    {
        BuildFontAB(BuildTarget.WebGL);
        BuildFontAB(BuildTarget.StandaloneWindows);
        BuildFontAB(BuildTarget.StandaloneWindows64);
    }


    private static void BuildFontAB(BuildTarget target)
    {
        string assetBundleDirectory = "Assets/StreamingAssets/fonts";

        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }

        string fontDirectory = "Assets/TextMesh Pro/Resources/Fonts Sources/";

        MarkFolderForAssetBundleBuild(fontDirectory, $"fontAssets_{BuildPipeline.GetBuildTargetName(target)}");

        BuildPipeline.BuildAssetBundles(assetBundleDirectory,
            BuildAssetBundleOptions.None,
            target);
    }

    internal static void MarkFolderForAssetBundleBuild(string fullPath, string abName)
    {
        string assetPath = PathUtils.GetRelativePathTo(Application.dataPath, fullPath);
        assetPath = Path.GetDirectoryName(assetPath);
        AssetImporter importer = AssetImporter.GetAtPath(assetPath);
        importer.SetAssetBundleNameAndVariant(abName, "");
    }
}
