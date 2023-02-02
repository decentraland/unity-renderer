using UnityEditor;
using UnityEngine;

namespace MainScripts.DCL.AssetsEmbedment.Editor
{
    public class EmbeddedTextureProcessor : AssetPostprocessor
    {
        private static string texturesPathCached;

        private static string texturesPath =
            texturesPathCached ??= TextureEmbedder.texturesPath.StartsWith(Application.dataPath)
                ? TextureEmbedder.texturesPath.Remove(0, Application.dataPath.Length)
                : TextureEmbedder.texturesPath;

        private void OnPreprocessTexture()
        {
            if (assetPath.Contains(texturesPath))
            {
                var texImporter = (TextureImporter)assetImporter;
                texImporter.isReadable = true;
                texImporter.alphaIsTransparency = true;
                texImporter.mipmapEnabled = false;
            }
        }
    }
}
