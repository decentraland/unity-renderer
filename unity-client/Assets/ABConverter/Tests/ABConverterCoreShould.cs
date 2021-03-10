using DCL;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using DCL.Helpers;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityGLTF.Cache;

namespace DCL.ABConverter.Tests
{
    public class ABConverterShould
    {
        [UnityTest]
        public IEnumerator DumpAreaCorrectly()
        {
            //TODO(Brian): Implement later 
            yield break;
        }

        [UnityTest]
        public IEnumerator DumpSceneCorrectly()
        {
            //TODO(Brian): Implement later 
            yield break;
        }
    }

    public class ABConverterCoreShould
    {
        private ABConverter.Core core;
        private ABConverter.Environment env;

        string basePath = @"C:\test-path\";
        string hash1 = "QmHash1", hash2 = "QmHash2", hash3 = "QmHash3", hash4 = "QmHash4";
        string baseUrl = @"https://peer.decentraland.org/lambdas/contentv2/contents/";
        private string contentData1 = "TestContent1 - guid: 14e357df3f3b75940b5d59e1035255b1\n";
        private string contentData2 = "TestContent2 - guid: 14e357df3f3b75940b5d59e1035255b2\n";
        private string contentData3 = "TestContent3 - guid: 14e357df3f3b75940b5d59e1035255b3\n";

        [SetUp]
        public void SetUp()
        {
            ResetCacheAndWorkingFolders();

            var settings = new ABConverter.Client.Settings(ContentServerUtils.ApiTLD.ZONE);
            settings.deleteDownloadPathAfterFinished = false;

            env = ABConverter.Environment.CreateWithMockImplementations();
            core = new ABConverter.Core(env, settings);

            if (env.webRequest is Mocked.WebRequest mockedReq)
            {
                mockedReq.mockedContent.Add($"{baseUrl}{hash1}", contentData1);
                mockedReq.mockedContent.Add($"{baseUrl}{hash2}", contentData2);
                mockedReq.mockedContent.Add($"{baseUrl}{hash3}", contentData3);
            }
        }

        [TearDown]
        public void TearDown()
        {
            ResetCacheAndWorkingFolders();
        }

        [Test]
        public void PopulateLowercaseMappingsCorrectly()
        {
            var pairs = new List<ContentServerUtils.MappingPair>();

            pairs.Add(new ContentServerUtils.MappingPair() {file = "foo", hash = "tEsT1"});
            pairs.Add(new ContentServerUtils.MappingPair() {file = "foo", hash = "Test2"});
            pairs.Add(new ContentServerUtils.MappingPair() {file = "foo", hash = "tesT3"});
            pairs.Add(new ContentServerUtils.MappingPair() {file = "foo", hash = "teSt4"});

            core.PopulateLowercaseMappings(pairs.ToArray());

            Assert.IsTrue(core.hashLowercaseToHashProper.ContainsKey("test1"));
            Assert.IsTrue(core.hashLowercaseToHashProper.ContainsKey("test2"));
            Assert.IsTrue(core.hashLowercaseToHashProper.ContainsKey("test3"));
            Assert.IsTrue(core.hashLowercaseToHashProper.ContainsKey("test4"));

            Assert.AreEqual("tEsT1", core.hashLowercaseToHashProper["test1"]);
            Assert.AreEqual("Test2", core.hashLowercaseToHashProper["test2"]);
            Assert.AreEqual("tesT3", core.hashLowercaseToHashProper["test3"]);
            Assert.AreEqual("teSt4", core.hashLowercaseToHashProper["test4"]);
        }

