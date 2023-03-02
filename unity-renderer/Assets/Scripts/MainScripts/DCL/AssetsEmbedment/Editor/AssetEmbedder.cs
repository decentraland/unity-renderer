using System.IO;

namespace MainScripts.DCL.AssetsEmbedment.Editor
{
    public abstract class AssetEmbedder
    {
        protected static void ResetTargetPath(string path)
        {
            // Clear the previous cache
            if (Directory.Exists(path))
                Directory.Delete(path, true);

            Directory.CreateDirectory(path);
        }
    }
}
