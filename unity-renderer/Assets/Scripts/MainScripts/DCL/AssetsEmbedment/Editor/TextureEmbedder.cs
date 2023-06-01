using Cysharp.Threading.Tasks;
using MainScripts.DCL.AssetsEmbedment.Runtime;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace MainScripts.DCL.AssetsEmbedment.Editor
{
    public class TextureEmbedder : AssetEmbedder
    {
        internal static string texturesPath => Path.Combine(Application.dataPath, "Resources", EmbeddedTextureResourcesPath.VALUE).Replace('\\', '/');

        public static async UniTask EmbedAsync(Dictionary<string, Texture2D> loadedData, string subPath)
        {
            var targetPath = string.IsNullOrEmpty(subPath) ? texturesPath : Path.Combine(texturesPath, subPath);
            ResetTargetPath(targetPath);

            var encodedToPng = loadedData.Select(kvp => (kvp.Key, kvp.Value.EncodeToPNG())).ToList();

            await Task.WhenAll(encodedToPng.Select(d => File.WriteAllBytesAsync(Path.Combine(targetPath, d.Key + ".png"), d.Item2)));
            await UniTask.SwitchToMainThread();
        }
    }
}
