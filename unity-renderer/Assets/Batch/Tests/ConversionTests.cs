using DCL;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace AssetBundleConversionTests
{
    public class ConversionTests
    {
        [Test]
        public void PopulateLowercaseMappingsWorkCorrectly()
        {
            var builder = new AssetBundleBuilder();
            var pairs = new List<ContentServerUtils.MappingPair>();

            pairs.Add(new ContentServerUtils.MappingPair() { file = "foo", hash = "tEsT1" });
            pairs.Add(new ContentServerUtils.MappingPair() { file = "foo", hash = "Test2" });
            pairs.Add(new ContentServerUtils.MappingPair() { file = "foo", hash = "tesT3" });
            pairs.Add(new ContentServerUtils.MappingPair() { file = "foo", hash = "teSt4" });

            builder.PopulateLowercaseMappings(pairs.ToArray());

            Assert.IsTrue(builder.hashLowercaseToHashProper.ContainsKey("test1"));
            Assert.IsTrue(builder.hashLowercaseToHashProper.ContainsKey("test2"));
            Assert.IsTrue(builder.hashLowercaseToHashProper.ContainsKey("test3"));
            Assert.IsTrue(builder.hashLowercaseToHashProper.ContainsKey("test4"));

            Assert.AreEqual("tEsT1", builder.hashLowercaseToHashProper["test1"]);
            Assert.AreEqual("Test2", builder.hashLowercaseToHashProper["test2"]);
            Assert.AreEqual("tesT3", builder.hashLowercaseToHashProper["test3"]);
            Assert.AreEqual("teSt4", builder.hashLowercaseToHashProper["test4"]);
        }

        [Test]
        public void InitializeDirectoryPathsWorkCorrectly()
        {
            var builder = new AssetBundleBuilder();
            builder.InitializeDirectoryPaths(false);

            Assert.IsFalse(string.IsNullOrEmpty(builder.finalAssetBundlePath));
            Assert.IsFalse(string.IsNullOrEmpty(builder.finalDownloadedPath));

            Assert.IsTrue(Directory.Exists(builder.finalAssetBundlePath));
            Assert.IsTrue(Directory.Exists(builder.finalDownloadedPath));

            string file1 = builder.finalAssetBundlePath + "test.txt";
            string file2 = builder.finalDownloadedPath + "test.txt";

            File.WriteAllText(file1, "test");
            File.WriteAllText(file2, "test");

            builder.InitializeDirectoryPaths(true);

            Assert.IsFalse(File.Exists(file1));
            Assert.IsFalse(File.Exists(file2));
        }

        [UnityTest]
        public IEnumerator WhenConvertedWithExternalTexturesDependenciesAreGeneratedCorrectly()
        {
            Caching.ClearCache();

            if (Directory.Exists(AssetBundleBuilderConfig.ASSET_BUNDLES_PATH_ROOT))
                Directory.Delete(AssetBundleBuilderConfig.ASSET_BUNDLES_PATH_ROOT, true);

            if (Directory.Exists(AssetBundleBuilderConfig.DOWNLOADED_PATH_ROOT))
                Directory.Delete(AssetBundleBuilderConfig.DOWNLOADED_PATH_ROOT, true);

            AssetDatabase.Refresh();

            var builder = new AssetBundleBuilder();
            bool finished = false;

            System.Action<AssetBundleBuilder.ErrorCodes> onFinish = (x) => { finished = true; };

            builder.DumpArea(new Vector2Int(-110, -110), new Vector2Int(1, 1), onFinish);

            yield return new WaitUntil(() => finished == true);

            AssetBundle abDependency = AssetBundle.LoadFromFile(AssetBundleBuilderConfig.ASSET_BUNDLES_PATH_ROOT + "/QmYACL8SnbXEonXQeRHdWYbfm8vxvaFAWnsLHUaDG4ABp5");
            abDependency.LoadAllAssets();

            AssetBundle abMain = AssetBundle.LoadFromFile(AssetBundleBuilderConfig.ASSET_BUNDLES_PATH_ROOT + "/QmNS4K7GaH63T9rhAfkrra7ADLXSEeco8FTGknkPnAVmKM");
            Material[] mats = abMain.LoadAllAssets<Material>();

            bool hasMap = false;

            foreach (var mat in mats)
            {
                if (mat.name.ToLowerInvariant().Contains("mini town"))
                    hasMap = mat.GetTexture("_BaseMap") != null;
            }

            abMain.Unload(true);
            abDependency.Unload(true);

            Assert.IsTrue(hasMap, "Dependency has NOT been generated correctly!");
        }
    }
}
