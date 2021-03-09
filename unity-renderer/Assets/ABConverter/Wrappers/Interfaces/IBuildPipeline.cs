using UnityEditor;
using UnityEngine;

namespace DCL
{
    public interface IBuildPipeline
    {
        AssetBundleManifest BuildAssetBundles(
            string outputPath,
            BuildAssetBundleOptions assetBundleOptions,
            BuildTarget targetPlatform);
    }
}