using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Rendering.LoadingAvatar.Editor
{
    [TestFixture]
    [Category("EditModeCI")]
    public class BaseAvatarTest
    {
        private const string assetPath = "Assets/Rendering/LoadingAvatar/CrossSection/Avatar_Male_SingleMesh.glb";

        [Test]
        public void BaseAvatarIsImportedCorrectly()
        {
            GameObject importedGameObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            Assert.IsNotNull(importedGameObject);
        }
    }
}
