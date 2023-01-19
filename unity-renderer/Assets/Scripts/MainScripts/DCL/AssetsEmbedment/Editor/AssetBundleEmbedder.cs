using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace MainScripts.DCL.AssetsEmbedment.Editor
{
    public class AssetBundleEmbedder : AssetEmbedder
    {
        public static async UniTask EmbedAsync(Dictionary<string, byte[]> loadedData, string subPath)
        {
            var streamingAssetsFolder = Path.Combine(Application.streamingAssetsPath, subPath);
            ResetTargetPath(streamingAssetsFolder);

            // Save Loaded Data into `StreamingAssets`
            await Task.WhenAll(loadedData.Select(ld => File.WriteAllBytesAsync(Path.Combine(streamingAssetsFolder, ld.Key), ld.Value)));
            await UniTask.SwitchToMainThread();
        }
    }
}