        [Test]
        public void InitializeDirectoryPathsCorrectly()
        {
            var settings = new ABConverter.Client.Settings(ContentServerUtils.ApiTLD.ZONE);
            settings.deleteDownloadPathAfterFinished = false;

            env = ABConverter.Environment.CreateWithDefaultImplementations();
            core = new ABConverter.Core(env, settings);

            core.InitializeDirectoryPaths(false);

            Assert.IsFalse(string.IsNullOrEmpty(core.settings.finalAssetBundlePath));
            Assert.IsFalse(string.IsNullOrEmpty(core.finalDownloadedPath));

            Assert.IsTrue(env.directory.Exists(core.settings.finalAssetBundlePath));
            Assert.IsTrue(env.directory.Exists(core.finalDownloadedPath));

            string file1 = core.settings.finalAssetBundlePath + "test.txt";
            string file2 = core.finalDownloadedPath + "test.txt";

            env.file.WriteAllText(file1, "test");
            env.file.WriteAllText(file2, "test");

            core.InitializeDirectoryPaths(true);

            Assert.IsFalse(env.file.Exists(file1));
            Assert.IsFalse(env.file.Exists(file2));
        }

        [Test]
        public void InjectTexturesCorrectly()
        {
            AssetPath gltfPath = new AssetPath(core.finalDownloadedPath, "MyHash", "model/myModel.gltf");
            AssetPath texturePath = new AssetPath(core.finalDownloadedPath, "MyHash2", "model/texture.png");
            AssetPath texturePath2 = new AssetPath(core.finalDownloadedPath, "MyHash3", "model/invalid-texture.png");

            PersistentAssetCache.ImageCacheByUri.Clear();
            PersistentAssetCache.StreamCacheByUri.Clear();

            core.RetrieveAndInjectTexture(gltfPath, texturePath);

            string content1 = "Test";
            env.file.WriteAllText(texturePath.finalPath, content1);

            core.RetrieveAndInjectTexture(gltfPath, texturePath);
            core.RetrieveAndInjectTexture(gltfPath, texturePath2);

            string id1 = $"texture.png@{gltfPath.finalPath}";
            string id2 = $"invalid-texture.png@{gltfPath.finalPath}";

            //NOTE(Brian): Check if streams exists and are added correctly
            Assert.IsTrue(PersistentAssetCache.HasImage(id1), $"id1:{id1} doesn't exist?");
            Assert.IsFalse(PersistentAssetCache.HasImage(id2), $"Second file with {id2} shouldn't be injected because it doesn't exist!");

            Assert.IsNotNull(PersistentAssetCache.GetImage(id1), "First image don't exist!");

            //NOTE(Brian): Read image and validate content
            var image1 = PersistentAssetCache.GetImage(id1);
        }

        [Test]
        public void InjectBuffersCorrectly()
        {
            AssetPath gltfPath = new AssetPath(core.finalDownloadedPath, "MyHash", "models/myModel.gltf");
            AssetPath bufferPath = new AssetPath(core.finalDownloadedPath, "MyHash2", "models/anims/anim1.bin");
            AssetPath bufferPath2 = new AssetPath(core.finalDownloadedPath, "MyHash3", "models/misc.bin");
            AssetPath bufferPath3 = new AssetPath(core.finalDownloadedPath, "MyHash4", "missing-file.bin");

            PersistentAssetCache.ImageCacheByUri.Clear();
            PersistentAssetCache.StreamCacheByUri.Clear();

            string content1 = "Test";
            string content2 = "Test2";

            env.file.WriteAllText(bufferPath.finalPath, content1);
            env.file.WriteAllText(bufferPath2.finalPath, content2);

            core.RetrieveAndInjectBuffer(gltfPath, bufferPath);
            core.RetrieveAndInjectBuffer(gltfPath, bufferPath2);
            core.RetrieveAndInjectBuffer(gltfPath, bufferPath3);

            char ds = Path.DirectorySeparatorChar;

            string id1 = $"anims{ds}anim1.bin@{gltfPath.finalPath}";
            string id2 = $"misc.bin@{gltfPath.finalPath}";
            string id3 = $"..{ds}missing-file.bin@{gltfPath.finalPath}";

            //NOTE(Brian): Check if streams exists and are added correctly
            Assert.IsTrue(PersistentAssetCache.HasBuffer(id1));
            Assert.IsTrue(PersistentAssetCache.HasBuffer(id2));
            Assert.IsFalse(PersistentAssetCache.HasBuffer(id3), "Third file shouldn't be injected because it doesn't exist!");

            Assert.IsNotNull(PersistentAssetCache.GetBuffer(id1), "First stream don't exist!");
            Assert.IsNotNull(PersistentAssetCache.GetBuffer(id2), "Second stream don't exist!");

            //NOTE(Brian): Read stream and validate content
            var buffer1 = PersistentAssetCache.GetBuffer(id1);
            var buffer2 = PersistentAssetCache.GetBuffer(id2);

            byte[] chars = new byte[100];
            buffer1.stream.Read(chars, 0, 100);
            string bufferText = UTF8Encoding.UTF8.GetString(chars).TrimEnd('\0');
            Assert.AreEqual(content1, bufferText, "First stream has invalid content!");

            buffer2.stream.Read(chars, 0, 100);
            bufferText = UTF8Encoding.UTF8.GetString(chars).TrimEnd('\0');
            Assert.AreEqual(content2, bufferText, "Second stream has invalid content!");

            buffer1.stream.Dispose();
            buffer2.stream.Dispose();
        }

