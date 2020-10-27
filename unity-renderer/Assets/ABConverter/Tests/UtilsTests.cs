using DCL;
using NUnit.Framework;
using System.IO;
using UnityEngine;

namespace DCL.ABConverter
{
    public class UtilsTests
    {
        [Test]
        public void CIDtoGuidTest()
        {
            Assert.AreEqual("d3b55cc7e3367537c1670ecebb1ccb05", DCL.ABConverter.Utils.CidToGuid("QmWVcyTEzSEBKC7hzq6doiTWnopZ6DdqJMqufx6gXwFnTS"));
        }

        [Test]
        [TestCase("..|FenceStoneLarge_01|file1.png", "models/Fountain_01/Fountain_01.glb", "models/FenceStoneLarge_01/file1.png")]
        [TestCase("file1.png", "models/Fountain_01/Fountain_01.glb", "models/Fountain_01/file1.png")]
        [TestCase("..|LampPost_01|file1.png", "models/Fountain_01/Fountain_01.glb", "models/LampPost_01/file1.png")]
        [TestCase("..|FenceStonePillarTall_01|file1.png", "models/Fountain_01/Fountain_01.glb", "models/FenceStonePillarTall_01/file1.png")]
        [TestCase("..|Grass_02|file1.png", "models/Fountain_01/Fountain_01.glb", "models/Grass_02/file1.png")]
        [TestCase("..|FloorBaseGrass_01|Floor_Grass01.png.png", "models/Fountain_01/Fountain_01.glb", "models/FloorBaseGrass_01/Floor_Grass01.png.png")]
        [TestCase("..|FenceStonePillar_01|file1.png", "models/Fountain_01/Fountain_01.glb", "models/FenceStonePillar_01/file1.png")]
        public void GetRelativePathToTest(string expected, string from, string to)
        {
            expected = expected.Replace('|', Path.DirectorySeparatorChar);
            Assert.AreEqual(expected, ABConverter.PathUtils.GetRelativePathTo(from, to));
        }

        [Test]
        [TestCase("C:/MyProject/Assets/MyAsset.png", "/Assets/MyAsset.png")]
        [TestCase("C:/MyProject/Assets/Blah/MyAsset.glb", "/Assets/Blah/MyAsset.glb")]
        [TestCase("C:/MyProject/Assets/Blah/My Asset With Spaces.long", "/Assets/Blah/My Asset With Spaces.long")]
        [TestCase("C:/MyProject", "")]
        public void AssetPathToFullPathTest(string expected, string input)
        {
            expected = ABConverter.PathUtils.FixDirectorySeparator(expected);
            string result = ABConverter.PathUtils.AssetPathToFullPath(input, $"C:/MyProject/Assets");
            UnityEngine.Assertions.Assert.AreEqual(expected, result);
        }

        [Test]
        [TestCase("C:/MyProjects/Assets/MyAsset.png", "/Assets/MyAsset.png")]
        [TestCase("C:/MyProjects/Assets/Blah/MyAsset.glb", "/Assets/Blah/MyAsset.glb")]
        [TestCase("C:/MyProjects/Assets/Blah/My Asset With Spaces.long", "/Assets/Blah/My Asset With Spaces.long")]
        [TestCase("", "")]
        public void FullPathToAssetPath(string input, string expected)
        {
            expected = ABConverter.PathUtils.FixDirectorySeparator(expected);
            string result = ABConverter.PathUtils.FullPathToAssetPath(input);
            UnityEngine.Assertions.Assert.AreEqual(expected, result);
        }
    }
}