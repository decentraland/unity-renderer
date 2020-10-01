using System.Collections.Generic;
using System.IO;
using DCL;
using UnityEditor;
using UnityEngine;

namespace DCL
{
    public sealed partial class Mocked
    {
        //TODO(Brian): Evaluate if we can use mocking library to replace this mock
        public class AssetDatabase : IAssetDatabase
        {
            private static Logger logger = new Logger("Mocked.AssetDatabase") {verboseEnabled = false};

            public HashSet<string> importedAssets = new HashSet<string>();
            public HashSet<string> savedAssets = new HashSet<string>();

            private IFile file;

            public AssetDatabase(IFile file)
            {
                this.file = file;
            }

            public void Refresh(ImportAssetOptions options = ImportAssetOptions.Default)
            {
                logger.Verbose("Refreshing assets...");

                foreach (var asset in importedAssets)
                {
                    string metaPath = Path.ChangeExtension(asset, ".meta");

                    if (!file.Exists(metaPath))
                        file.Copy(asset, metaPath);
                }
            }

            public void SaveAssets()
            {
                logger.Verbose("Saving assets...");
                savedAssets = new HashSet<string>(importedAssets);
            }

            public void ImportAsset(string fullPath, ImportAssetOptions options = ImportAssetOptions.Default)
            {
                if (!file.Exists(fullPath))
                {
                    logger.Verbose($"Importing asset fail (not exist)...\n({fullPath})");
                    return;
                }

                if (importedAssets.Contains(fullPath))
                {
                    logger.Verbose($"Importing asset fail (contained)...\n({fullPath})");
                    return;
                }

                file.Copy(fullPath, Path.ChangeExtension(fullPath, ".meta"));
                importedAssets.Add(fullPath);
                logger.Verbose($"Importing asset...\n({fullPath})");
            }

            public bool DeleteAsset(string path)
            {
                if (importedAssets.Contains(path))
                {
                    logger.Verbose($"Delete asset...\n({path})");
                    importedAssets.Remove(path);
                    return true;
                }

                logger.Verbose($"Delete asset... (fail, missing)\n({path})");
                return false;
            }

            public string MoveAsset(string src, string dst)
            {
                if (importedAssets.Contains(src))
                {
                    logger.Verbose($"Move asset from {src} to {dst}");
                    importedAssets.Remove(src);
                    importedAssets.Add(src);
                    return "";
                }

                logger.Verbose($"Move asset from {src} to {dst} (fail)");
                return "Error";
            }

            public void ReleaseCachedFileHandles()
            {
            }

            public T LoadAssetAtPath<T>(string path) where T : Object
            {
                logger.Verbose($"LoadAssetAtPath {path}");

                if (file.Exists(path))
                {
                    if (typeof(T) == typeof(Texture2D))
                    {
                        return Texture2D.whiteTexture as T;
                    }

                    return new Object() as T;
                }

                return null;
            }

            public string GetAssetPath(Object asset)
            {
                return "";
            }

            public string AssetPathToGUID(string path)
            {
                return "";
            }

            public string GetTextMetaFilePathFromAssetPath(string path)
            {
                return Path.ChangeExtension(path, ".meta");
            }

            public AssetImporter GetImporterAtPath(string path)
            {
                return null;
            }
        }
    }
}