        [Test]
        public void DumpGLTFSucceedsCorrectly()
        {
            List<AssetPath> texturePaths = new List<AssetPath>();
            List<AssetPath> bufferPaths = new List<AssetPath>();

            AssetPath gltfPath = new AssetPath(basePath, hash1, "test.gltf");

            var output = core.DumpGltf(gltfPath, texturePaths, bufferPaths);

            Assert.IsNotNull(output);
        }

        [Test]
        public void DumpGLTFFailsCorrectly()
        {
            List<AssetPath> texturePaths = new List<AssetPath>();
            List<AssetPath> bufferPaths = new List<AssetPath>();

            AssetPath gltfPath = new AssetPath(basePath, "QmNonExistentHash", "test.gltf");

            var output = core.DumpGltf(gltfPath, texturePaths, bufferPaths);

            LogAssert.Expect(LogType.Error, new Regex("^.*?Download failed!"));
            Assert.IsNull(output);
        }

        [Test]
        public void DumpImportableAssetsCorrectly()
        {
            List<AssetPath> paths = new List<AssetPath>();

            string[] files = {"file1.png", "file2.png", "file3.png", "file4.png"};

            paths.Add(new AssetPath(basePath, hash1, files[0]));
            paths.Add(new AssetPath(basePath, hash2, files[1]));
            paths.Add(new AssetPath(basePath, hash3, files[2]));
            paths.Add(new AssetPath(basePath, hash4, files[3]));

            string targetGuid1 = ABConverter.Utils.CidToGuid(hash1);
            string targetGuid2 = ABConverter.Utils.CidToGuid(hash2);
            string targetGuid3 = ABConverter.Utils.CidToGuid(hash3);

            var textures = core.DumpImportableAssets(paths);

            LogAssert.Expect(LogType.Error, new Regex(@"^.*?Download failed"));
            LogAssert.Expect(LogType.Error, new Regex(@"^.*?QmHash4"));

            Assert.AreEqual(3, textures.Count);

            //NOTE(Brian): textures exist?
            Assert.IsTrue(env.file.Exists(paths[0].finalPath));
            Assert.IsTrue(env.file.Exists(paths[1].finalPath));
            Assert.IsTrue(env.file.Exists(paths[2].finalPath));
            Assert.IsFalse(env.file.Exists(paths[3].finalPath));

            //NOTE(Brian): textures .meta exist?
            Assert.IsTrue(env.file.Exists(paths[0].finalMetaPath));
            Assert.IsTrue(env.file.Exists(paths[1].finalMetaPath));
            Assert.IsTrue(env.file.Exists(paths[2].finalMetaPath));
            Assert.IsFalse(env.file.Exists(paths[3].finalMetaPath));

            //NOTE(Brian): textures .meta guid is changed?
            Assert.IsTrue(env.file.ReadAllText(paths[0].finalMetaPath).Contains(targetGuid1));
            Assert.IsTrue(env.file.ReadAllText(paths[1].finalMetaPath).Contains(targetGuid2));
            Assert.IsTrue(env.file.ReadAllText(paths[2].finalMetaPath).Contains(targetGuid3));
        }

