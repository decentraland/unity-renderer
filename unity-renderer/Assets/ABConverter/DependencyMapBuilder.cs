using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("AssetBundleBuilderEditorTests")]

namespace DCL.ABConverter
{
    public static class DependencyMapBuilder
    {
        [System.Serializable]
        public class AssetDependencyMap
        {
            public string[] dependencies;
        }

        /// <summary>
        /// This dumps .depmap files
        /// </summary>
        /// <param name="manifest"></param>
        public static void Generate(IFile file, string path, Dictionary<string, string> hashLowercaseToHashProper, AssetBundleManifest manifest, string exceptions = null)
        {
            string[] assetBundles = manifest.GetAllAssetBundles();

            for (int i = 0; i < assetBundles.Length; i++)
            {
                if (string.IsNullOrEmpty(assetBundles[i]))
                    continue;

                var depMap = new AssetDependencyMap();
                string[] deps = manifest.GetAllDependencies(assetBundles[i]);

                if (deps.Length > 0)
                {
                    deps = deps.Where(s => s != exceptions).ToArray();

                    depMap.dependencies = deps.Select((x) =>
                    {
                        if (hashLowercaseToHashProper.ContainsKey(x))
                            return hashLowercaseToHashProper[x];
                        else
                            return x;
                    }).ToArray();
                }

                string json = JsonUtility.ToJson(depMap);
                string finalFilename = assetBundles[i];

                hashLowercaseToHashProper.TryGetValue(assetBundles[i], out finalFilename);

                if (!string.IsNullOrEmpty(finalFilename))
                {
                    file.WriteAllText(path + finalFilename + ".depmap", json);
                }
            }
        }
    }
}