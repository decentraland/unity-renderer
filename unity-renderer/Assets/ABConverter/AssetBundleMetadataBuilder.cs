using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("AssetBundleBuilderEditorTests")]

namespace DCL.ABConverter
{
    public static class AssetBundleMetadataBuilder
    {
        /// <summary>
        /// Creates the asset bundle metadata file (dependencies, version, timestamp) 
        /// </summary>
        public static void Generate(IFile file, string path, Dictionary<string, string> hashLowercaseToHashProper, AssetBundleManifest manifest, string version = "1.0", string exceptions = null)
        {
            string[] assetBundles = manifest.GetAllAssetBundles();

            for (int i = 0; i < assetBundles.Length; i++)
            {
                if (string.IsNullOrEmpty(assetBundles[i]))
                    continue;

                var metadata = new AssetBundleMetadata { version = version, timestamp = DateTime.UtcNow.Ticks };
                string[] deps = manifest.GetAllDependencies(assetBundles[i]);

                if (deps.Length > 0)
                {
                    deps = deps.Where(s => s != exceptions).ToArray();

                    metadata.dependencies = deps.Select((x) =>
                                              {
                                                  if (hashLowercaseToHashProper.ContainsKey(x))
                                                      return hashLowercaseToHashProper[x];
                                                  else
                                                      return x;
                                              })
                                              .ToArray();
                }

                string json = JsonUtility.ToJson(metadata);
                string assetHashName = assetBundles[i];

                hashLowercaseToHashProper.TryGetValue(assetBundles[i], out assetHashName);

                if (!string.IsNullOrEmpty(assetHashName))
                {
                    file.WriteAllText(path + $"/{assetHashName}/metadata.json", json);
                }
            }
        }
    }
}