        [Test]
        public void DumpRawAssetsCorrectly()
        {
            List<AssetPath> paths = new List<AssetPath>();

            string[] files = {"file1.bin", "file2.bin", "file3.bin", "file4.bin"};

            paths.Add(new AssetPath(basePath, hash1, files[0]));
            paths.Add(new AssetPath(basePath, hash2, files[1]));
            paths.Add(new AssetPath(basePath, hash3, files[2]));
            paths.Add(new AssetPath(basePath, hash4, files[3]));

            var buffers = core.DumpRawAssets(paths);

            LogAssert.Expect(LogType.Error, new Regex(@"^.*?Download failed"));
            LogAssert.Expect(LogType.Error, new Regex(@"^.*?QmHash4"));

            Assert.AreEqual(3, buffers.Count);

            //NOTE(Brian): textures exist?
            Assert.IsTrue(env.file.Exists(paths[0].finalPath));
            Assert.IsTrue(env.file.Exists(paths[1].finalPath));
            Assert.IsTrue(env.file.Exists(paths[2].finalPath));
            Assert.IsFalse(env.file.Exists(paths[3].finalPath));

            //NOTE(Brian): textures .meta exist?
            Assert.IsTrue(env.file.Exists(paths[0].finalMetaPath));
            Assert.IsTrue(env.file.Exists(paths[1].finalMetaPath));
            Assert.IsTrue(env.file.Exists(paths[2].finalMetaPath));
            Assert.IsFalse(env.file.Exists(paths[3].finalMetaPath));
        }


        [Test]
        public void DownloadAssetCorrectly()
        {
            AssetPath path = new AssetPath(
                basePath: basePath,
                hash: hash1,
                file: "texture.png"
            );

            string output = core.DownloadAsset(path);

            UnityEngine.Assertions.Assert.IsTrue(env.file.Exists(path.finalPath));
            UnityEngine.Assertions.Assert.IsTrue(env.file.Exists(path.finalMetaPath));
            UnityEngine.Assertions.Assert.AreEqual(contentData1, env.file.ReadAllText(output));
        }

        [UnityTest]
        public IEnumerator ConvertAssetsWithExternalTextures()
        {
            ContentServerUtils.MappingPair[] input =
            {
                new ContentServerUtils.MappingPair {file = "SimpleCubeWithSharedNormal.gltf", hash = "SimpleCubeWithSharedNormal.gltf"},
                new ContentServerUtils.MappingPair {file = "SimpleCubeWithSharedNormal.bin", hash = "SimpleCubeWithSharedNormal.bin"},
                new ContentServerUtils.MappingPair {file = "Textures/Test.png", hash = "Test.png"}
            };

            core.settings.baseUrl = DCL.Helpers.Utils.GetTestsAssetsPath() + "/GLTF/SimpleCube/";

            env = ABConverter.Environment.CreateWithDefaultImplementations();
            core = new ABConverter.Core(env, core.settings);

            core.Convert(input);

            yield return new WaitUntil(() => core.state.step == ABConverter.Core.State.Step.FINISHED);

            Assert.IsTrue(core.state.lastErrorCode == ABConverter.Core.ErrorCodes.SUCCESS);

            AssetBundle abDependency = AssetBundle.LoadFromFile(ABConverter.Config.ASSET_BUNDLES_PATH_ROOT + "/Test.png");
            abDependency.LoadAllAssets();

            AssetBundle abMain = AssetBundle.LoadFromFile(ABConverter.Config.ASSET_BUNDLES_PATH_ROOT + "/SimpleCubeWithSharedNormal.gltf");
            Material[] mats = abMain.LoadAllAssets<Material>();

            bool hasMap = false;

            foreach (var mat in mats)
            {
                hasMap = mat.GetTexture("_BaseMap") != null;
            }

            abMain.Unload(true);
            abDependency.Unload(true);

            Assert.IsTrue(hasMap, "Dependency has NOT been generated correctly!");
        }

        [UnityTest]
        public IEnumerator NotGenerateColorMapsWithDXTnm()
        {
            ContentServerUtils.MappingPair[] input =
            {
                new ContentServerUtils.MappingPair {file = "SimpleCubeWithSharedNormal.gltf", hash = "SimpleCubeWithSharedNormal.gltf"},
                new ContentServerUtils.MappingPair {file = "SimpleCubeWithSharedNormal.bin", hash = "SimpleCubeWithSharedNormal.bin"},
                new ContentServerUtils.MappingPair {file = "Textures/Test.png", hash = "Test.png"}
            };

            core.settings.baseUrl = DCL.Helpers.Utils.GetTestsAssetsPath() + "/GLTF/SimpleCube/";
            core.settings.verbose = true;
            core.settings.dumpOnly = true;
            core.settings.deleteDownloadPathAfterFinished = false;

            env = ABConverter.Environment.CreateWithDefaultImplementations();
            core = new ABConverter.Core(env, core.settings);

            core.Convert(input);

            yield return new WaitUntil(() => core.state.step == ABConverter.Core.State.Step.FINISHED);

            Assert.IsTrue(core.state.lastErrorCode == ABConverter.Core.ErrorCodes.SUCCESS);

            string importerPath = $"{core.finalDownloadedPath}Test.png{ABConverter.Config.DASH}Test.png.png";
            TextureImporter importer = env.assetDatabase.GetImporterAtPath(importerPath) as TextureImporter;

            Assert.IsTrue(importer != null, "Texture importer is null!");
            Assert.IsTrue(TextureImporterType.NormalMap != importer.textureType, "Texture is used for color! It shouldn't be never importer as normal map!");
        }

        [UnityTest]
        public IEnumerator NotFailIfExternalTexturesAreMissing()
        {
            ContentServerUtils.MappingPair[] input =
            {
                new ContentServerUtils.MappingPair {file = "SimpleCubeWithSharedNormal.gltf", hash = "SimpleCubeWithSharedNormal.gltf"},
                new ContentServerUtils.MappingPair {file = "SimpleCubeWithSharedNormal.bin", hash = "SimpleCubeWithSharedNormal.bin"},
            };

            core.settings.baseUrl = DCL.Helpers.Utils.GetTestsAssetsPath() + "/GLTF/SimpleCube/";

            env = ABConverter.Environment.CreateWithDefaultImplementations();
            core = new ABConverter.Core(env, core.settings);

            core.Convert(input);

            yield return new WaitUntil(() => core.state.step == ABConverter.Core.State.Step.FINISHED);

            Assert.IsTrue(core.state.lastErrorCode == ABConverter.Core.ErrorCodes.SUCCESS);
            LogAssert.Expect(LogType.Error, new Regex(@"^.*?Buffer file not found"));
            LogAssert.Expect(LogType.Error, new Regex(@"^.*?Buffer file not found"));
        }


        void ResetCacheAndWorkingFolders()
        {
            Caching.ClearCache();

            if (Directory.Exists(ABConverter.Config.ASSET_BUNDLES_PATH_ROOT))
                Directory.Delete(ABConverter.Config.ASSET_BUNDLES_PATH_ROOT, true);

            if (Directory.Exists(ABConverter.Config.DOWNLOADED_PATH_ROOT))
                Directory.Delete(ABConverter.Config.DOWNLOADED_PATH_ROOT, true);

            if (File.Exists(ABConverter.Config.DOWNLOADED_PATH_ROOT + ".meta"))
                File.Delete(ABConverter.Config.DOWNLOADED_PATH_ROOT + ".meta");

            UnityEditor.AssetDatabase.Refresh();
        }
    }
}